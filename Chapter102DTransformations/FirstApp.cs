namespace Chapter102DTransformations;

public class FirstApp : IDisposable
{
    // Window stuff
    private IView window = null!;
    private int width = 800;
    private int height = 600;
    private string windowName = "Vulkan Tut";
    private bool framebufferResized = false;
    private long fpsUpdateInterval = 200 * 10_000;
    private long fpsLastUpdate;

    //private long frame = 0;

    // Vk api
    private readonly Vk vk = null!;


    private LveDevice device = null!;
    private LvePipeline pipeline = null!;
    private LveSwapChain swapChain = null!;
    //private LveModel model = null!;
    private List<LveGameObject> gameObjects = new();

    private PipelineLayout pipelineLayout;
    private CommandBuffer[] commandBuffers = null!;

    private bool disposedValue;

    //[StructLayout(LayoutKind.Sequential, Pack = 16)] this doesn't work?  i'm missing something
    // will 
    public struct SimplePushConstantData
    {
        public Matrix2X2<float> Transform;
        public Vector4 Offset;
        public Vector4 Color;

        public SimplePushConstantData()
        {
            Transform = Matrix2X2<float>.Identity;
        }

        public static uint SizeOf() => (uint)Unsafe.SizeOf<SimplePushConstantData>();
    }

    public FirstApp()
    {
        log.RestartTimer();
        log.d("app run", "starting Run");

        vk = Vk.GetApi();
        log.d("app run", "got vk");

        initWindow();
        log.d("app run", "got window");

        device = new LveDevice(vk, window);
        log.d("app run", "got device");

        loadGameObjects();
        createPipelineLayout();
        recreateSwapChain();
        createCommandBuffers();
    }

    public void Run()
    {
        MainLoop();
        CleanUp();
    }


    private void renderGameObjects(CommandBuffer commandBuffer)
    {
        pipeline.Bind(commandBuffer);


        foreach (var g in gameObjects)
        {
            g.Transform2d = g.Transform2d with
            {
                Rotation = g.Transform2d.Rotation + .0001f * MathF.Tau
            };
            SimplePushConstantData push = new()
            {
                Offset = new(g.Transform2d.Translation, 0.0f, 0.0f),
                Color = g.Color,
                Transform = g.Transform2d.Mat2()
            };
            vk.CmdPushConstants(commandBuffer, pipelineLayout, ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit, 0, SimplePushConstantData.SizeOf(), ref push);
            g.Model.Bind(commandBuffer);
            g.Model.Draw(commandBuffer);

        }
    }


    private void drawFrame(double delta)
    {
        uint imageIndex = 0;
        var result = swapChain.AcquireNextImage(ref imageIndex);

        if (result == Result.ErrorOutOfDateKhr)
        {
            recreateSwapChain();
            return;
        }
        else if (result != Result.Success && result != Result.SuboptimalKhr)
        {
            throw new Exception("failed to acquire next swapchain image");
        }

        recordCommandBuffer(imageIndex);
        result = swapChain.SubmitCommandBuffers(commandBuffers[imageIndex], imageIndex);
        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || framebufferResized)
        {
            framebufferResized = false;
            recreateSwapChain();
            return;
        }

        else if (result != Result.Success)
        {
            throw new Exception("failed to submit command buffers");
        }

    }


    private void resize(Vector2D<int> newsize)
    {

    }

    private void MainLoop()
    {
        window.Run();

        vk.DeviceWaitIdle(device.VkDevice);
    }

    private void CleanUp()
    {
        window.Dispose();
    }

    private void initWindow()
    {
        //Create a window.
        var options = WindowOptions.DefaultVulkan with
        {
            Size = new Vector2D<int>(width, height),
            Title = windowName
        };

        window = Window.Create(options);
        window.Initialize();

        if (window.VkSurface is null)
        {
            throw new Exception("Windowing platform doesn't support Vulkan.");
        }

        fpsLastUpdate = DateTime.Now.Ticks;

        window.Render += drawFrame;
        window.FramebufferResize += resize;
        window.Update += updateWindow;
    }

    private void updateWindow(double frametime)
    {

        if (DateTime.Now.Ticks - fpsLastUpdate < fpsUpdateInterval) return;

        fpsLastUpdate = DateTime.Now.Ticks;
        if (window is IWindow w)
        {
            //w.Title = $"{windowName} | W {window.Size.X}x{window.Size.Y} | FPS {Math.Ceiling(1d / obj)} | ";
            w.Title = $"{windowName} - {1d / frametime,-8: #,##0.0} fps";
        }

    }

    private Extent2D GetWindowExtents()
    {
        return new Extent2D((uint)window.FramebufferSize.X, (uint)window.FramebufferSize.Y);
    }


    private void recreateSwapChain()
    {
        var frameBufferSize = window.FramebufferSize;
        while (frameBufferSize.X == 0 || frameBufferSize.Y == 0)
        {
            frameBufferSize = window.FramebufferSize;
            window.DoEvents();
        }
        vk.DeviceWaitIdle(device.VkDevice);


        if (swapChain is null)
        {
            swapChain = new LveSwapChain(vk, device, GetWindowExtents());
        }
        else
        {
            swapChain = new LveSwapChain(vk, device, GetWindowExtents(), swapChain);
            if (swapChain.GetFrameBufferCount() != commandBuffers.Length)
            {
                freeCommandBuffers();
                createCommandBuffers();
            }
        }


        createPipeline();
    }


    private void createPipeline()
    {
        var pipelineConfig = new PipelineConfigInfo();
        LvePipeline.DefaultPipelineConfigInfo(ref pipelineConfig);

        pipelineConfig.RenderPass = swapChain.GetRenderPass();
        pipelineConfig.PipelineLayout = pipelineLayout;
        pipeline = new LvePipeline(
            vk, device,
            "simpleShader.vert.spv", "simpleShader.frag.spv",
            pipelineConfig
            );
        log.d("app run", " got pipeline");
    }

    private unsafe void createCommandBuffers()
    {
        commandBuffers = new CommandBuffer[swapChain.ImageCount()];

        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = device.GetCommandPool(),
            CommandBufferCount = (uint)commandBuffers.Length,
        };

        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            if (vk!.AllocateCommandBuffers(device.VkDevice, allocInfo, commandBuffersPtr) != Result.Success)
            {
                throw new Exception("failed to allocate command buffers!");
            }
        }
    }


    private unsafe void freeCommandBuffers()
    {
        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            vk.FreeCommandBuffers(device.VkDevice, device.GetCommandPool(), (uint)commandBuffers.Length, commandBuffersPtr);
        }
        Array.Clear(commandBuffers);
    }







    /// <summary>
    /// 
    /// </summary>
    /// <param name="imageIndex"></param>
    /// <exception cref="Exception"></exception>
    private unsafe void recordCommandBuffer(uint imageIndex)
    {
        //frame = (frame + 1) % 3000;


        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
        };

        if (vk.BeginCommandBuffer(commandBuffers[imageIndex], beginInfo) != Result.Success)
        {
            throw new Exception("failed to begin recording command buffer!");
        }

        RenderPassBeginInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = swapChain.GetRenderPass(),
            Framebuffer = swapChain.GetFrameBufferAt(imageIndex),
            RenderArea =
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = swapChain.GetSwapChainExtent(),
                }
        };

        var clearValues = new ClearValue[]
        {
                new()
                {
                    Color = new (){ Float32_0 = 0.01f, Float32_1 = 0.01f, Float32_2 = 0.01f, Float32_3 = 1 },
                },
                new()
                {
                    DepthStencil = new () { Depth = 1, Stencil = 0 }
                }
        };


        fixed (ClearValue* clearValuesPtr = clearValues)
        {
            renderPassInfo.ClearValueCount = (uint)clearValues.Length;
            renderPassInfo.PClearValues = clearValuesPtr;

            vk.CmdBeginRenderPass(commandBuffers[imageIndex], &renderPassInfo, SubpassContents.Inline);
        }


        Viewport viewport = new()
        {
            X = 0.0f,
            Y = 0.0f,
            Width = swapChain.GetSwapChainExtent().Width,
            Height = swapChain.GetSwapChainExtent().Height,
            MinDepth = 0.0f,
            MaxDepth = 1.0f,
        };
        Rect2D scissor = new(new Offset2D(), swapChain.GetSwapChainExtent());
        vk.CmdSetViewport(commandBuffers[imageIndex], 0, 1, &viewport);
        vk.CmdSetScissor(commandBuffers[imageIndex], 0, 1, &scissor);

        renderGameObjects(commandBuffers[imageIndex]);
        //vk.CmdSetViewport(commandBuffers[imageIndex], 0, 1, (Viewport*)Unsafe.AsPointer(ref viewport));
        //vk.CmdSetScissor(commandBuffers[imageIndex], 0, 1, (Rect2D*)Unsafe.AsPointer(ref scissor));

        vk.CmdEndRenderPass(commandBuffers[imageIndex]);
        if (vk.EndCommandBuffer(commandBuffers[imageIndex]) != Result.Success)
        {
            throw new Exception("failed to record command buffer!");
        }

    }

    private void loadGameObjects()
    {
        var vertices = new Vertex[]
        {
            new Vertex(0.0f, -0.5f, 1.0f, 0.0f, 0.0f),
            new Vertex(0.5f, 0.5f, 0.0f, 1.0f, 0.0f),
            new Vertex(-0.5f, 0.5f, 0.0f, 0.0f, 1.0f),
        };

        var model = new LveModel(vk, device, vertices);

        var triangle = LveGameObject.CreateGameObject();
        triangle.Model = model;
        triangle.Color = new(0.1f, 0.8f, 0.1f, 0.0f);
        triangle.Transform2d = triangle.Transform2d with 
        { 
            Translation = new Vector2(0.2f, 0.0f),
            Scale = new Vector2(2.0f, 0.5f),
            Rotation = 0.25f * MathF.Tau
        };
        gameObjects.Add(triangle);

    }

    private unsafe void createPipelineLayout()
    {
        PushConstantRange pushConstantRange = new()
        {
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            Offset = 0,
            Size = SimplePushConstantData.SizeOf(),
        };


        PipelineLayoutCreateInfo pipelineLayoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 0,
            PSetLayouts = default,
            PushConstantRangeCount = 1,
            PPushConstantRanges = &pushConstantRange,
        };

        if (vk.CreatePipelineLayout(device.VkDevice, pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
        {
            throw new Exception("failed to create pipeline layout!");
        }
    }

    protected unsafe virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            window.Dispose();
            vk.DestroyPipelineLayout(device.VkDevice, pipelineLayout, null);

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~FirstApp()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
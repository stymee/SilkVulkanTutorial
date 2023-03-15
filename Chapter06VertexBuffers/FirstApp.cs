namespace Chapter06VertexBuffers;

public class FirstApp : IDisposable
{
    private const int WIDTH = 800;
    private const int HEIGHT = 600;


    private Vk vk = null!;

    private LveWindow window = null!;
    private LveDevice device = null!;
    private LvePipeline pipeline = null!;
    private LveSwapChain swapChain = null!;
    private LveModel model = null!;
    private PipelineLayout pipelineLayout;
    private CommandBuffer[] commandBuffers = null!;
    private bool disposedValue;

    public FirstApp()
    {
        log.RestartTimer();
        log.d("app run", "starting Run");

        vk = Vk.GetApi();
        log.d("app run", "got vk");

        window = new LveWindow(WIDTH, HEIGHT, "MyApp");
        log.d("app run", "got window");

        device = new LveDevice(vk, window);
        log.d("app run", "got device");

        swapChain = new LveSwapChain(vk, device, window.GetExtent());
        log.d("app run", "got swapchain");

        loadModels();
        createPipelineLayout();
        createPipeline();
        createCommandBuffers();
    }

    public void Run()
    {

        //pipeline = new LvePipeline(
        //    vk, device, 
        //    "simpleShader.vert.spv", "simpleShader.frag.spv", 
        //    LvePipeline.DefaultPipelineConfigInfo(WIDTH, HEIGHT)
        //    );
        //log.d("app run", "got pipeline");

        MainLoop();
        CleanUp();
    }


    private void drawFrame(double delta)
    {
        uint imageIndex = 0;
        var result = swapChain.AcquireNextImage(imageIndex);

        if (result != Result.Success && result != Result.SuboptimalKhr)
        {
            throw new Exception("failed to acquire next swapchain image");
        }


        result = swapChain.SubmitCommandBuffers(commandBuffers[imageIndex], imageIndex);
        if (result != Result.Success)
        {
            throw new Exception("failed to submit command buffers");
        }

    }

    private void MainLoop()
    {
        window.GlfwWindow.Render += drawFrame;
        window.Run();

        vk.DeviceWaitIdle(device.VkDevice);
    }

    private void CleanUp()
    {
        window.Dispose();
    }

    private void loadModels()
    {
        var vertices = new Vertex[3]
        {
            new Vertex(0.0f, -0.5f),
            new Vertex(0.5f, 0.5f),
            new Vertex(-0.5f, 0.5f),
        };

        model = new LveModel(vk, device, vertices);
    }

    private unsafe void createPipelineLayout()
    {
        PipelineLayoutCreateInfo pipelineLayoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 0,
            PSetLayouts = null,
            PushConstantRangeCount = 0,
            PPushConstantRanges = null,
        };

        if (vk.CreatePipelineLayout(device.VkDevice, pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
        {
            throw new Exception("failed to create pipeline layout!");
        }
    }

    private void createPipeline()
    {
        var pipelineConfig = LvePipeline.DefaultPipelineConfigInfo(swapChain.Width, swapChain.Height);
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
            CommandPool = device.GetCommandPool(),
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = (uint)commandBuffers.Length,
        };

        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            if (vk!.AllocateCommandBuffers(device.VkDevice, allocInfo, commandBuffersPtr) != Result.Success)
            {
                throw new Exception("failed to allocate command buffers!");
            }
        }


        for (int i = 0; i < commandBuffers.Length; i++)
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
            };

            if (vk.BeginCommandBuffer(commandBuffers[i], beginInfo) != Result.Success)
            {
                throw new Exception("failed to begin recording command buffer!");
            }

            RenderPassBeginInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = swapChain.GetRenderPass(),
                Framebuffer = swapChain.GetFrameBufferAt(i),
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
                    Color = new (){ Float32_0 = 0.1f, Float32_1 = 0.1f, Float32_2 = 0.1f, Float32_3 = 1 },
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

                vk.CmdBeginRenderPass(commandBuffers[i], &renderPassInfo, SubpassContents.Inline);
            }

            pipeline.Bind(commandBuffers[i]);

            model.Bind(commandBuffers[i]);
            model.Draw(commandBuffers[i]);
            //vk.CmdDraw(commandBuffers[i], 3, 1, 0, 0);


            vk.CmdEndRenderPass(commandBuffers[i]);


            if (vk.EndCommandBuffer(commandBuffers[i]) != Result.Success)
            {
                throw new Exception("failed to record command buffer!");
            }

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
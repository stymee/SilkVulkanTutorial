
using Silk.NET.Vulkan;

namespace Chapter20aDescriptorSetsNoBuilder;

public class FirstApp : IDisposable
{
    // Window stuff
    private IView window = null!;
    private int width = 1800;
    private int height = 1200;
    private string windowName = "Vulkan Tut";
    private long fpsUpdateInterval = 5 * 10_000;
    private long fpsLastUpdate;

    // Vk api
    private readonly Vk vk = null!;

    private LveDevice device = null!;
    private LveRenderer lveRenderer = null!;
    //private LveDescriptorPool globalPool = null!;

    private List<LveGameObject> gameObjects = new();

    private OrthographicCamera camera = null!;

    private bool disposedValue;

    private SimpleRenderSystem simpleRenderSystem = null!;

    private CameraController cameraController = null!;


    private LveBuffer[] uboBuffers = null!;
    private LveDescriptorSetLayout globalSetLayout = null!;
    private DescriptorSet[][] globalDescriptorSets = null!;

    private DescriptorPool descriptorPool;
    private DescriptorSet[] descriptorSets;
    //private DescriptorSetLayout descriptorSetLayout;

    public FirstApp()
    {
        log.RestartTimer();
        log.d("startup", "starting up");

        vk = Vk.GetApi();
        log.d("startup", "got vk");

        initWindow();
        log.d("startup", "got window");

        device = new LveDevice(vk, window);
        log.d("startup", "got device");

        lveRenderer = new LveRenderer(vk, window, device, useFifo: true);
        log.d("startup", "got renderer");

        //globalPool = new LveDescriptorPool.Builder(vk, device)
        //    .setMaxSets((uint)LveSwapChain.MAX_FRAMES_IN_FLIGHT)
        //    .AddPoolSize(DescriptorType.UniformBuffer, (uint)LveSwapChain.MAX_FRAMES_IN_FLIGHT)
        //    .Build();
        //globalPool = new LveDescriptorPool.Builder(vk,device)
        //    .setMaxSets(3)
        //    .AddPoolSize(DescriptorType.UniformBuffer, 3)
        //    .Build();
        //log.d("startup", "global descriptor pool created");
        //CreateDescriptorSetLayout();

        //CreateDescriptorSets();
        CreateDescriptorPool();

        loadGameObjects();
        log.d("startup", "objects loaded");

    }

    public void Run()
    {
        uboBuffers = new LveBuffer[LveSwapChain.MAX_FRAMES_IN_FLIGHT];
        for (int i = 0; i < LveSwapChain.MAX_FRAMES_IN_FLIGHT; i++)
        {
            uboBuffers[i] = new(
                vk, device,
                GlobalUbo.SizeOf(),
                1,
                BufferUsageFlags.UniformBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
                );
            uboBuffers[i].Map();
        }

        globalSetLayout = new LveDescriptorSetLayout.Builder(vk, device)
            .AddBinding(0, DescriptorType.UniformBuffer, ShaderStageFlags.VertexBit)
            .Build();

        

        simpleRenderSystem = new(
            vk, device, 
            lveRenderer.GetSwapChainRenderPass(), 
            globalSetLayout.GetDescriptorSetLayout()
            //descriptorSetLayout
            );
        log.d("run", "got render system");

        CreateDescriptorSets();


        //globalDescriptorSets = new DescriptorSet[LveSwapChain.MAX_FRAMES_IN_FLIGHT][];
        //for (var i = 0; i < globalDescriptorSets.Length; i++)
        //{
        //    var bufferInfo = uboBuffers[i].DescriptorInfo();
        //    _ = new LveDescriptorSetWriter(vk, device, globalSetLayout)
        //        .WriteBuffer(0, bufferInfo)
        //        .Build(descriptorPool, globalSetLayout.GetDescriptorSetLayout(), ref globalDescriptorSets[i]);
        //    //Console.WriteLine($"got a  built globalDescriptorSet[{i}]={globalDescriptorSets[i].Handle}");
        //}

        //simpleRenderSystem = new(
        //    vk, device, 
        //    lveRenderer.GetSwapChainRenderPass(), 
        //    globalSetLayout.GetDescriptorSetLayout()
        //    );
        //log.d("run", "got render system");

        camera = new(Vector3.Zero, 2f, -20f, -140f, window.FramebufferSize);
        cameraController = new(camera, (IWindow)window);
        resize(window.FramebufferSize);
        log.d("run", "got camera");





        MainLoop();
        CleanUp();
    }


    // mouse stuff
    private MouseState mouseLast;


    private void render(double delta)
    {
        mouseLast = cameraController.GetMouseState();

        var commandBuffer = lveRenderer.BeginFrame();

        if (commandBuffer is not null)
        {
            int frameIndex = lveRenderer.GetFrameIndex();
            //var checkHandle = descriptorSets[frameIndex].Handle;
            //if (checkHandle == 0)
            //{
            //    Console.WriteLine($"in render...globalDescriptorSets[{frameIndex}] handle is ZERO!");
            //    var crap = 0;
            //}
            //else
            //{
            //    Console.WriteLine($"in render...globalDescriptorSets[{frameIndex}] handle is {checkHandle}!");
            //    var crap = 0;
            //}
            FrameInfo frameInfo = new()
            {
                FrameIndex = frameIndex,
                CommandBuffer = commandBuffer.Value,
                Camera = camera,
                //GlobalDescriptorSet = globalDescriptorSets[frameIndex][0],
                GlobalDescriptorSet = descriptorSets[frameIndex],
            };

            var ubo = new GlobalUbo[1]
            {
                new GlobalUbo()
                {
                    ProjectionView = camera.GetViewMatrix() * camera.GetProjectionMatrix()
                }
            };

            uboBuffers[frameIndex].WriteToBuffer(ubo);
            // using coherent bit in ubo construction, so don't need to flush?  confusing
            //globalUboBuffer[frameIndex].Flush();
            
            lveRenderer.BeginSwapChainRenderPass(commandBuffer.Value);
            simpleRenderSystem.RenderGameObjects(frameInfo, ref gameObjects);
            lveRenderer.EndSwapChainRenderPass(commandBuffer.Value);
            lveRenderer.EndFrame();
        }
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

        window.FramebufferResize += resize;
        window.Update += updateWindow;
        window.Render += render;
    }

    private void updateWindow(double frametime)
    {

        if (DateTime.Now.Ticks - fpsLastUpdate < fpsUpdateInterval) return;

        fpsLastUpdate = DateTime.Now.Ticks;
        if (window is IWindow w)
        {
            //w.Title = $"{windowName} | W {window.Size.X}x{window.Size.Y} | FPS {Math.Ceiling(1d / obj)} | ";
            w.Title = $"{windowName} | {mouseLast.Debug} | {1d / frametime,-8: 0,000.0} fps";
        }

    }

    private void resize(Vector2D<int> newsize)
    {
        camera.Resize(0, 0, (uint)newsize.X, (uint)newsize.Y);
        cameraController.Resize(newsize);
    }

    private void loadGameObjects()
    {
        //cube.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/smooth_vase.obj");
        //cube.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/colored_cube.obj");
        //cube.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/viking_room.obj");
        //cube.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/colored_cube.obj");
        //cube.Model = ModelUtils.CreateCubeModel3(vk, device);

        var flatVase = LveGameObject.CreateGameObject();
        flatVase.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/flat_vase.obj");
        flatVase.Transform.Translation = new(0.25f, 0.0f, 0.0f);
        flatVase.Transform.Scale = new(1.0f, 0.5f, 1.0f);
        gameObjects.Add(flatVase);

        var smoothVase = LveGameObject.CreateGameObject();
        smoothVase.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/smooth_vase.obj");
        smoothVase.Transform.Translation = new(-0.25f, 0.0f, 0.0f);
        smoothVase.Transform.Scale = new(1.0f, 0.5f, 1.0f);
        gameObjects.Add(smoothVase);
    }


    protected unsafe virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                window.Dispose();
                // TODO: dispose managed state (managed objects)
            }

            //vk.DestroyPipelineLayout(device.VkDevice, pipelineLayout, null);

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

    private unsafe void CreateDescriptorPool()
    {
        DescriptorPoolSize poolSize = new()
        {
            Type = DescriptorType.UniformBuffer,
            DescriptorCount = (uint)LveSwapChain.MAX_FRAMES_IN_FLIGHT,
        };


        DescriptorPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            PoolSizeCount = 1,
            PPoolSizes = &poolSize,
            MaxSets = (uint)LveSwapChain.MAX_FRAMES_IN_FLIGHT,
        };

        //fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
        fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
        {
            if (vk!.CreateDescriptorPool(device.VkDevice, poolInfo, null, descriptorPoolPtr) != Result.Success)
            {
                throw new Exception("failed to create descriptor pool!");
            }

        }
    }

    //private unsafe void CreateDescriptorSetLayout()
    //{
    //    DescriptorSetLayoutBinding uboLayoutBinding = new()
    //    {
    //        Binding = 0,
    //        DescriptorCount = 1,
    //        DescriptorType = DescriptorType.UniformBuffer,
    //        PImmutableSamplers = null,
    //        StageFlags = ShaderStageFlags.VertexBit,
    //    };

    //    DescriptorSetLayoutCreateInfo layoutInfo = new()
    //    {
    //        SType = StructureType.DescriptorSetLayoutCreateInfo,
    //        BindingCount = 1,
    //        PBindings = &uboLayoutBinding,
    //    };

    //    fixed (DescriptorSetLayout* descriptorSetLayoutPtr = &descriptorSetLayout)
    //    {
    //        if (vk!.CreateDescriptorSetLayout(device.VkDevice, layoutInfo, null, descriptorSetLayoutPtr) != Result.Success)
    //        {
    //            throw new Exception("failed to create descriptor set layout!");
    //        }
    //    }
    //}


    private unsafe void CreateDescriptorSets()
    {
        var layouts = new DescriptorSetLayout[LveSwapChain.MAX_FRAMES_IN_FLIGHT];
        //Array.Fill(layouts, descriptorSetLayout);
        Array.Fill(layouts, globalSetLayout.GetDescriptorSetLayout());

        fixed (DescriptorSetLayout* layoutsPtr = layouts)
        {
            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = descriptorPool,// globalPool.GetDescriptorPool(),
                DescriptorSetCount = (uint)LveSwapChain.MAX_FRAMES_IN_FLIGHT,
                PSetLayouts = layoutsPtr,
            };

            descriptorSets = new DescriptorSet[LveSwapChain.MAX_FRAMES_IN_FLIGHT];
            fixed (DescriptorSet* descriptorSetsPtr = descriptorSets)
            {
                var result = vk!.AllocateDescriptorSets(device.VkDevice, allocateInfo, descriptorSetsPtr);
                if (result != Result.Success)
                {
                    throw new Exception("failed to allocate descriptor sets!");
                }
            }
        }


        for (int i = 0; i < LveSwapChain.MAX_FRAMES_IN_FLIGHT; i++)
        {
            DescriptorBufferInfo bufferInfo = new()
            {
                Buffer = uboBuffers[i].VkBuffer,
                Offset = 0,
                Range = (ulong)Unsafe.SizeOf<GlobalUbo>(),

            };

            WriteDescriptorSet descriptorWrite = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSets[i],
                DstBinding = 0,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = &bufferInfo,
            };

            vk!.UpdateDescriptorSets(device.VkDevice, 1, descriptorWrite, 0, null);
        }

    }

}
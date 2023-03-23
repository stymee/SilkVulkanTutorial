
namespace Chapter27AlphaBlending;

public class FirstApp : IDisposable
{
    // set to true to force FIFO swapping
    private const bool USE_FIFO = true;

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
    private LveDescriptorPool globalPool = null!;

    private Dictionary<uint, LveGameObject> gameObjects = new();

    private ICamera camera = null!;

    private bool disposedValue;

    private SimpleRenderSystem simpleRenderSystem = null!;
    private PointLightRenderSystem pointLightRenderSystem = null!;

    private CameraController cameraController = null!;
    private KeyboardController keyboardController = null!;


    private GlobalUbo[] ubos = null!;
    private LveBuffer[] uboBuffers = null!;
    
    private LveDescriptorSetLayout globalSetLayout = null!;
    private DescriptorSet[] globalDescriptorSets = null!;

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

        lveRenderer = new LveRenderer(vk, window, device, useFifo: USE_FIFO);
        log.d("startup", "got renderer");

        globalPool = new LveDescriptorPool.Builder(vk, device)
            .SetMaxSets((uint)LveSwapChain.MAX_FRAMES_IN_FLIGHT)
            .AddPoolSize(DescriptorType.UniformBuffer, (uint)LveSwapChain.MAX_FRAMES_IN_FLIGHT)
            .Build();
        log.d("startup", "global descriptor pool created");

        loadGameObjects();
        log.d("startup", "objects loaded");

    }

    public void Run()
    {
        int frames = LveSwapChain.MAX_FRAMES_IN_FLIGHT;
        ubos = new GlobalUbo[frames];
        uboBuffers = new LveBuffer[frames];
        for (int i = 0; i < frames; i++)
        {
            ubos[i] = new GlobalUbo();
            uboBuffers[i] = new(
                vk, device,
                GlobalUbo.SizeOf(),
                1,
                BufferUsageFlags.UniformBufferBit,
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
                );
            uboBuffers[i].Map();
        }
        log.d("run", "initialized ubo buffers");

        globalSetLayout = new LveDescriptorSetLayout.Builder(vk, device)
            .AddBinding(0, DescriptorType.UniformBuffer, ShaderStageFlags.AllGraphics)
            .Build();

        globalDescriptorSets = new DescriptorSet[frames];
        for (var i = 0; i < globalDescriptorSets.Length; i++)
        {
            var bufferInfo = uboBuffers[i].DescriptorInfo();
            _ = new LveDescriptorSetWriter(vk, device, globalSetLayout)
                .WriteBuffer(0, bufferInfo)
                .Build(
                    globalPool,
                    globalSetLayout.GetDescriptorSetLayout(), ref globalDescriptorSets[i]
                    );
        }
        log.d("run", "got globalDescriptorSets");


        simpleRenderSystem = new(
            vk, device,
            lveRenderer.GetSwapChainRenderPass(),
            globalSetLayout.GetDescriptorSetLayout()
            );

        pointLightRenderSystem = new(
            vk, device,
            lveRenderer.GetSwapChainRenderPass(),
            globalSetLayout.GetDescriptorSetLayout()
            );
        log.d("run", "got render systems");



        camera = new OrthographicCamera(Vector3.Zero, 4f, -20f, -140f, window.FramebufferSize);
        //camera = new PerspectiveCamera(new Vector3(5,5,5), 45f, 0f, 0f, window.FramebufferSize);
        cameraController = new(camera, (IWindow)window);
        resize(window.FramebufferSize);
        keyboardController = new((IWindow)window);
        keyboardController.OnKeyPressed += onKeyPressed;
        log.d("run", "got camera and controls");

        //Console.WriteLine($"GlobalUbo blittable={BlittableHelper<GlobalUbo>.IsBlittable}");

        MainLoop();
        CleanUp();
    }

    private void onKeyPressed(Key key)
    {
        switch (key)
        {
            case Key.Space:
                pointLightRenderSystem.RotateLightsEnabled = !pointLightRenderSystem.RotateLightsEnabled;
                break;
            case Key.KeypadAdd:
                pointLightRenderSystem.RotateSpeed += 0.5f;
                break;
            case Key.KeypadSubtract:
                pointLightRenderSystem.RotateSpeed -= 0.5f;
                break;
            case Key.Up:
                pointLightRenderSystem.YPosition += 0.05f;
                break;
            case Key.Down:
                pointLightRenderSystem.YPosition -= 0.05f;
                break;
            case Key.Left:
                pointLightRenderSystem.XPosition += 0.2f;
                break;
            case Key.Right:
                pointLightRenderSystem.XPosition += 0.2f;
                break;
            default:
                break;
        }
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
            FrameInfo frameInfo = new()
            {
                FrameIndex = frameIndex,
                FrameTime = (float)delta,
                CommandBuffer = commandBuffer.Value,
                Camera = camera,
                GlobalDescriptorSet = globalDescriptorSets[frameIndex],
                GameObjects = gameObjects
            };

            pointLightRenderSystem.Update(frameInfo, ref ubos[frameIndex]);
            
            ubos[frameIndex].Update(camera.GetProjectionMatrix(), camera.GetViewMatrix(), camera.GetFrontVec4());
            uboBuffers[frameIndex].WriteBytesToBuffer(ubos[frameIndex].AsBytes());

            lveRenderer.BeginSwapChainRenderPass(commandBuffer.Value);

            // render solid objects first!
            simpleRenderSystem.Render(frameInfo);

            pointLightRenderSystem.Render(frameInfo);

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
        var flatVase = LveGameObject.CreateGameObject();
        flatVase.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/flat_vase.obj");
        flatVase.Transform.Translation = new(-.5f, 0.5f, 0.0f);
        flatVase.Transform.Scale = new(3.0f, 1.5f, 3.0f);
        gameObjects.Add(flatVase.Id, flatVase);

        var smoothVase = LveGameObject.CreateGameObject();
        smoothVase.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/smooth_vase.obj");
        smoothVase.Transform.Translation = new(.5f, 0.5f, 0.0f);
        smoothVase.Transform.Scale = new(3.0f, 1.5f, 3.0f);
        gameObjects.Add(smoothVase.Id, smoothVase);

        var floor = LveGameObject.CreateGameObject();
        floor.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/quad.obj");
        floor.Transform.Translation = new(0f, 0.5f, 0f);
        floor.Transform.Scale = new(3f, 1f, 3f);
        gameObjects.Add(floor.Id, floor);


        var lightColors = new Vector4[]
        {
          new(1f, .1f, .1f, 1f),
          new(.1f, .1f, 1f, 1f),
          new(.1f, 1f, .1f, 1f),
          new(1f, 1f, .1f, 1f),
          new(.1f, 1f, 1f, 1f),
          new(1f, 1f, 1f, 1f)
        };
        for (var i = 0; i < 6; i++)
        {
            var pointLight = LveGameObject.MakePointLight(
                0.2f, 0.05f, lightColors[i]
                );
            var rotateLight = Matrix4x4.CreateRotationY(i * MathF.PI / lightColors.Length * 2f);
            pointLight.Transform.Translation = Vector3.Transform(new(1.25f, 1.25f, 0f), rotateLight);
            gameObjects.Add(pointLight.Id, pointLight);
        }

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


}



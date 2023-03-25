namespace Chapter18DiffuseShading;

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
    private List<LveGameObject> gameObjects = new();

    private OrthographicCamera camera = null!;

    private bool disposedValue;

    private SimpleRenderSystem simpleRenderSystem = null!;

    private CameraController cameraController = null!;


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

        lveRenderer = new LveRenderer(vk, window, device, useFifo: false);
        log.d("startup", "got renderer");

        loadGameObjects();
        log.d("startup", "objects loaded");

        simpleRenderSystem = new(vk, device, lveRenderer.GetSwapChainRenderPass());
        log.d("startup", "got render system");

        camera = new(Vector3.Zero, 2f, -20f, -140f, window.FramebufferSize);
        cameraController = new(camera, (IWindow)window);
        resize(window.FramebufferSize);
        log.d("startup", "got camera");
    }

    public void Run()
    {
        MainLoop();
    }


    // mouse stuff
    private MouseState mouseLast;


    private void render(double delta)
    {
        mouseLast = cameraController.GetMouseState();

        var commandBuffer = lveRenderer.BeginFrame();

        if (commandBuffer is not null)
        {
            lveRenderer.BeginSwapChainRenderPass(commandBuffer.Value);
            simpleRenderSystem.RenderGameObjects(commandBuffer.Value, ref gameObjects, camera);
            lveRenderer.EndSwapChainRenderPass(commandBuffer.Value);
            lveRenderer.EndFrame();
        }
    }

    private void MainLoop()
    {
        window.Run();

        vk.DeviceWaitIdle(device.VkDevice);
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


    public unsafe void Dispose()
    {
        window.Dispose();
        lveRenderer.Dispose();
        simpleRenderSystem.Dispose();
        device.Dispose();

        GC.SuppressFinalize(this);
    }

}
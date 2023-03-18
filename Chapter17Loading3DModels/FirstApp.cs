namespace Chapter17Loading3DModels;

public class FirstApp : IDisposable
{
    // Window stuff
    private IView window = null!;
    private int width = 1600;
    private int height = 800;
    private string windowName = "Vulkan Tut";
    private long fpsUpdateInterval = 5 * 10_000;
    private long fpsLastUpdate;
    //private IInputContext inputContext = null!;

    // Vk api
    private readonly Vk vk = null!;

    private LveDevice device = null!;
    private LveRenderer lveRenderer = null!;
    private List<LveGameObject> gameObjects = new();

    private OrthographicCamera camera = null!;

    private bool disposedValue;

    private SimpleRenderSystem simpleRenderSystem = null!;

    //private LveGameObject viewerObject = null!;
    //private KeyboardMovementController cameraController = null!;
    private MouseMovementController cameraController = null!;


    //long currentTime = 0;

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

        //inputContext = window.CreateInput();
        //viewerObject = LveGameObject.CreateGameObject();
        //cameraController = new((IWindow)window);
        //log.d("startup", "got input");


        simpleRenderSystem = new(vk, device, lveRenderer.GetSwapChainRenderPass());
        log.d("startup", "got render system");

        camera = new(new Vector3(-10f, 0f, 0f), 2f, 0f, 0f, window.FramebufferSize);
        cameraController = new(camera, (IWindow)window);
        cameraController.OnMouseStateChanged += OnMouseStateChanged;
        //camera.SetViewDirection(Vector3.Zero, new(0.5f, 0f, 1f), -Vector3.UnitY);
        //camera.SetViewTarget(new(-1f, -2f, 2f), new(0f, 0f, 2.5f), -Vector3.UnitY);
        log.d("startup", "got camera");
    }

    public void Run()
    {
        MainLoop();
        CleanUp();
    }


    // mouse stuff
    private MouseState mouseLast;

    private void OnMouseStateChanged(MouseState mouseCurrent) 
    {

        switch (mouseCurrent.ControlState)
        {
            case MouseControlState.Pick:
                break;
            case MouseControlState.Pan:
                camera.Pan2d(mouseLast.Pos2d, mouseCurrent.Pos2d);
                break;
            case MouseControlState.ZoomWheel:
                camera.ZoomIncremental(mouseCurrent.Wheel);
                break;
            case MouseControlState.Rotate:
                camera.Rotate(mouseLast.Pos2d, mouseCurrent.Pos2d);
                break;
            default:
                break;
        }
        mouseLast = mouseCurrent;
    }

    private void render(double delta)
    {
        //cameraController.MoveInPlaceXZ((IWindow)window, delta, ref viewerObject);
        //camera.SetViewYXZ(viewerObject.Transform.Translation, viewerObject.Transform.Rotation);

        //float aspect = lveRenderer.GetAspectRatio();
        //camera.SetOrthographicProjection(-aspect, aspect, -1f, 1f, -1f, 1f);
        //camera.SetPerspectiveProjection(50f * MathF.PI / 180f, aspect, 0.1f, 10f);

        //lastMouseState = currentMouseState;
        //cameraController.DoMouse();

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
        var cube = LveGameObject.CreateGameObject();
        cube.Model = ModelUtils.LoadModelFromFile(vk, device, "Assets/colored_cube.obj");
        //cube.Model = CreateCubeModel(vk, device, Vector3.Zero);
        //cube.Transform.Translation = new(0.0f, 0.0f, 0.0f);
        //cube.Transform.Scale = new(0.5f);

        gameObjects.Add(cube);
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
namespace Chapter11RendererSystems;

public class FirstApp : IDisposable
{
    // Window stuff
    private IView window = null!;
    private int width = 800;
    private int height = 600;
    private string windowName = "Vulkan Tut";
    private long fpsUpdateInterval = 200 * 10_000;
    private long fpsLastUpdate;

    // Vk api
    private readonly Vk vk = null!;

    private LveDevice device = null!;
    private LveRenderer lveRenderer = null!;
    private List<LveGameObject> gameObjects = new();

    private bool disposedValue;

    private SimpleRenderSystem simpleRenderSystem = null!;

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

        lveRenderer = new LveRenderer(vk, window, device);
        log.d("startup", "got renderer");

        loadGameObjects();
        log.d("startup", "objects loaded");
    }

    public void Run()
    {
        simpleRenderSystem = new(vk, device, lveRenderer.GetSwapChainRenderPass());
        log.d("startup", "got render system");
        MainLoop();
        CleanUp();
    }


    private void render(double delta)
    {
        var commandBuffer = lveRenderer.BeginFrame();

        if (commandBuffer is not null)
        {
            //var check = commandBuffer.Value.Handle;
            //Console.WriteLine($"  0x{check:X8} [{lveRenderer.GetFrameIndex(),4}] BeginFrame outside");

            lveRenderer.BeginSwapChainRenderPass(commandBuffer.Value);
            simpleRenderSystem.RenderGameObjects(commandBuffer.Value, ref gameObjects);
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
            w.Title = $"{windowName} - {1d / frametime,-8: #,##0.0} fps";
        }

    }

    private void resize(Vector2D<int> newsize)
    {

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
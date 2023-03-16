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

    private SimpleRenderSystem srs = null!;

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

        lveRenderer = new LveRenderer(vk, window, device);

        
        srs = new(vk, device, lveRenderer.GetSwapChainRenderPass());
    }

    public void Run()
    {
        MainLoop();
        CleanUp();
    }


    private void render(double delta)
    {
        var commandBuffer = lveRenderer.BeginFrame();
        if (commandBuffer is not null)
        {
            lveRenderer.BeginSwapChainRenderPass(commandBuffer.Value);
            srs.RenderGameObjects(commandBuffer.Value, ref gameObjects);
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

    private void resize(Vector2D<int> newsize)
    {

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
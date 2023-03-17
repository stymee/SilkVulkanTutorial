namespace Chapter13ProjectionMatrices;

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
    private LveCamera camera = null!;

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
        camera = new();
        log.d("startup", "got camera");
        MainLoop();
        CleanUp();
    }


    private void render(double delta)
    {
        float aspect = lveRenderer.GetAspectRatio();
        //camera.SetOrthographicProjection(-aspect, aspect, -1f, 1f, -1f, 1f);
        camera.SetPerspectiveProjection(50f * MathF.PI / 180f, aspect, 0.1f, 10f);

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
            w.Title = $"{windowName} - {1d / frametime,-8: #,##0.0} fps";
        }

    }

    private void resize(Vector2D<int> newsize)
    {

    }

    private void loadGameObjects()
    {
        var cube = LveGameObject.CreateGameObject();
        cube.Model = CreateCubeModel(vk, device, Vector3.Zero);
        cube.Transform = cube.Transform with
        {
            Translation = new(0.0f, 0.0f, 2.5f),
            Scale = new(0.5f)
        };

        gameObjects.Add(cube);
    }


    public static LveModel CreateCubeModel(Vk vk, LveDevice device, Vector3 offset)
    {
        var vertices = new List<Vertex>
        {
            // left face (white)
            new Vertex(new Vector3(-.5f, -.5f, -.5f) + offset, new Vector3(.9f, .9f, .9f)),
            new Vertex(new Vector3(-.5f, .5f, .5f) + offset, new Vector3(.9f, .9f, .9f)),
            new Vertex(new Vector3(-.5f, -.5f, .5f) + offset, new Vector3(.9f, .9f, .9f)),
            new Vertex(new Vector3(-.5f, -.5f, -.5f) + offset, new Vector3(.9f, .9f, .9f)),
            new Vertex(new Vector3(-.5f, .5f, -.5f) + offset, new Vector3(.9f, .9f, .9f)),
            new Vertex(new Vector3(-.5f, .5f, .5f) + offset, new Vector3(.9f, .9f, .9f)),

            // right face (yellow)
            new Vertex(new Vector3(.5f, -.5f, -.5f) + offset, new Vector3(.8f, .8f, .1f)),
            new Vertex(new Vector3(.5f, .5f, .5f) + offset, new Vector3(.8f, .8f, .1f)),
            new Vertex(new Vector3(.5f, -.5f, .5f) + offset, new Vector3(.8f, .8f, .1f)),
            new Vertex(new Vector3(.5f, -.5f, -.5f) + offset, new Vector3(.8f, .8f, .1f)),
            new Vertex(new Vector3(.5f, .5f, -.5f) + offset, new Vector3(.8f, .8f, .1f)),
            new Vertex(new Vector3(.5f, .5f, .5f) + offset, new Vector3(.8f, .8f, .1f)),

            // top face (orange, remember y axis points down)
            new Vertex(new Vector3(-.5f, -.5f, -.5f) + offset, new Vector3(.9f, .6f, .1f)),
            new Vertex(new Vector3(.5f, -.5f, .5f) + offset, new Vector3(.9f, .6f, .1f)),
            new Vertex(new Vector3(-.5f, -.5f, .5f) + offset, new Vector3(.9f, .6f, .1f)),
            new Vertex(new Vector3(-.5f, -.5f, -.5f) + offset, new Vector3(.9f, .6f, .1f)),
            new Vertex(new Vector3(.5f, -.5f, -.5f) + offset, new Vector3(.9f, .6f, .1f)),
            new Vertex(new Vector3(.5f, -.5f, .5f) + offset, new Vector3(.9f, .6f, .1f)),

            // bottom face (red)
            new Vertex(new Vector3(-.5f, .5f, -.5f) + offset, new Vector3(.8f, .1f, .1f)),
            new Vertex(new Vector3(.5f, .5f, .5f) + offset, new Vector3(.8f, .1f, .1f)),
            new Vertex(new Vector3(-.5f, .5f, .5f) + offset, new Vector3(.8f, .1f, .1f)),
            new Vertex(new Vector3(-.5f, .5f, -.5f) + offset, new Vector3(.8f, .1f, .1f)),
            new Vertex(new Vector3(.5f, .5f, -.5f) + offset, new Vector3(.8f, .1f, .1f)),
            new Vertex(new Vector3(.5f, .5f, .5f) + offset, new Vector3(.8f, .1f, .1f)),

            // nose face (blue)
            new Vertex(new Vector3(-.5f, -.5f, .5f) + offset, new Vector3(.1f, .1f, .8f)),
            new Vertex(new Vector3(.5f, .5f, .5f) + offset, new Vector3(.1f, .1f, .8f)),
            new Vertex(new Vector3(-.5f, .5f, .5f) + offset, new Vector3(.1f, .1f, .8f)),
            new Vertex(new Vector3(-.5f, -.5f, .5f) + offset, new Vector3(.1f, .1f, .8f)),
            new Vertex(new Vector3(.5f, -.5f, .5f) + offset, new Vector3(.1f, .1f, .8f)),
            new Vertex(new Vector3(.5f, .5f, .5f) + offset, new Vector3(.1f, .1f, .8f)),

            // tail face (green)
            new Vertex(new Vector3(-.5f, -.5f, -.5f) + offset, new Vector3(.1f, .8f, .1f)),
            new Vertex(new Vector3(.5f, .5f, -.5f) + offset, new Vector3(.1f, .8f, .1f)),
            new Vertex(new Vector3(-.5f, .5f, -.5f) + offset, new Vector3(.1f, .8f, .1f)),
            new Vertex(new Vector3(-.5f, -.5f, -.5f) + offset, new Vector3(.1f, .8f, .1f)),
            new Vertex(new Vector3(.5f, -.5f, -.5f) + offset, new Vector3(.1f, .8f, .1f)),
            new Vertex(new Vector3(.5f, .5f, -.5f) + offset, new Vector3(.1f, .8f, .1f)),

            // ...
        };
        return new LveModel(vk, device, vertices.ToArray());
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
namespace Chapter08DynamicViewports;

public class LveWindow: IDisposable
{
    private string windowName = "";
    private int width = 800;
    private int height = 600;
    private bool framebufferResized = false;
    public bool WasWindowResized => framebufferResized;

    public Action<double> OnRender = null!;
    public Action<Vector2D<int>> OnResize = null!;

    private long fpsUpdateInterval =  200 * 10_000;
    private long fpsLastUpdate;

    private IWindow window = null!;
    public IWindow GlfwWindow => window;

    public IVkSurface VkSurface => window.VkSurface ?? throw new ApplicationException("VkSurface is null!");

    public LveWindow(int w, int h, string name)
    {
        width = w;
        height = h;
        windowName = name;

        initWindow();
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

        window.Render += onRender;
        window.FramebufferResize += onResize;
        window.Update += onUpdate;
    }

    public Extent2D GetExtent()
    {
        return new((uint)width, (uint)height);
    }

    //public unsafe void CreateWindowSurface(Instance instance, out SurfaceKHR surface)
    //{
    //    if (window.VkSurface is null) throw new ApplicationException("In CreateWindowSurface, window.VkSurface is null!");

    //    surface = window.VkSurface.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();

    //}
    private void onRender(double delta)
    {
        OnRender?.Invoke(delta);
    }

    private void onResize(Vector2D<int> newsize)
    {
        width = newsize.X;
        height = newsize.Y;
        framebufferResized = true;
        OnResize?.Invoke(newsize);
    }

    private void onUpdate(double frametime)
    {

        if (DateTime.Now.Ticks - fpsLastUpdate < fpsUpdateInterval) return;

        fpsLastUpdate = DateTime.Now.Ticks;
        window.Title = $"{windowName} - {1d / frametime,-8: #,##0.0} fps";
    }

    public void ResetWindowResizeFlag()
    {
        framebufferResized = false;
    }

    public void Run()
    {
        window.Run();
    }

    public void Dispose()
    {
        window.Render -= onRender;
        window.FramebufferResize -= onResize;
        window.Dispose();
    }
}

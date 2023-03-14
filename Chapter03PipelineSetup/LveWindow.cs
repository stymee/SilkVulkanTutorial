global using Window = Silk.NET.Windowing.Window;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace Chapter03PipelineSetup;

public class LveWindow: IDisposable
{
    private string windowName = "";
    private int width = 800;
    private int height = 600;
    private IWindow window = null!;

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

    }

    //public unsafe void CreateWindowSurface(Instance instance, out SurfaceKHR surface)
    //{
    //    if (window.VkSurface is null) throw new ApplicationException("In CreateWindowSurface, window.VkSurface is null!");

    //    surface = window.VkSurface.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();

    //}
    public void Run()
    {
        window.Run();
    }

    public void Dispose()
    {
        window.Dispose();
    }
}

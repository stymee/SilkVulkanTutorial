using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Chapter01Window;

public class LveWindow: IDisposable
{
    private string windowName = "";
    private int width = 800;
    private int height = 600;
    private IWindow window = null!;

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

    public void Run()
    {
        window.Run();
    }

    public void Dispose()
    {
        window.Dispose();
    }
}

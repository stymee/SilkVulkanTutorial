using Silk.NET.Vulkan;
using System.Reflection;

namespace Chapter02Shaders;

public class FirstApp
{
    private const int WIDTH = 800;
    private const int HEIGHT = 600;

    
    private LveWindow window = null!;
    private LvePipeline pipeline = null!;

    private Vk vk = null!;


    public void Run()
    {
        window = new LveWindow(WIDTH, HEIGHT, "MyApp");
        pipeline = new LvePipeline("simpleShader.vert.spv", "simpleShader.frag.spv");
        
        InitVulkan();
        MainLoop();
        CleanUp();
    }


    private void InitVulkan()
    {
        vk = Vk.GetApi();


        //uint extensionsCount = 0;
        //byte* nullBytes = null;
        //vk.EnumerateInstanceExtensionProperties(nullBytes, ref extensionsCount, null);

        //Console.WriteLine($" {extensionsCount} available extensions supported...");
    }




    private void MainLoop()
    {
        window.Run();
    }

    private void CleanUp()
    {
        window.Dispose();
    }


}
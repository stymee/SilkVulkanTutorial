using Silk.NET.Vulkan;
using System.Reflection;

namespace Chapter03PipelineSetup;

public class FirstApp
{
    private const int WIDTH = 800;
    private const int HEIGHT = 600;


    private Vk vk = null!;

    private LveWindow window = null!;
    private LveDevice device = null!;
    private LvePipeline pipeline = null!;



    public void Run()
    {
        vk = Vk.GetApi();
        window = new LveWindow(WIDTH, HEIGHT, "MyApp");
        device = new LveDevice(window, vk);
        pipeline = new LvePipeline("simpleShader.vert.spv", "simpleShader.frag.spv");
        
        //InitVulkan();
        MainLoop();
        CleanUp();
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
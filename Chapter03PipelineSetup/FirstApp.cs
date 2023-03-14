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
        log.RestartTimer();
        log.d("app run", "starting Run");

        vk = Vk.GetApi();
        log.d("app run", "got vk");

        window = new LveWindow(WIDTH, HEIGHT, "MyApp");
        log.d("app run", "got window");

        device = new LveDevice(vk, window);
        log.d("app run", "got device");

        pipeline = new LvePipeline(
            vk, device, 
            "simpleShader.vert.spv", "simpleShader.frag.spv", 
            LvePipeline.DefaultPipelineConfigInfo(WIDTH, HEIGHT)
            );
        log.d("app run", "got pipeline");

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
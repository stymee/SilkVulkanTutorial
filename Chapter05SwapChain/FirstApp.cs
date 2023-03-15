using Silk.NET.Vulkan;

namespace Chapter05SwapChain;

public class FirstApp : IDisposable
{
    private const int WIDTH = 800;
    private const int HEIGHT = 600;


    private Vk vk = null!;

    private LveWindow window = null!;
    private LveDevice device = null!;
    private LvePipeline pipeline = null!;
    private LveSwapChain swapChain = null!;
    private PipelineLayout pipelineLayout;
    private CommandBuffer[] commandBuffers = null!;
    private bool disposedValue;

    public FirstApp()
    {
        log.RestartTimer();
        log.d("app run", "starting Run");

        vk = Vk.GetApi();
        log.d("app run", "got vk");

        window = new LveWindow(WIDTH, HEIGHT, "MyApp");
        log.d("app run", "got window");

        device = new LveDevice(vk, window);
        log.d("app run", "got device");

        swapChain = new LveSwapChain(vk, device, window.GetExtent());
        log.d("app run", "got swapchain");

        createPipelineLayout();
        createPipeline();
        createCommandBuffers();
    }

    public void Run()
    {

        //pipeline = new LvePipeline(
        //    vk, device, 
        //    "simpleShader.vert.spv", "simpleShader.frag.spv", 
        //    LvePipeline.DefaultPipelineConfigInfo(WIDTH, HEIGHT)
        //    );
        //log.d("app run", "got pipeline");

        MainLoop();
        CleanUp();
    }


    private void drawFrame()
    {

    }

    private void MainLoop()
    {
        window.Run();
    }

    private void CleanUp()
    {
        window.Dispose();
    }

    private unsafe void createPipelineLayout()
    {
        PipelineLayoutCreateInfo pipelineLayoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 0,
            PSetLayouts = null,
            PushConstantRangeCount = 0,
            PPushConstantRanges = null,
        };

        if (vk.CreatePipelineLayout(device.VkDevice, pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
        {
            throw new Exception("failed to create pipeline layout!");
        }
    }

    private void createPipeline()
    {
        var pipelineConfig = LvePipeline.DefaultPipelineConfigInfo(swapChain.Width, swapChain.Height);
        pipelineConfig.RenderPass = swapChain.GetRenderPass();
        pipelineConfig.PipelineLayout = pipelineLayout;
        pipeline = new LvePipeline(
            vk, device,
            "simpleShader.vert.spv", "simpleShader.frag.spv",
            pipelineConfig
            );
        log.d("app run", "got pipeline");
    }

    private void createCommandBuffers()
    {

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
            vk.DestroyPipelineLayout(device.VkDevice, pipelineLayout, null);

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
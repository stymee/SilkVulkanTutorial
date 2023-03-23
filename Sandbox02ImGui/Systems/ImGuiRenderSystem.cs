
namespace Sandbox02ImGui;

public class ImGuiRenderSystem : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    private bool disposedValue;

    private LvePipeline pipeline = null!;
    private PipelineLayout pipelineLayout;

    // ImGui init stuff
    private IView view = null!;
    private IInputContext inputContext = null!;
    private PhysicalDevice physicalDevice;
    private uint graphicsFamilyIndex;
    private int windowWidth;
    private int windowHeight;
    private int swapChainImageCt;

    // other ImGui stuff
    private Sampler _fontSampler;
    private DeviceMemory _fontMemory;
    private Image _fontImage;
    private ImageView _fontView;
    private ulong _bufferMemoryAlignment = 256;

    //private Vector2D<int> windowSize;
    private Vector2D<int> framebufferSize;

    public ImGuiRenderSystem(Vk vk, LveDevice device, RenderPass renderPass, DescriptorSetLayout globalSetLayout, IWindow window)
    {
        this.vk = vk;
        this.device = device;
        view = window;
        inputContext = window.CreateInput();
        physicalDevice = device.VkPhysicalDevice;
        graphicsFamilyIndex = device.GraphicsFamilyIndex;
        swapChainImageCt = LveSwapChain.MAX_FRAMES_IN_FLIGHT;
        
        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        
        var io = ImGui.GetIO();
        io.Fonts.AddFontDefault();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        windowWidth = window.Size.X;
        windowHeight = window.Size.Y;
        framebufferSize = window.FramebufferSize;


        createPipelineLayout(globalSetLayout);
        createPipeline(renderPass);

        init();
        

        SetKeyMappings();

        SetPerFrameImGuiData(1f / 60f);

        

    }

    private unsafe void init()
    {
        // Set default style
        ImGui.StyleColorsDark();

        var info = new SamplerCreateInfo();
        info.SType = StructureType.SamplerCreateInfo;
        info.MagFilter = Filter.Linear;
        info.MinFilter = Filter.Linear;
        info.MipmapMode = SamplerMipmapMode.Linear;
        info.AddressModeU = SamplerAddressMode.Repeat;
        info.AddressModeV = SamplerAddressMode.Repeat;
        info.AddressModeW = SamplerAddressMode.Repeat;
        info.MinLod = -1000;
        info.MaxLod = 1000;
        info.MaxAnisotropy = 1.0f;
        if (vk.CreateSampler(device.VkDevice, info, default, out _fontSampler) != Result.Success)
        {
            throw new Exception($"Unable to create sampler");
        }

        //var sampler = _fontSampler;





    }


    private unsafe void createPipelineLayout(DescriptorSetLayout globalSetLayout)
    {
        var descriptorSetLayouts = new DescriptorSetLayout[] { globalSetLayout };
        PushConstantRange pushConstantRange = new()
        {
            StageFlags = ShaderStageFlags.VertexBit,
            Offset = sizeof(float) * 0,
            Size = sizeof(float) * 4,
        };


        fixed (DescriptorSetLayout* descriptorSetLayoutPtr = descriptorSetLayouts)
        {
            PipelineLayoutCreateInfo pipelineLayoutInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = (uint)descriptorSetLayouts.Length,
                PSetLayouts = descriptorSetLayoutPtr,
                PushConstantRangeCount = 1,
                PPushConstantRanges = &pushConstantRange,
            };

            if (vk.CreatePipelineLayout(device.VkDevice, pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
            {
                throw new Exception("failed to create pipeline layout!");
            }
        }
    }


    private unsafe void createPipeline(RenderPass renderPass)
    {
        Debug.Assert(pipelineLayout.Handle != 0, "Cannot create pipeline before pipeline layout");

        var pipelineConfig = LvePipeline.GetDefaultPipelineConfigInfo();

        LvePipeline.EnableMultiSampling(ref pipelineConfig, device.GetMsaaSamples());
        
        pipelineConfig.RenderPass = renderPass;
        pipelineConfig.PipelineLayout = pipelineLayout;
        
        pipeline = new LvePipeline(
            vk, device,
            "imGui.vert.spv",
            "imGui.frag.spv",
            pipelineConfig
            );
            
        
        //log.d("app run", " got pipeline");
    }





    public unsafe void Render(FrameInfo frameInfo)
    {
        pipeline.Bind(frameInfo.CommandBuffer);

        vk.CmdBindDescriptorSets(
            frameInfo.CommandBuffer,
            PipelineBindPoint.Graphics,
            pipelineLayout,
            0,
            1,
            frameInfo.GlobalDescriptorSet,
            0,
            null
        );
        
        


    }


    private static void SetKeyMappings()
    {
        var io = ImGuiNET.ImGui.GetIO();
        io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
        io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
        io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
        io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
        io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
        io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
        io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
        io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
        io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
        io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
        io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.Backspace;
        io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
        io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
        io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
        io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
        io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
        io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
        io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
        io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
    }
    
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGuiNET.ImGui.GetIO();
        io.DisplaySize = new Vector2(windowWidth, windowHeight);

        if (windowWidth > 0 && windowHeight > 0)
        {
            io.DisplayFramebufferScale = new Vector2(framebufferSize.X / windowWidth, framebufferSize.Y / windowHeight);
        }
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ImGuiRenderSystem()
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

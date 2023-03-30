namespace Sandbox04MeshShaders;

public enum GraphicsPipelineTypes
{
    Std,
    Mesh
}

public class LvePipeline : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;

    private Pipeline graphicsPipeline;
    public Pipeline VkPipeline => graphicsPipeline;

    private GraphicsPipelineTypes pipelineType = GraphicsPipelineTypes.Std;
    public GraphicsPipelineTypes PipelineType => pipelineType;

    private ShaderModule vertShaderModule;
    private ShaderModule fragShaderModule;
    private ShaderModule taskShaderModule;
    private ShaderModule meshShaderModule;

    // Constructors

    // Std Vertex/Fragment Pipeline
    public LvePipeline(Vk vk, LveDevice device, string vertPath, string fragPath, PipelineConfigInfo configInfo, string renderSystemName = "unknown")
    {
        this.vk = vk;
        this.device = device;
        createGraphicsPipelineStd(vertPath, fragPath, configInfo, renderSystemName);
    }

    // Mesh Task/MeshFragment Pipeline
    public LvePipeline(Vk vk, LveDevice device, string taskPath, string meshPath, string fragPath, PipelineConfigInfo configInfo, string renderSystemName = "unknown")
    {
        this.vk = vk;
        this.device = device;
        createGraphicsPipelineMesh(taskPath, meshPath, fragPath, configInfo, renderSystemName);
    }
    //

    public void Bind(CommandBuffer commandBuffer)
    {
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, graphicsPipeline);
    }

    private unsafe void createGraphicsPipelineStd(string vertPath, string fragPath, PipelineConfigInfo configInfo, string renderSystemName)
    {
        var vertBytes = FileUtil.GetShaderBytes(vertPath, renderSystemName);
        var fragBytes = FileUtil.GetShaderBytes(fragPath, renderSystemName);
        vertShaderModule = createShaderModule(vertBytes);
        fragShaderModule = createShaderModule(fragBytes);

        PipelineShaderStageCreateInfo vertShaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.VertexBit,
            Module = vertShaderModule,
            PName = (byte*)SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        PipelineShaderStageCreateInfo fragShaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.FragmentBit,
            Module = fragShaderModule,
            PName = (byte*)SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        var shaderStages = stackalloc[]
        {
            vertShaderStageInfo,
            fragShaderStageInfo
        };

        var bindingDescriptions = configInfo.BindingDescriptions;
        var attributeDescriptions = configInfo.AttributeDescriptions;


        fixed (VertexInputBindingDescription* bindingDescriptionsPtr = bindingDescriptions)
        fixed (VertexInputAttributeDescription* attributeDescriptionsPtr = attributeDescriptions)
        {

            var vertextInputInfo = new PipelineVertexInputStateCreateInfo()
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length,
                VertexBindingDescriptionCount = (uint)bindingDescriptions.Length,
                PVertexAttributeDescriptions = attributeDescriptionsPtr,
                PVertexBindingDescriptions = bindingDescriptionsPtr,
            };

            // stole this from ImGui controller, pulled this out of the default pipelineConfig and constructor
            Span<DynamicState> dynamic_states = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };
            var dynamic_state = new PipelineDynamicStateCreateInfo();
            dynamic_state.SType = StructureType.PipelineDynamicStateCreateInfo;
            dynamic_state.DynamicStateCount = (uint)dynamic_states.Length;
            dynamic_state.PDynamicStates = (DynamicState*)Unsafe.AsPointer(ref dynamic_states[0]);


            var pipelineInfo = new GraphicsPipelineCreateInfo()
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = shaderStages,
                PVertexInputState = &vertextInputInfo,
                PInputAssemblyState = &configInfo.InputAssemblyInfo,
                PViewportState = &configInfo.ViewportInfo,
                PRasterizationState = &configInfo.RasterizationInfo,
                PMultisampleState = &configInfo.MultisampleInfo,
                PColorBlendState = &configInfo.ColorBlendInfo,
                PDepthStencilState = &configInfo.DepthStencilInfo,
                PDynamicState = (PipelineDynamicStateCreateInfo*)Unsafe.AsPointer(ref dynamic_state),

                Layout = configInfo.PipelineLayout,
                RenderPass = configInfo.RenderPass,
                Subpass = configInfo.Subpass,

                BasePipelineIndex = -1,
                BasePipelineHandle = default
            };

            if (vk.CreateGraphicsPipelines(device.VkDevice, default, 1, pipelineInfo, default, out graphicsPipeline) != Result.Success)
            {
                throw new Exception("failed to create graphics pipeline!");
            }

        }

        vk.DestroyShaderModule(device.VkDevice, fragShaderModule, null);
        vk.DestroyShaderModule(device.VkDevice, vertShaderModule, null);

        SilkMarshal.Free((nint)shaderStages[0].PName);
        SilkMarshal.Free((nint)shaderStages[1].PName);

    }
    private unsafe void createGraphicsPipelineMesh(string taskPath, string meshPath, string fragPath, PipelineConfigInfo configInfo, string renderSystemName)
    {
        var taskBytes = FileUtil.GetShaderBytes(taskPath, renderSystemName);
        var meshBytes = FileUtil.GetShaderBytes(meshPath, renderSystemName);
        var fragBytes = FileUtil.GetShaderBytes(fragPath, renderSystemName);
        taskShaderModule = createShaderModule(taskBytes);
        meshShaderModule = createShaderModule(meshBytes);
        fragShaderModule = createShaderModule(fragBytes);

        PipelineShaderStageCreateInfo meshShaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.MeshBitNV,
            Module = meshShaderModule,
            PName = (byte*)SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        PipelineShaderStageCreateInfo taskShaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.TaskBitNV,
            Module = taskShaderModule,
            PName = (byte*)SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        PipelineShaderStageCreateInfo fragShaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.FragmentBit,
            Module = fragShaderModule,
            PName = (byte*)SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        var shaderStages = stackalloc[]
        {
            meshShaderStageInfo,
            taskShaderStageInfo,
            fragShaderStageInfo
        };


        // stole this from ImGui controller, pulled this out of the default pipelineConfig and constructor
        Span<DynamicState> dynamic_states = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };
        var dynamic_state = new PipelineDynamicStateCreateInfo();
        dynamic_state.SType = StructureType.PipelineDynamicStateCreateInfo;
        dynamic_state.DynamicStateCount = (uint)dynamic_states.Length;
        dynamic_state.PDynamicStates = (DynamicState*)Unsafe.AsPointer(ref dynamic_states[0]);



        var pipelineInfo = new GraphicsPipelineCreateInfo()
        {
            SType = StructureType.GraphicsPipelineCreateInfo,
            StageCount = 3,
            PStages = shaderStages,
            PVertexInputState = null,
            PInputAssemblyState = null,
            PViewportState = &configInfo.ViewportInfo,
            PRasterizationState = &configInfo.RasterizationInfo,
            PMultisampleState = &configInfo.MultisampleInfo,
            PColorBlendState = &configInfo.ColorBlendInfo,
            PDepthStencilState = &configInfo.DepthStencilInfo,
            PDynamicState = (PipelineDynamicStateCreateInfo*)Unsafe.AsPointer(ref dynamic_state),

            Layout = configInfo.PipelineLayout,
            RenderPass = configInfo.RenderPass,
            Subpass = configInfo.Subpass,

            BasePipelineIndex = -1,
            BasePipelineHandle = default
        };

        if (vk.CreateGraphicsPipelines(device.VkDevice, default, 1, pipelineInfo, default, out graphicsPipeline) != Result.Success)
        {
            throw new Exception("failed to create graphics pipeline!");
        }


        vk.DestroyShaderModule(device.VkDevice, meshShaderModule, null);
        vk.DestroyShaderModule(device.VkDevice, taskShaderModule, null);
        vk.DestroyShaderModule(device.VkDevice, fragShaderModule, null);

        SilkMarshal.Free((nint)shaderStages[0].PName);
        SilkMarshal.Free((nint)shaderStages[1].PName);
        SilkMarshal.Free((nint)shaderStages[2].PName);

    }


    private unsafe ShaderModule createShaderModule(byte[] code)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (nuint)code.Length,
        };

        ShaderModule shaderModule;

        fixed (byte* codePtr = code)
        {
            createInfo.PCode = (uint*)codePtr;

            if (vk.CreateShaderModule(device.VkDevice, createInfo, null, out shaderModule) != Result.Success)
            {
                throw new Exception();
            }
        }

        return shaderModule;
    }



    // Default PipelineConfig
    public unsafe static void DefaultPipelineConfigInfo(ref PipelineConfigInfo configInfo)
    {
        configInfo.InputAssemblyInfo.SType = StructureType.PipelineInputAssemblyStateCreateInfo;
        configInfo.InputAssemblyInfo.Topology = PrimitiveTopology.TriangleList;
        configInfo.InputAssemblyInfo.PrimitiveRestartEnable = Vk.False; //imgui


        configInfo.ViewportInfo.SType = StructureType.PipelineViewportStateCreateInfo;
        configInfo.ViewportInfo.ViewportCount = 1;
        configInfo.ViewportInfo.PViewports = default; // imgui
        configInfo.ViewportInfo.ScissorCount = 1;
        configInfo.ViewportInfo.PScissors = default;  // imgui


        configInfo.RasterizationInfo.SType = StructureType.PipelineRasterizationStateCreateInfo;
        configInfo.RasterizationInfo.DepthClampEnable = Vk.False;
        configInfo.RasterizationInfo.RasterizerDiscardEnable = Vk.False;
        configInfo.RasterizationInfo.PolygonMode = PolygonMode.Fill;
        configInfo.RasterizationInfo.LineWidth = 1f;
        configInfo.RasterizationInfo.CullMode = CullModeFlags.None;
        configInfo.RasterizationInfo.FrontFace = FrontFace.CounterClockwise;
        configInfo.RasterizationInfo.DepthBiasEnable = Vk.False;
        configInfo.RasterizationInfo.DepthBiasConstantFactor = 0f;
        configInfo.RasterizationInfo.DepthBiasClamp = 0f;
        configInfo.RasterizationInfo.DepthBiasSlopeFactor = 0f;


        configInfo.MultisampleInfo.SType = StructureType.PipelineMultisampleStateCreateInfo;
        configInfo.MultisampleInfo.SampleShadingEnable = Vk.False;
        configInfo.MultisampleInfo.RasterizationSamples = SampleCountFlags.Count1Bit;
        configInfo.MultisampleInfo.MinSampleShading = 1.0f;
        configInfo.MultisampleInfo.PSampleMask = default;
        configInfo.MultisampleInfo.AlphaToCoverageEnable = Vk.False;
        configInfo.MultisampleInfo.AlphaToOneEnable = Vk.False;


        configInfo.ColorBlendAttachment.BlendEnable = Vk.False;
        configInfo.ColorBlendAttachment.SrcColorBlendFactor = BlendFactor.One;
        configInfo.ColorBlendAttachment.DstColorBlendFactor = BlendFactor.Zero;
        configInfo.ColorBlendAttachment.ColorBlendOp = BlendOp.Add;
        configInfo.ColorBlendAttachment.SrcAlphaBlendFactor = BlendFactor.One;
        configInfo.ColorBlendAttachment.DstAlphaBlendFactor = BlendFactor.Zero;
        configInfo.ColorBlendAttachment.AlphaBlendOp = BlendOp.Add;
        configInfo.ColorBlendAttachment.ColorWriteMask =
            ColorComponentFlags.RBit | ColorComponentFlags.GBit |
            ColorComponentFlags.BBit | ColorComponentFlags.ABit;

        configInfo.ColorBlendInfo.SType = StructureType.PipelineColorBlendStateCreateInfo;
        configInfo.ColorBlendInfo.LogicOpEnable = Vk.False;
        configInfo.ColorBlendInfo.LogicOp = LogicOp.Copy;
        configInfo.ColorBlendInfo.AttachmentCount = 1;
        configInfo.ColorBlendInfo.PAttachments = (PipelineColorBlendAttachmentState*)Unsafe.AsPointer(ref configInfo.ColorBlendAttachment);

        configInfo.ColorBlendInfo.BlendConstants[0] = 0;
        configInfo.ColorBlendInfo.BlendConstants[1] = 0;
        configInfo.ColorBlendInfo.BlendConstants[2] = 0;
        configInfo.ColorBlendInfo.BlendConstants[3] = 0;


        configInfo.DepthStencilInfo.SType = StructureType.PipelineDepthStencilStateCreateInfo;
        configInfo.DepthStencilInfo.DepthTestEnable = Vk.True;
        configInfo.DepthStencilInfo.DepthWriteEnable = Vk.True;
        configInfo.DepthStencilInfo.DepthCompareOp = CompareOp.Less;
        configInfo.DepthStencilInfo.DepthBoundsTestEnable = Vk.False;
        configInfo.DepthStencilInfo.MinDepthBounds = 0.0f;
        configInfo.DepthStencilInfo.MaxDepthBounds = 1.0f;
        configInfo.DepthStencilInfo.StencilTestEnable = Vk.False;
        configInfo.DepthStencilInfo.Front = default;
        configInfo.DepthStencilInfo.Back = default;

        configInfo.BindingDescriptions = Vertex.GetBindingDescriptions();
        configInfo.AttributeDescriptions = Vertex.GetAttributeDescriptions();

        // pulled dynamic state stuff out of here 

        //var dynamicStateEnables = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };
        //Span<DynamicState> dynamic_states = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };

        //PipelineDynamicStateCreateInfo dynamicState = new()
        //{

        //configInfo.DynamicStateInfo.SType = StructureType.PipelineDynamicStateCreateInfo;
        //configInfo.DynamicStateInfo.PDynamicStates = (DynamicState*)Unsafe.AsPointer(ref dynamic_states[0]);
        //configInfo.DynamicStateInfo.DynamicStateCount = (uint)dynamic_states.Length;
        //configInfo.DynamicStateInfo.Flags = 0;
        //};


        //return configInfo with
        //{
        //    InputAssemblyInfo = inputAssembly,
        //    //Viewport = viewport,
        //    //Scissor = scissor,
        //    ViewportInfo = viewportInfo,
        //    RasterizationInfo = rasterizer,
        //    MultisampleInfo = multisampling,
        //    ColorBlendAttachment = colorBlendAttachment,
        //    ColorBlendInfo = colorBlending,
        //    DepthStencilInfo = depthStencil,
        //    DynamicStateInfo = dynamicState,
        //};
    }

    public static void EnableAlphaBlending(ref PipelineConfigInfo configInfo)
    {
        configInfo.ColorBlendAttachment.BlendEnable = Vk.True;
        configInfo.ColorBlendAttachment.SrcColorBlendFactor = BlendFactor.SrcAlpha;
        configInfo.ColorBlendAttachment.DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha;
        configInfo.ColorBlendAttachment.ColorBlendOp = BlendOp.Add;
        configInfo.ColorBlendAttachment.SrcAlphaBlendFactor = BlendFactor.One;
        configInfo.ColorBlendAttachment.DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha;
        configInfo.ColorBlendAttachment.AlphaBlendOp = BlendOp.Add;
        configInfo.ColorBlendAttachment.ColorWriteMask =
            ColorComponentFlags.RBit | ColorComponentFlags.GBit |
            ColorComponentFlags.BBit | ColorComponentFlags.ABit;

    }
    public static void EnableMultiSampling(ref PipelineConfigInfo configInfo, SampleCountFlags msaaSamples)
    {
        configInfo.MultisampleInfo.RasterizationSamples = msaaSamples;
    }


    public unsafe void Dispose()
    {
        vk.DestroyShaderModule(device.VkDevice, vertShaderModule, null);
        vk.DestroyShaderModule(device.VkDevice, fragShaderModule, null);
        vk.DestroyPipeline(device.VkDevice, graphicsPipeline, null);

        GC.SuppressFinalize(this);
    }

}

public struct PipelineConfigInfo
{
    public VertexInputBindingDescription[] BindingDescriptions;
    public VertexInputAttributeDescription[] AttributeDescriptions;


    //public Viewport Viewport;
    //public Rect2D Scissor;
    public PipelineViewportStateCreateInfo ViewportInfo;
    public PipelineInputAssemblyStateCreateInfo InputAssemblyInfo;
    public PipelineRasterizationStateCreateInfo RasterizationInfo;
    public PipelineMultisampleStateCreateInfo MultisampleInfo;
    public PipelineColorBlendAttachmentState ColorBlendAttachment;
    public PipelineColorBlendStateCreateInfo ColorBlendInfo;
    public PipelineDepthStencilStateCreateInfo DepthStencilInfo;
    //public DynamicState[] DynamicStateEnables;

    //public PipelineDynamicStateCreateInfo DynamicStateInfo;
    public PipelineLayout PipelineLayout; // no default to be set
    public RenderPass RenderPass; // no default to be set
    public uint Subpass;
    //public DynamicState[] DynamicStateEnables; //= stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };

    public PipelineConfigInfo()
    {
        Subpass = 0;
        BindingDescriptions = Array.Empty<VertexInputBindingDescription>();
        AttributeDescriptions = Array.Empty<VertexInputAttributeDescription>();


        //PipelineDynamicStateCreateInfo dynamicState = new()
        //{
        //unsafe
        //{
        //    Span<DynamicState> dynamic_states = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };
        //    DynamicStateEnables = dynamic_states.ToArray();
        //    DynamicStateInfo.PDynamicStates = (DynamicState*)Unsafe.AsPointer(ref DynamicStateEnables[0]);
        //}

        //DynamicStateEnables = Array.Empty<DynamicState>();
    }
}
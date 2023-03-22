namespace Chapter21PointLights;

public class LvePipeline : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;

    private Pipeline graphicsPipeline;
    public Pipeline VkPipeline => graphicsPipeline;

    private ShaderModule vertShaderModule;
    private ShaderModule fragShaderModule;
    private bool disposedValue;


    public LvePipeline(Vk vk, LveDevice device, string vertPath, string fragPath, PipelineConfigInfo configInfo)
    {
        this.vk = vk;
        this.device = device;
        createGraphicsPipeline(vertPath, fragPath, configInfo);
    }

    public void Bind(CommandBuffer commandBuffer)
    {
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, graphicsPipeline);
    }

    private unsafe void createGraphicsPipeline(string vertPath, string fragPath, PipelineConfigInfo configInfo)
    {
        var vertSource = getShaderBytes(vertPath);
        var fragSource = getShaderBytes(fragPath);

        vertShaderModule = createShaderModule(vertSource);
        fragShaderModule = createShaderModule(fragSource);

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

        var bindingDescriptions = Vertex.GetBindingDescriptions();
        var attributeDescriptions = Vertex.GetAttributeDescriptions();


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


    private static byte[] getShaderBytes(string filename)
    {
        //foreach (var item in assembly.GetManifestResourceNames())
        //{
        //    Console.WriteLine($"{item}");
        //}
        //var resourceName = $"Chapter05SwapChain.{filename.Replace('/', '.')}";
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(filename));
        if (resourceName is null) throw new ApplicationException($"*** No shader file found with name {filename}\n*** Check that resourceName and try again!  Did you forget to set glsl file to Embedded Resource/Do Not Copy?");

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ApplicationException($"*** No shader file found at {resourceName}\n*** Check that resourceName and try again!  Did you forget to set glsl file to Embedded Resource/Do Not Copy?");
        using var ms = new MemoryStream();
        if (stream is null) return Array.Empty<byte>();
        stream.CopyTo(ms);
        return ms.ToArray();

    }




    // Default PipelineConfig
    public unsafe static void DefaultPipelineConfigInfo(ref PipelineConfigInfo configInfo)
    {
        configInfo.InputAssemblyInfo.SType = StructureType.PipelineInputAssemblyStateCreateInfo;
        configInfo.InputAssemblyInfo.Topology = PrimitiveTopology.TriangleList;
        configInfo.InputAssemblyInfo.PrimitiveRestartEnable = false;


        configInfo.ViewportInfo.SType = StructureType.PipelineViewportStateCreateInfo;
        configInfo.ViewportInfo.ViewportCount = 1;
        configInfo.ViewportInfo.PViewports = default;
        configInfo.ViewportInfo.ScissorCount = 1;
        configInfo.ViewportInfo.PScissors = default;


        configInfo.RasterizationInfo.SType = StructureType.PipelineRasterizationStateCreateInfo;
        configInfo.RasterizationInfo.DepthClampEnable = false;
        configInfo.RasterizationInfo.RasterizerDiscardEnable = false;
        configInfo.RasterizationInfo.PolygonMode = PolygonMode.Fill;
        configInfo.RasterizationInfo.LineWidth = 1f;
        configInfo.RasterizationInfo.CullMode = CullModeFlags.None;
        configInfo.RasterizationInfo.FrontFace = FrontFace.CounterClockwise;
        configInfo.RasterizationInfo.DepthBiasEnable = false;
        configInfo.RasterizationInfo.DepthBiasConstantFactor = 0f;
        configInfo.RasterizationInfo.DepthBiasClamp = 0f;
        configInfo.RasterizationInfo.DepthBiasSlopeFactor = 0f;


        configInfo.MultisampleInfo.SType = StructureType.PipelineMultisampleStateCreateInfo;
        configInfo.MultisampleInfo.SampleShadingEnable = false;
        configInfo.MultisampleInfo.RasterizationSamples = SampleCountFlags.Count1Bit;
        configInfo.MultisampleInfo.MinSampleShading = 1.0f;
        configInfo.MultisampleInfo.PSampleMask = default;
        configInfo.MultisampleInfo.AlphaToCoverageEnable = false;
        configInfo.MultisampleInfo.AlphaToOneEnable = false;

        configInfo.ColorBlendAttachment.ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit;
        configInfo.ColorBlendAttachment.BlendEnable = false;
        configInfo.ColorBlendAttachment.SrcColorBlendFactor = BlendFactor.One;
        configInfo.ColorBlendAttachment.DstColorBlendFactor = BlendFactor.Zero;
        configInfo.ColorBlendAttachment.ColorBlendOp = BlendOp.Add;
        configInfo.ColorBlendAttachment.SrcAlphaBlendFactor = BlendFactor.One;
        configInfo.ColorBlendAttachment.DstAlphaBlendFactor = BlendFactor.Zero;
        configInfo.ColorBlendAttachment.AlphaBlendOp = BlendOp.Add;

        configInfo.ColorBlendInfo.SType = StructureType.PipelineColorBlendStateCreateInfo;
        configInfo.ColorBlendInfo.LogicOpEnable = false;
        configInfo.ColorBlendInfo.LogicOp = LogicOp.Copy;
        configInfo.ColorBlendInfo.AttachmentCount = 1;
        configInfo.ColorBlendInfo.PAttachments = (PipelineColorBlendAttachmentState*)Unsafe.AsPointer(ref configInfo.ColorBlendAttachment);

        configInfo.ColorBlendInfo.BlendConstants[0] = 0;
        configInfo.ColorBlendInfo.BlendConstants[1] = 0;
        configInfo.ColorBlendInfo.BlendConstants[2] = 0;
        configInfo.ColorBlendInfo.BlendConstants[3] = 0;


        configInfo.DepthStencilInfo.SType = StructureType.PipelineDepthStencilStateCreateInfo;
        configInfo.DepthStencilInfo.DepthTestEnable = true;
        configInfo.DepthStencilInfo.DepthWriteEnable = true;
        configInfo.DepthStencilInfo.DepthCompareOp = CompareOp.Less;
        configInfo.DepthStencilInfo.DepthBoundsTestEnable = false;
        configInfo.DepthStencilInfo.MinDepthBounds = 0.0f;
        configInfo.DepthStencilInfo.MaxDepthBounds = 1.0f;
        configInfo.DepthStencilInfo.StencilTestEnable = false;
        configInfo.DepthStencilInfo.Front = default;
        configInfo.DepthStencilInfo.Back = default;


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


    #region Dispose
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
    // ~LvePipeline()
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
    #endregion
}

public struct PipelineConfigInfo
{
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
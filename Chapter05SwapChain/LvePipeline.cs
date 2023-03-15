namespace Chapter05SwapChain;

public class LvePipeline : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;

    private Pipeline graphicsPipeline;
    private ShaderModule vertShaderModule;
    private ShaderModule fragShaderModule;
    private bool disposedValue;

    public LvePipeline(Vk vk, LveDevice device, string vertPath, string fragPath, PipelineConfigInfo configInfo)
    {
        this.vk = vk;
        this.device = device;
        createGraphicsPipeline(vertPath, fragPath, configInfo);
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

        //var shaderStages = new PipelineShaderStageCreateInfo[2];

        //// vertex shader stage
        //shaderStages[0].SType = StructureType.PipelineShaderStageCreateInfo;
        //shaderStages[0].Stage = ShaderStageFlags.VertexBit;
        //shaderStages[0].Module = vertShaderModule;
        //shaderStages[0].PName = (byte*)SilkMarshal.StringToPtr("main");
        //shaderStages[0].Flags = PipelineShaderStageCreateFlags.None;
        //shaderStages[0].PNext = null;
        //shaderStages[0].PSpecializationInfo = null;

        //// frag shader stage
        //shaderStages[1].SType = StructureType.PipelineShaderStageCreateInfo;
        //shaderStages[1].Stage = ShaderStageFlags.FragmentBit;
        //shaderStages[1].Module = fragShaderModule;
        //shaderStages[1].PName = (byte*)SilkMarshal.StringToPtr("main");
        //shaderStages[1].Flags = PipelineShaderStageCreateFlags.None;
        //shaderStages[1].PNext = null;
        //shaderStages[1].PSpecializationInfo = null;

        var vertextInputInfo = new PipelineVertexInputStateCreateInfo()
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo,
            VertexAttributeDescriptionCount = 0,
            VertexBindingDescriptionCount = 0,
            PVertexAttributeDescriptions = null,
            PVertexBindingDescriptions = null,
        };


        var viewportInfo = new PipelineViewportStateCreateInfo()
        {
            SType = StructureType.PipelineViewportStateCreateInfo,
            ViewportCount = 1,
            PViewports = &configInfo.Viewport,
            ScissorCount = 1,
            PScissors = &configInfo.Scissor,
        };


        var pipelineInfo = new GraphicsPipelineCreateInfo()
        {
            SType = StructureType.GraphicsPipelineCreateInfo,
            StageCount = 2,
            PStages = shaderStages,
            PVertexInputState = &vertextInputInfo,
            PInputAssemblyState = &configInfo.InputAssemblyInfo,
            PViewportState = &viewportInfo,
            PRasterizationState = &configInfo.RasterizationInfo,
            PColorBlendState = &configInfo.ColorBlendInfo,
            PDepthStencilState = &configInfo.DepthStencilInfo,
            PDynamicState = null,
            Layout = configInfo.PipelineLayout, // 
            RenderPass = configInfo.RenderPass, // 
            Subpass = configInfo.Subpass,       //
            BasePipelineIndex = -1,
            BasePipelineHandle = default
        };

        if (vk.CreateGraphicsPipelines(device.VkDevice, default, 1, pipelineInfo, null, out graphicsPipeline) != Result.Success)
        {
            throw new Exception("failed to create graphics pipeline!");
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
    public unsafe static PipelineConfigInfo DefaultPipelineConfigInfo(uint width, uint height)
    {
        PipelineInputAssemblyStateCreateInfo inputAssembly = new()
        {
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = PrimitiveTopology.TriangleList,
            PrimitiveRestartEnable = false,
        };


        Viewport viewport = new()
        {
            X = 0,
            Y = 0,
            Width = width,
            Height = height,
            MinDepth = 0,
            MaxDepth = 1,
        };

        Rect2D scissor = new()
        {
            Offset = { X = 0, Y = 0 },
            Extent = new(width, height),
        };


        PipelineRasterizationStateCreateInfo rasterizer = new()
        {
            SType = StructureType.PipelineRasterizationStateCreateInfo,
            DepthClampEnable = false,
            RasterizerDiscardEnable = false,
            PolygonMode = PolygonMode.Fill,
            LineWidth = 1f,
            CullMode = CullModeFlags.None,
            FrontFace = FrontFace.CounterClockwise,
            DepthBiasEnable = false,
            DepthBiasConstantFactor = 0f,
            DepthBiasClamp = 0f,
            DepthBiasSlopeFactor = 0f,
        };

        PipelineMultisampleStateCreateInfo multisampling = new()
        {
            SType = StructureType.PipelineMultisampleStateCreateInfo,
            SampleShadingEnable = false,
            RasterizationSamples = SampleCountFlags.Count1Bit,
            MinSampleShading = 1.0f,
            PSampleMask = null,
            AlphaToCoverageEnable = false,
            AlphaToOneEnable = false,
        };

        PipelineColorBlendAttachmentState colorBlendAttachment = new()
        {
            ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
            BlendEnable = false,
            SrcColorBlendFactor = BlendFactor.One,
            DstColorBlendFactor = BlendFactor.Zero,
            ColorBlendOp = BlendOp.Add,
            SrcAlphaBlendFactor = BlendFactor.One,
            DstAlphaBlendFactor = BlendFactor.Zero,
            AlphaBlendOp = BlendOp.Add,
        };

        PipelineColorBlendStateCreateInfo colorBlending = new()
        {
            SType = StructureType.PipelineColorBlendStateCreateInfo,
            LogicOpEnable = false,
            LogicOp = LogicOp.Copy,
            AttachmentCount = 1,
            PAttachments = &colorBlendAttachment,
        };

        colorBlending.BlendConstants[0] = 0;
        colorBlending.BlendConstants[1] = 0;
        colorBlending.BlendConstants[2] = 0;
        colorBlending.BlendConstants[3] = 0;

        PipelineDepthStencilStateCreateInfo depthStencil = new()
        {
            SType = StructureType.PipelineDepthStencilStateCreateInfo,
            DepthTestEnable = true,
            DepthWriteEnable = true,
            DepthCompareOp = CompareOp.Less,
            DepthBoundsTestEnable = false,
            MinDepthBounds = 0.0f,
            MaxDepthBounds = 1.0f,
            StencilTestEnable = false,
            Front = { },
            Back = { },
        };


        return new PipelineConfigInfo()
        {
            InputAssemblyInfo = inputAssembly,
            Viewport = viewport,
            Scissor = scissor,
            RasterizationInfo = rasterizer,
            MultisampleInfo = multisampling,
            ColorBlendAttachment = colorBlendAttachment,
            ColorBlendInfo = colorBlending,
            DepthStencilInfo = depthStencil
        };
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
    public PipelineInputAssemblyStateCreateInfo InputAssemblyInfo;
    public Viewport Viewport;
    public Rect2D Scissor;
    public PipelineRasterizationStateCreateInfo RasterizationInfo;
    public PipelineMultisampleStateCreateInfo MultisampleInfo;
    public PipelineColorBlendAttachmentState ColorBlendAttachment;
    public PipelineColorBlendStateCreateInfo ColorBlendInfo;
    public PipelineDepthStencilStateCreateInfo DepthStencilInfo;
    public PipelineLayout PipelineLayout; // no default to be set
    public RenderPass RenderPass; // no default to be set
    public uint Subpass;

    public PipelineConfigInfo()
    {
        Subpass = 0;
    }
}
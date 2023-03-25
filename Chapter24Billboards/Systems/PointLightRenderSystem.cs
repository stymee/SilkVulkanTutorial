
namespace Chapter24Billboards;

class PointLightRenderSystem : IDisposable
{
	private readonly Vk vk = null!;
	private readonly LveDevice device = null!;
    private bool disposedValue;

    private LvePipeline pipeline = null!;
    private PipelineLayout pipelineLayout;

    public PointLightRenderSystem(Vk vk, LveDevice device, RenderPass renderPass, DescriptorSetLayout globalSetLayout)
	{
		this.vk = vk;
		this.device = device;
        createPipelineLayout(globalSetLayout);
        createPipeline(renderPass);
	}
    
    private unsafe void createPipelineLayout(DescriptorSetLayout globalSetLayout)
    {
        var descriptorSetLayouts = new DescriptorSetLayout[] { globalSetLayout };

        fixed (DescriptorSetLayout* descriptorSetLayoutPtr = descriptorSetLayouts)
        {
            PipelineLayoutCreateInfo pipelineLayoutInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = (uint)descriptorSetLayouts.Length,
                PSetLayouts = descriptorSetLayoutPtr,
                PushConstantRangeCount = 0,
                PPushConstantRanges = null,
            };

            if (vk.CreatePipelineLayout(device.VkDevice, pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
            {
                throw new Exception("failed to create pipeline layout!");
            }
        }
    }


    private void createPipeline(RenderPass renderPass)
    {
        Debug.Assert(pipelineLayout.Handle != 0, "Cannot create pipeline before pipeline layout");

        //var pipelineConfig = LvePipeline.GetDefaultPipelineConfigInfo();
        var pipelineConfig = new PipelineConfigInfo();
        LvePipeline.DefaultPipelineConfigInfo(ref pipelineConfig);
        pipelineConfig.BindingDescriptions = Array.Empty<VertexInputBindingDescription>();
        pipelineConfig.AttributeDescriptions = Array.Empty<VertexInputAttributeDescription>();
        //Array.Clear(pipelineConfig.BindingDescriptions);
        //Array.Clear(pipelineConfig.AttributeDescriptions);


        pipelineConfig.RenderPass = renderPass;
        pipelineConfig.PipelineLayout = pipelineLayout;
        pipeline = new LvePipeline(
            vk, device,
            "pointLight.vert.spv", 
            "pointLight.frag.spv",
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


        vk.CmdDraw(frameInfo.CommandBuffer, 6, 1, 0, 0);
        

    }

    public unsafe void Dispose()
    {
        pipeline.Dispose();
        vk.DestroyPipelineLayout(device.VkDevice, pipelineLayout, null);
        GC.SuppressFinalize(this);
    }
}



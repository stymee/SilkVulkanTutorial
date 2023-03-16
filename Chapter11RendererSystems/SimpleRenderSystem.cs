
namespace Chapter11RendererSystems;

class SimpleRenderSystem
{
	private readonly Vk vk = null!;
	private readonly LveDevice device = null!;

    private LvePipeline pipeline = null!;
    private PipelineLayout pipelineLayout;
    //private readonly RenderPass renderPass;

    public SimpleRenderSystem(Vk vk, LveDevice device, RenderPass renderPass)
	{
		this.vk = vk;
		this.device = device;
		//this.renderPass = renderPass;

        createPipelineLayout();
        createPipeline(renderPass);
	}
    
    public void RenderGameObjects(CommandBuffer commandBuffer, ref List<LveGameObject> gameObjects)
    {
        pipeline.Bind(commandBuffer);


        foreach (var g in gameObjects)
        {
            g.Transform2d = g.Transform2d with
            {
                Rotation = g.Transform2d.Rotation + .0001f * MathF.Tau
            };
            SimplePushConstantData push = new()
            {
                Offset = new(g.Transform2d.Translation, 0.0f, 0.0f),
                Color = g.Color,
                Transform = g.Transform2d.Mat2()
            };
            vk.CmdPushConstants(commandBuffer, pipelineLayout, ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit, 0, SimplePushConstantData.SizeOf(), ref push);
            g.Model.Bind(commandBuffer);
            g.Model.Draw(commandBuffer);

        }
    }

    private unsafe void createPipelineLayout()
    {
        PushConstantRange pushConstantRange = new()
        {
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            Offset = 0,
            Size = SimplePushConstantData.SizeOf(),
        };


        PipelineLayoutCreateInfo pipelineLayoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 0,
            PSetLayouts = default,
            PushConstantRangeCount = 1,
            PPushConstantRanges = &pushConstantRange,
        };

        if (vk.CreatePipelineLayout(device.VkDevice, pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
        {
            throw new Exception("failed to create pipeline layout!");
        }
    }


    private void createPipeline(RenderPass renderPass)
    {
        var pipelineConfig = new PipelineConfigInfo();
        LvePipeline.DefaultPipelineConfigInfo(ref pipelineConfig);

        pipelineConfig.RenderPass = renderPass;
        pipelineConfig.PipelineLayout = pipelineLayout;
        pipeline = new LvePipeline(
            vk, device,
            "simpleShader.vert.spv", "simpleShader.frag.spv",
            pipelineConfig
            );
        log.d("app run", " got pipeline");
    }


}


public struct SimplePushConstantData
{
    public Matrix2X2<float> Transform;
    public Vector4 Offset;
    public Vector4 Color;

    public SimplePushConstantData()
    {
        Transform = Matrix2X2<float>.Identity;
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<SimplePushConstantData>();
}

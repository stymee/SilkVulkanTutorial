
namespace Chapter19zNoCoherentAomSizeFix;

class SimpleRenderSystem : IDisposable
{
	private readonly Vk vk = null!;
	private readonly LveDevice device = null!;

    private LvePipeline pipeline = null!;
    private PipelineLayout pipelineLayout;

    public SimpleRenderSystem(Vk vk, LveDevice device, RenderPass renderPass)
	{
		this.vk = vk;
		this.device = device;

        createPipelineLayout();
        createPipeline(renderPass);
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
        Debug.Assert(pipelineLayout.Handle != 0, "Cannot create pipeline before pipeline layout");

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





    public void RenderGameObjects(FrameInfo frameInfo, ref List<LveGameObject> gameObjects)
    {
        pipeline.Bind(frameInfo.CommandBuffer);

        var projectionView = frameInfo.Camera.GetViewMatrix() * frameInfo.Camera.GetProjectionMatrix() ;

        foreach (var g in gameObjects)
        {
            var modelMatrix = g.Transform.Mat4();
            SimplePushConstantData push = new()
            {
                //Color = g.Color,
                Transform = modelMatrix * projectionView, // this is reverse from tutorial?
                NormalMatrix = g.Transform.NormalMatrix()
            };
            vk.CmdPushConstants(frameInfo.CommandBuffer, pipelineLayout, ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit, 0, SimplePushConstantData.SizeOf(), ref push);
            g.Model.Bind(frameInfo.CommandBuffer);
            g.Model.Draw(frameInfo.CommandBuffer);

        }
    }

    public unsafe void Dispose()
    {
        pipeline.Dispose();
        vk.DestroyPipelineLayout(device.VkDevice, pipelineLayout, null);
        GC.SuppressFinalize(this);
    }

}


public struct SimplePushConstantData
{
    public Matrix4x4 Transform;
    public Matrix4x4 NormalMatrix;
    //public Vector4 Color;

    public SimplePushConstantData()
    {
        Transform = Matrix4x4.Identity;
        NormalMatrix = Matrix4x4.Identity;
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<SimplePushConstantData>();
}

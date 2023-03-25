
namespace Chapter14CameraViewTransform;

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

    public void RenderGameObjects(CommandBuffer commandBuffer, ref List<LveGameObject> gameObjects, LveCamera camera)
    {
        pipeline.Bind(commandBuffer);

        var projectionView = camera.GetView() * camera.GetProjection() ;

        foreach (var g in gameObjects)
        {
            g.Transform = g.Transform with
            {
                Rotation = g.Transform.Rotation with
                {
                    Y = g.Transform.Rotation.Y + .0001f * MathF.Tau,
                    X = g.Transform.Rotation.X + .00005f * MathF.Tau
                }
            };
            SimplePushConstantData push = new()
            {
                Color = g.Color,
                Transform = g.Transform.Mat4() * projectionView // this is reverse from tutorial?
            };
            vk.CmdPushConstants(commandBuffer, pipelineLayout, ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit, 0, SimplePushConstantData.SizeOf(), ref push);
            g.Model.Bind(commandBuffer);
            g.Model.Draw(commandBuffer);

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
    public Vector4 Color;

    public SimplePushConstantData()
    {
        Transform = Matrix4x4.Identity;
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<SimplePushConstantData>();
}

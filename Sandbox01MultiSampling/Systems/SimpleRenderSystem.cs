
namespace Sandbox01MultiSampling;

class SimpleRenderSystem : IDisposable
{
	private readonly Vk vk = null!;
	private readonly LveDevice device = null!;
    private bool disposedValue;

    private LvePipeline pipeline = null!;
    private PipelineLayout pipelineLayout;

    public SimpleRenderSystem(Vk vk, LveDevice device, RenderPass renderPass, DescriptorSetLayout globalSetLayout)
	{
		this.vk = vk;
		this.device = device;
        createPipelineLayout(globalSetLayout);
        createPipeline(renderPass);
	}
    
    private unsafe void createPipelineLayout(DescriptorSetLayout globalSetLayout)
    {
        var descriptorSetLayouts = new DescriptorSetLayout[] { globalSetLayout };
        PushConstantRange pushConstantRange = new()
        {
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            Offset = 0,
            Size = SimplePushConstantData.SizeOf(),
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


    private void createPipeline(RenderPass renderPass)
    {
        Debug.Assert(pipelineLayout.Handle != 0, "Cannot create pipeline before pipeline layout");

        var pipelineConfig = LvePipeline.GetDefaultPipelineConfigInfo();

        pipelineConfig.RenderPass = renderPass;
        pipelineConfig.PipelineLayout = pipelineLayout;
        pipeline = new LvePipeline(
            vk, device,
            "simpleShader.vert.spv", 
            "simpleShader.frag.spv",
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


        foreach (var (id, g) in frameInfo.GameObjects)
        {
            if (g.Model is null) continue;
            SimplePushConstantData push = new()
            {
                ModelMatrix = g.Transform.Mat4(),
                NormalMatrix = g.Transform.NormalMatrix()
            };
            vk.CmdPushConstants(
                frameInfo.CommandBuffer, 
                pipelineLayout, 
                ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit, 
                0, 
                SimplePushConstantData.SizeOf(), 
                ref push
            );
            g.Model.Bind(frameInfo.CommandBuffer);
            g.Model.Draw(frameInfo.CommandBuffer);

        }
    }

    protected unsafe virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            vk.DestroyPipelineLayout(device.VkDevice, pipelineLayout, null);
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~SimpleRenderSystem()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}


public struct SimplePushConstantData
{
    public Matrix4x4 ModelMatrix;
    public Matrix4x4 NormalMatrix;
    //public Vector4 Color;

    public SimplePushConstantData()
    {
        ModelMatrix = Matrix4x4.Identity;
        NormalMatrix = Matrix4x4.Identity;
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<SimplePushConstantData>();
}

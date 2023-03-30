
namespace Sandbox03MeshShaders;

class PointLightRenderSystem : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;

    private LveStdPipeline pipeline = null!;
    private PipelineLayout pipelineLayout;

    private Dictionary<uint, float> sortedZ = new();


    // public props
    public bool RotateLightsEnabled { get; set; } = true;
    public float RotateSpeed { get; set; } = 1.0f;
    public float LightIntensity { get; set; }  
    public float LightRadius { get; set; }  

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
        PushConstantRange pushConstantRange = new()
        {
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            Offset = 0,
            Size = PointLightPushConstants.SizeOf(),
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

        var pipelineConfig = new PipelineConfigInfo();
        LveStdPipeline.DefaultPipelineConfigInfo(ref pipelineConfig);
        LveStdPipeline.EnableAlphaBlending(ref pipelineConfig);
        
        LveStdPipeline.EnableMultiSampling(ref pipelineConfig, device.GetMsaaSamples());
        
        pipelineConfig.BindingDescriptions = Array.Empty<VertexInputBindingDescription>();
        pipelineConfig.AttributeDescriptions = Array.Empty<VertexInputAttributeDescription>();


        pipelineConfig.RenderPass = renderPass;
        pipelineConfig.PipelineLayout = pipelineLayout;
        pipeline = new LveStdPipeline(
            vk, device,
            "pointLight.vert.spv",
            "pointLight.frag.spv",
            pipelineConfig
            );
        //log.d("app run", " got pipeline");
    }


    public void Update(FrameInfo frameInfo, ref GlobalUbo ubo)
    {
        int lightIndex = 0;
        foreach (var (idx, g) in frameInfo.GameObjects)
        {
            if (g.PointLight is null) continue;

            if (RotateLightsEnabled)
            {
                var rotateLight = Matrix4x4.CreateRotationY(frameInfo.FrameTime * RotateSpeed);
                g.Transform.Translation = Vector3.Transform(g.Transform.Translation, rotateLight);
            }


            ubo.SetPointLightTranslation(lightIndex, g.Transform.Translation);
            ubo.SetPointLightColor(lightIndex, g.Color, g.PointLight.Value.LightIntensity);
            lightIndex++;
        }
        ubo.SetNumLights(lightIndex);
    }


    public unsafe void Render(FrameInfo frameInfo)
    {
        sortedZ.Clear();
        foreach (var (idx, g) in frameInfo.GameObjects)
        {
            if (g.PointLight is null) continue;

            var offset = frameInfo.Camera.FrontVec - g.Transform.Translation;
            float distSquared = Vector3.Dot(offset, offset);
            if (!sortedZ.TryAdd(g.Id, distSquared))
            {
                //Console.WriteLine("error trying to sort point lights by z order!");
                throw new ApplicationException($"error trying to sort point lights by z order!");
            }
        }

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

        foreach (var (gid, dist) in sortedZ.OrderBy(s => s.Value))
        {
            var g = frameInfo.GameObjects[gid];
            if (g.PointLight is null) continue;

            PointLightPushConstants push = new()
            {
                Position = new(g.Transform.Translation, 1f),
                Color = new(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity),
                Radius = g.Transform.Scale.X,
            };
            vk.CmdPushConstants(
                frameInfo.CommandBuffer,
                pipelineLayout,
                ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
                0,
                PointLightPushConstants.SizeOf(),
                ref push
            );
            vk.CmdDraw(frameInfo.CommandBuffer, 6, 1, 0, 0);
        }


    }


    public unsafe void Dispose()
    {
        pipeline.Dispose();
        vk.DestroyPipelineLayout(device.VkDevice, pipelineLayout, null);
        GC.SuppressFinalize(this);
    }


    public struct PointLightPushConstants
    {
        public Vector4 Position;
        public Vector4 Color;
        public float Radius;

        public PointLightPushConstants(Vector4 position, Vector4 color, float radius)
        {
            Position = position;
            Color = color;
            Radius = radius;
        }

        public static uint SizeOf() => (uint)Unsafe.SizeOf<PointLightPushConstants>();

    }
}



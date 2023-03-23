
namespace Chapter26SpecularLighting;

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

        var pipelineConfig = LvePipeline.GetDefaultPipelineConfigInfo();
        pipelineConfig.BindingDescriptions = Array.Empty<VertexInputBindingDescription>();
        pipelineConfig.AttributeDescriptions = Array.Empty<VertexInputAttributeDescription>();


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
    

    public void Update(FrameInfo frameInfo, ref GlobalUbo ubo)
    {
        int lightIndex = 0;
        foreach (var (idx, g) in frameInfo.GameObjects)
        {
            if (g.PointLight is null) continue;

            var rotateLight = Matrix4x4.CreateRotationY(frameInfo.FrameTime);
            g.Transform.Translation = Vector3.Transform(g.Transform.Translation, rotateLight);

            switch (lightIndex + 1)
            {
                case 1:
                    ubo.PointLight1.Position = new Vector4(g.Transform.Translation, 0.0f);
                    ubo.PointLight1.Color = new Vector4(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity);
                    break;
                
                case 2:
                    ubo.PointLight2.Position = new Vector4(g.Transform.Translation, 0.0f);
                    ubo.PointLight2.Color = new Vector4(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity);
                    break;
                
                case 3:
                    ubo.PointLight3.Position = new Vector4(g.Transform.Translation, 0.0f);
                    ubo.PointLight3.Color = new Vector4(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity);
                    break;
                
                case 4:
                    ubo.PointLight4.Position = new Vector4(g.Transform.Translation, 0.0f);
                    ubo.PointLight4.Color = new Vector4(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity);
                    break;
                
                case 5:
                    ubo.PointLight5.Position = new Vector4(g.Transform.Translation, 0.0f);
                    ubo.PointLight5.Color = new Vector4(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity);
                    break;
                
                case 6:
                    ubo.PointLight6.Position = new Vector4(g.Transform.Translation, 0.0f);
                    ubo.PointLight6.Color = new Vector4(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity);
                    break;

                default:
                    break;
                

            };

            //ubo.PointLight1.Position = new Vector4(g.Transform.Translation, 0.0f);
            //ubo.PointLight1.Color = new Vector4(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity);
            //}
            //else if (lightIndex == 1)
            //{
            //    ubo.PointLight2.Position = new Vector4(g.Transform.Translation, 0.0f);
            //    ubo.PointLight2.Color = new Vector4(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity);
            //}

            //ubo.PointLightPositions[lightIndex] = new Vector4(g.Transform.Translation, 0.0f);
            //ubo.PointLightColors[lightIndex] = new Vector4(g.Color.X, g.Color.Y, g.Color.Z, g.PointLight.Value.LightIntensity);
            lightIndex++;
        }
        ubo.NumLights = lightIndex;
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

        foreach (var (idx, g) in frameInfo.GameObjects)
        {
            if (g.PointLight is null) continue;

            PointLightPushConstants push = new()
            {
                Position = new(g.Transform.Translation,1f),
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
    ~PointLightRenderSystem()
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



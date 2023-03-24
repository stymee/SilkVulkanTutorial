
namespace Sandbox02ImGui;

public unsafe class LveDescriptorSetLayout : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    private bool disposedValue;

    private DescriptorSetLayout descriptorSetLayout;
    public DescriptorSetLayout GetDescriptorSetLayout() => descriptorSetLayout;

    private Dictionary<uint, DescriptorSetLayoutBinding> bindings = null!;
    public Dictionary<uint, DescriptorSetLayoutBinding> Bindings => bindings;

    public LveDescriptorSetLayout(Vk vk, LveDevice device, Dictionary<uint, DescriptorSetLayoutBinding> bindings)
    {
        this.vk = vk;
        this.device = device;
        this.bindings = bindings;

        fixed (DescriptorSetLayoutBinding* setLayoutPtr = this.bindings.Values.ToArray())
        {
            DescriptorSetLayoutCreateInfo descriptorSetLayoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint)this.bindings.Count,
                PBindings = setLayoutPtr
            };

            if (vk.CreateDescriptorSetLayout(device.VkDevice, &descriptorSetLayoutInfo, null, out descriptorSetLayout) != Result.Success)
            {
                throw new ApplicationException($"Failed to create descriptor set layout");
            }
        }


    }






    // builder class...
    public class Builder
    {
        private readonly Vk vk = null!;
        private readonly LveDevice device = null!;

        private Dictionary<uint, DescriptorSetLayoutBinding> bindings = new();

        public Builder(Vk vk, LveDevice device)
        {
            this.vk = vk;
            this.device = device;

        }

        public Builder AddBinding(uint binding, DescriptorType descriptorType, ShaderStageFlags stageFlags, uint count = 1)
        {
            //Debug.Assert(bindings.Count(binding) == 0 && "Binding already in use");
            if (bindings.ContainsKey(binding))
            {
                throw new ApplicationException($"Binding {binding} is already in use, can't add");
            }
            DescriptorSetLayoutBinding layoutBinding = new()
            {
                Binding = binding,
                DescriptorType = descriptorType,
                DescriptorCount = count,
                StageFlags = stageFlags
            };
            bindings[binding] = layoutBinding;
            return this;
        }

        public unsafe Builder AddBinding(uint binding, DescriptorType descriptorType, ShaderStageFlags stageFlags, Sampler sampler, uint count = 1)
        {
            //Debug.Assert(bindings.Count(binding) == 0 && "Binding already in use");
            if (bindings.ContainsKey(binding))
            {
                throw new ApplicationException($"Binding {binding} is already in use, can't add");
            }
            DescriptorSetLayoutBinding layoutBinding = new()
            {
                Binding = binding,
                DescriptorType = descriptorType,
                DescriptorCount = count,
                StageFlags = stageFlags,
                PImmutableSamplers = (Sampler*)Unsafe.AsPointer(ref sampler)
            };
            bindings[binding] = layoutBinding;
            return this;
        }

        public LveDescriptorSetLayout Build()
        {
            return new LveDescriptorSetLayout(vk, device, bindings);
        }


    }

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
            vk.DestroyDescriptorSetLayout(device.VkDevice, descriptorSetLayout, null);
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~LveDescriptorSetLayout()
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


namespace Chapter26SpecularLighting;

public unsafe class LveDescriptorPool : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    public LveDevice LveDevice => device;
    private bool disposedValue;

    private DescriptorPool descriptorPool;
    public DescriptorPool GetDescriptorPool() => descriptorPool;

    private DescriptorPoolSize[] poolSizes = null!;// = new();
    private DescriptorPoolCreateFlags poolFlags;// = DescriptorPoolCreateFlags.None;
    private uint maxSets;

    public LveDescriptorPool(Vk vk, LveDevice device, uint maxSets, DescriptorPoolCreateFlags poolFlags, DescriptorPoolSize[] poolSizes)
    {
        this.vk = vk;
        this.device = device;
        this.poolSizes = poolSizes;
        this.maxSets = maxSets;
        this.poolFlags = poolFlags;

        fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
        fixed (DescriptorPoolSize* poolSizesPtr = poolSizes)
        {
            DescriptorPoolCreateInfo descriptorPoolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = poolSizesPtr,
                MaxSets = maxSets,
                Flags = poolFlags
            };

            if (vk.CreateDescriptorPool(device.VkDevice, &descriptorPoolInfo, null, descriptorPoolPtr) != Result.Success)
            {
                throw new ApplicationException($"Failed to create descriptor pool");
            }
        }

    }

    public bool AllocateDescriptorSet(DescriptorSetLayout descriptorSetLayout, ref DescriptorSet descriptorSet)
    {
        var allocInfo = new DescriptorSetAllocateInfo()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = descriptorPool,
            PSetLayouts = &descriptorSetLayout,
            DescriptorSetCount = 1,
        };
        var result = vk.AllocateDescriptorSets(device.VkDevice, allocInfo, out descriptorSet);
        if (result != Result.Success)
        {
            return false;
        }
        return true;
    }

    private void freeDescriptors(ref DescriptorSet[] descriptors)
    {
        vk.FreeDescriptorSets(device.VkDevice, descriptorPool, descriptors);
    }

    private void resetPool()
    {
        vk.ResetDescriptorPool(device.VkDevice, descriptorPool, 0);
    }



    // helper builder class for chaining calls
    public class Builder
    {
        private readonly Vk vk = null!;
        private readonly LveDevice device = null!;

        private List<DescriptorPoolSize> poolSizes = new();
        private DescriptorPoolCreateFlags poolFlags;
        private uint maxSets;

        public Builder(Vk vk, LveDevice device)
        {
            this.vk = vk;
            this.device = device;
        }


        public Builder AddPoolSize(DescriptorType descriptorType, uint count)
        {
            poolSizes.Add(new DescriptorPoolSize(descriptorType, count));
            return this;
        }

        public Builder SetPoolFlags(DescriptorPoolCreateFlags flags)
        {
            poolFlags = flags;
            return this;
        }
        public Builder SetMaxSets(uint count)
        {
            maxSets = count;
            return this;
        }

        public LveDescriptorPool Build()
        {
            return new LveDescriptorPool(vk, device, maxSets, poolFlags, poolSizes.ToArray());
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
            vk.DestroyDescriptorPool(device.VkDevice, descriptorPool, null);
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~LveDescriptorPool()
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

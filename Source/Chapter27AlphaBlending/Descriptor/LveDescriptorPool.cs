
namespace Chapter27AlphaBlending;

public unsafe class LveDescriptorPool : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    public LveDevice LveDevice => device;

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

    public unsafe void Dispose()
    {
        vk.DestroyDescriptorPool(device.VkDevice, descriptorPool, null);
        GC.SuppressFinalize(this);
    }

}

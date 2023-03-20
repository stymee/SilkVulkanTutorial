
using Silk.NET.OpenAL;
using Silk.NET.Vulkan;

namespace Chapter20DescriptorSets;

public unsafe class LveDescriptorPool : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    public LveDevice LveDevice => device;
    private bool disposedValue;

    private DescriptorPool descriptorPool;
    private List<DescriptorPoolSize> poolSizes = new();
    private DescriptorPoolCreateFlags poolFlags = DescriptorPoolCreateFlags.None;
    private uint maxSets;

    public LveDescriptorPool(Vk vk, LveDevice device, uint maxSets, DescriptorPoolCreateFlags poolFlags, DescriptorPoolSize[] poolSizes)
    {
        this.vk = vk;
        this.device = device;
        this.maxSets = maxSets;
        this.poolSizes.AddRange(poolSizes);
        this.poolFlags = poolFlags;
        fixed (DescriptorPoolSize* poolSizesPtr = poolSizes.ToArray())
        {
            DescriptorPoolCreateInfo descriptorPoolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = poolSizesPtr,
                MaxSets = maxSets,
                Flags = poolFlags
            };

            if (vk.CreateDescriptorPool(device.VkDevice, &descriptorPoolInfo, null, out descriptorPool) != Result.Success)
            {
                throw new ApplicationException($"Failed to create descriptor pool");
            }
        }

    }

    public bool AllocateDescriptorSet(DescriptorSetLayout descriptorSetLayout, ref DescriptorSet descriptorSet)
    {
        //fixed (DescriptorSetLayout* pg_DescriptorSetLayout = descriptorSetLayout)
        //{
        var allocInfo = new DescriptorSetAllocateInfo()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = descriptorPool,
            DescriptorSetCount = 1,
            PSetLayouts = &descriptorSetLayout,
        };
        if (vk.AllocateDescriptorSets(device.VkDevice, allocInfo, out descriptorSet) != Result.Success)
        {
            return false;
            //throw new Exception($"Unable to create descriptor sets");
        }
        return true;
        //}

        //DescriptorSetAllocateInfo allocInfo = new()
        //{
        //    SType = StructureType.DescriptorSetAllocateInfo,
        //    DescriptorPool = descriptorPool,
        //    PSetLayouts = &descriptorSetLayout,
        //    DescriptorSetCount = 1
        //};

        //fixed (DescriptorSet* descriptorSetsPtr = descriptorSets)
        //{
        //    if (vk.AllocateDescriptorSets(device.VkDevice, &allocInfo, descriptorSetsPtr) != Result.Success)
        //    {
        //        return false;
        //    }
        //    return true;

        //}
    }

    private void freeDescriptors(DescriptorSet[] descriptors)
    {
        vk.FreeDescriptorSets(device.VkDevice, descriptorPool, descriptors);
    }

    private void resetPool()
    {
        vk.ResetDescriptorPool(device.VkDevice, descriptorPool, 0);
    }


    public class Builder
    {
        //private readonly Vk vk = null!;
        //private readonly LveDevice device = null!;
        private readonly LveDescriptorPool pool = null!;

        public Builder(Vk vk, LveDevice device)
        {
            pool = new LveDescriptorPool(vk, device, 1, DescriptorPoolCreateFlags.None, new DescriptorPoolSize[0]);
        }


        public Builder AddPoolSize(DescriptorType descriptorType, uint count)
        {
            pool.poolSizes.Add(new DescriptorPoolSize(descriptorType, count));

            return this;
        }

        public Builder setPoolFlags(DescriptorPoolCreateFlags flags)
        {
            pool.poolFlags = flags;
            return this;
        }
        public Builder setMaxSets(uint count)
        {
            pool.maxSets = count;
            return this;
        }

        public LveDescriptorPool Build()
        {
            return pool;// new LveDescriptorPool(pool.vk, pool.device, pool.maxSets, pool.poolFlags, pool.poolSizes.ToArray());
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


namespace Chapter20DescriptorSets;

public unsafe class LveDescriptorPool : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    public LveDevice LveDevice => device;
    private bool disposedValue;

    private DescriptorPool descriptorPool;

    public LveDescriptorPool(Vk vk, LveDevice device, uint maxSets, DescriptorPoolCreateFlags poolFlags, DescriptorPoolSize[] poolSizes)
    {
        this.vk = vk;
        this.device = device;

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

            if (vk.CreateDescriptorPool(device.VkDevice, &descriptorPoolInfo, null, out descriptorPool) != Result.Success)
            {
                throw new ApplicationException($"Failed to create descriptor pool");
            }
        }

    }

    public bool AllocateDescriptor(DescriptorSetLayout descriptorSetLayout, DescriptorSet descriptor)
    {
        DescriptorSetAllocateInfo allocInfo = new()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = descriptorPool,
            PSetLayouts = &descriptorSetLayout,
            DescriptorSetCount = 1
        };

        if (vk.AllocateDescriptorSets(device.VkDevice, &allocInfo, &descriptor) != Result.Success)
        {
            return false;
        }
        return true;
    }

    private void freeDescriptors(DescriptorSet[] descriptors)
    {
        vk.FreeDescriptorSets(device.VkDevice, descriptorPool, descriptors);
    }

    private void resetPool()
    {
        vk.ResetDescriptorPool(device.VkDevice, descriptorPool, 0);
    }


    private class Builder
    {
        private readonly Vk vk = null!;
        private readonly LveDevice device = null!;


        public Builder(Vk vk, LveDevice device)
        {
            this.vk = vk;
            this.device = device;

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

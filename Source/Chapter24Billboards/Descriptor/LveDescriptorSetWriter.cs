
namespace Chapter24Billboards;

public unsafe class LveDescriptorSetWriter
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    private LveDescriptorSetLayout setLayout = null!;

    private WriteDescriptorSet[] writes = Array.Empty<WriteDescriptorSet>();

    public LveDescriptorSetWriter(Vk vk, LveDevice device, LveDescriptorSetLayout setLayout)
    {
        this.vk = vk;
        this.device = device;
        this.setLayout = setLayout;
    }

    public LveDescriptorSetWriter WriteBuffer(uint binding, DescriptorBufferInfo bufferInfo)
    {
        if (!setLayout.Bindings.ContainsKey(binding))
        {
            throw new ApplicationException($"Layout does not contain the specified binding at {binding}");
        }

        var bindingDescription = setLayout.Bindings[binding];

        if (bindingDescription.DescriptorCount > 1)
        {
            throw new ApplicationException($"Binding single descriptor info, but binding expects multiple");
        }

        WriteDescriptorSet write = new()
        {
            SType = StructureType.WriteDescriptorSet,
            DescriptorType = bindingDescription.DescriptorType,
            DstBinding = binding,
            PBufferInfo = &bufferInfo,
            DescriptorCount = 1,
        };

        var writesLen = writes.Length;
        Array.Resize(ref writes, writesLen + 1);
        writes[writesLen] = write;
        return this;
    }

    public LveDescriptorSetWriter WriteImage(uint binding, DescriptorImageInfo imageInfo)
    {
        if (!setLayout.Bindings.ContainsKey(binding))
        {
            throw new ApplicationException($"Layout does not contain the specified binding at {binding}");
        }

        var bindingDescription = setLayout.Bindings[binding];

        if (bindingDescription.DescriptorCount > 1)
        {
            throw new ApplicationException($"Binding single descriptor info, but binding expects multiple");
        }

        WriteDescriptorSet write = new()
        {
            SType = StructureType.WriteDescriptorSet,
            DescriptorType = bindingDescription.DescriptorType,
            DstBinding = binding,
            PImageInfo = &imageInfo,
            DescriptorCount = 1,
        };

        var writesLen = writes.Length;
        Array.Resize(ref writes, writesLen + 1);
        writes[writesLen] = write;
        return this;
    }

    public bool Build(LveDescriptorPool pool, DescriptorSetLayout layout, ref DescriptorSet set)
    {
        var success = pool.AllocateDescriptorSet(setLayout.GetDescriptorSetLayout(), ref set);
        if (!success)
        {
            return false;
        }
        overwrite(ref set);
        return true;
    }


    private void overwrite(ref DescriptorSet set)
    {
        for (var i = 0; i < writes.Length; i++)
        {
            writes[i].DstSet = set;
        }
        fixed (WriteDescriptorSet* writesPtr = writes)
        {
            vk.UpdateDescriptorSets(device.VkDevice, (uint)writes.Length, writesPtr, 0, null);
        }
    }

}

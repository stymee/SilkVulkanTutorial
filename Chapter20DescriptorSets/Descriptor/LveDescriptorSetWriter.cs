namespace Chapter20DescriptorSets;

public unsafe class LveDescriptorSetWriter
{
    private readonly Vk vk = null!;
    private LveDescriptorSetLayout setLayout = null!;
    private LveDescriptorPool pool = null!;

    private List<WriteDescriptorSet> writes = new();

    public LveDescriptorSetWriter(Vk vk, LveDescriptorSetLayout setLayout, LveDescriptorPool pool)
    {
        this.vk = vk;
        this.setLayout = setLayout;
        this.pool = pool;
    }

    public LveDescriptorSetWriter writeBuffer(uint binding, DescriptorBufferInfo bufferInfo)
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

        writes.Add(write);
        return this;
    }

    public LveDescriptorSetWriter writeImage(uint binding, DescriptorImageInfo imageInfo)
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

        writes.Add(write);
        return this;
    }

    public bool Build(DescriptorSet set)
    {
        var success = pool.AllocateDescriptor(setLayout.GetDescriptorSetLayout(), set);
        if (!success)
        {
            return false;
        }
        overwrite(set);
        return true;
    }


    private void overwrite(DescriptorSet set)
    {
        for (var i = 0; i < writes.Count; i++)
        {
            var write = writes[i];
            write.DstSet = set;
        }
        fixed (WriteDescriptorSet* writesPtr = writes.ToArray())
        {
            vk.UpdateDescriptorSets(pool.LveDevice.VkDevice, (uint)writes.Count, writesPtr, 0, null);   
        }    
    }

}

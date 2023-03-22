using Silk.NET.Vulkan;

namespace Chapter20DescriptorSets;

public unsafe class LveDescriptorSetWriter
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    private LveDescriptorSetLayout setLayout = null!;
    //private LveDescriptorPool pool = null!;
    private DescriptorPool pool;

    private WriteDescriptorSet[] writes = Array.Empty<WriteDescriptorSet>();

    public LveDescriptorSetWriter(Vk vk, LveDevice device, LveDescriptorSetLayout setLayout)//, DescriptorPool pool)// LveDescriptorPool pool)
    {
        this.vk = vk;
        this.device = device;
        this.setLayout = setLayout;
        //this.pool = pool;
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

    public bool Build(DescriptorPool pool, DescriptorSetLayout layout, ref DescriptorSet[] sets)
    {
        var layouts = new DescriptorSetLayout[LveSwapChain.MAX_FRAMES_IN_FLIGHT];
        //Array.Fill(layouts, descriptorSetLayout);
        Array.Fill(layouts, layout);

        fixed (DescriptorSetLayout* layoutsPtr = layouts)
        {
            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = pool,// globalPool.GetDescriptorPool(),
                DescriptorSetCount = (uint)LveSwapChain.MAX_FRAMES_IN_FLIGHT,
                PSetLayouts = layoutsPtr,
            };

            sets = new DescriptorSet[LveSwapChain.MAX_FRAMES_IN_FLIGHT];
            fixed (DescriptorSet* descriptorSetsPtr = sets)
            {
                var result = vk!.AllocateDescriptorSets(device.VkDevice, allocateInfo, descriptorSetsPtr);
                if (result != Result.Success)
                {
                    throw new Exception("failed to allocate descriptor sets!");
                }
                overwrite(ref sets);
            }
        }

        return true;
        //var success = pool.AllocateDescriptorSet(setLayout.GetDescriptorSetLayout(), ref set);
        //if (!success)
        //{
        //    return false;
        //}
        //return true;
    }


    private void overwrite(ref DescriptorSet set)
    {
        for (var i = 0; i < writes.Length; i++)
        {
            writes[i].DstSet = set;
            //write.DstSet = set;
        }
        fixed (WriteDescriptorSet* writesPtr = writes)
        {
            vk.UpdateDescriptorSets(device.VkDevice, (uint)writes.Length, writesPtr, 0, null);
        }
    }

    private void overwrite(ref DescriptorSet[] sets)
    {
        for (var i = 0; i < writes.Length; i++)
        {
            for (var j = 0; j < sets.Length; j++)
            {
                writes[i].DstSet = sets[j];

            }
            //write.DstSet = set;
        }
        fixed (WriteDescriptorSet* writesPtr = writes)
        {
            vk.UpdateDescriptorSets(device.VkDevice, (uint)writes.Length, writesPtr, 0, null);
        }
    }

}

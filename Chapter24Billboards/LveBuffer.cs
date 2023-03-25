
namespace Chapter24Billboards;

public unsafe class LveBuffer : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    private bool disposedValue;

    private ulong bufferSize;
    public ulong BufferSize => bufferSize;

    private uint instanceCount;
    public uint InstanceCount => instanceCount;

    private ulong instanceSize;
    public ulong InstanceSize => instanceSize;

    private ulong alignmentSize;
    public ulong AlignmentSize => alignmentSize;


    private BufferUsageFlags usageFlags;
    public BufferUsageFlags UsageFlags => usageFlags;

    private MemoryPropertyFlags memoryPropertyFlags;
    public MemoryPropertyFlags MemoryPropertyFlags => memoryPropertyFlags;

    private void* mapped = null;
    private Buffer buffer;
    public Buffer VkBuffer => buffer;
    private DeviceMemory memory;


    public LveBuffer(
        Vk vk, LveDevice device, 
        ulong instanceSize, uint instanceCount, 
        BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags, 
        ulong minOffsetAlignment = 1
        )
    {
        this.vk = vk;
        this.device = device;
        this.instanceCount = instanceCount;
        this.instanceSize = instanceSize;
        this.usageFlags = usageFlags;
        this.memoryPropertyFlags = memoryPropertyFlags;


        alignmentSize = getAlignment(instanceSize, minOffsetAlignment);
        bufferSize = alignmentSize * instanceCount;
        device.CreateBuffer(bufferSize, usageFlags, memoryPropertyFlags, ref buffer, ref memory);
    }

    private ulong getAlignment(ulong instanceSize, ulong minOffsetAlignment)
    {
        if (minOffsetAlignment > 0)
        {
            return (instanceSize + minOffsetAlignment - 1) & ~(minOffsetAlignment - 1);
        }
        return instanceSize;
    }


    /**
     * Map a memory range of this buffer. If successful, mapped points to the specified buffer range.
     *
     * @param size (Optional) Size of the memory range to map. Pass VK_WHOLE_SIZE to map the complete
     * buffer range.
     * @param offset (Optional) Byte offset from beginning
     *
     * @return VkResult of the buffer mapping call
    */
    public Result Map(ulong size = Vk.WholeSize, ulong offset = 0)
    {
        Debug.Assert(buffer.Handle != 0 && memory.Handle != 0, "Called map on buffer before create");
        return vk.MapMemory(device.VkDevice, memory, offset, size, 0, ref mapped);
    }


    /**
     * Unmap a mapped memory range
     *
     * @note Does not return a result as vkUnmapMemory can't fail
    */
    public void UnMap()
    {
        if (mapped is not null)
        {
            vk.UnmapMemory(device.VkDevice, memory);
            mapped = null;
        }
    }



    /**
     * Copies the specified data to the mapped buffer. Default value writes whole buffer range
     *
     * @param data Pointer to the data to copy
     * @param size (Optional) Size of the data to copy. Pass VK_WHOLE_SIZE to flush the complete buffer
     * range.
     * @param offset (Optional) Byte offset from beginning of mapped region
     *
    */
    public void WriteToBuffer<T>(T[] data, ulong size = Vk.WholeSize, ulong offset = 0)
    {
        if (size == Vk.WholeSize)
        {
            var tmpSpan = new Span<T>(mapped, data.Length);
            data.AsSpan().CopyTo(tmpSpan);
        }
        else
        {
            //https://github.com/dotnet/runtime/discussions/73108
            //You can just do span1.CopyTo(span2.Slice(index)) or span1.CopyTo[span2[index..]]. No more overload is needed.
            //var tmpSpan = new Span<T>(mapped, (int)instanceCount);
            //data.AsSpan().CopyTo(tmpSpan[(int)offset..]);

            throw new NotImplementedException("don't have offset stuff working yet");
        }
    }
    /**
     * Copies "instanceSize" bytes of data to the mapped buffer at an offset of index * alignmentSize
     *
     * @param data Pointer to the data to copy
     * @param index Used in offset calculation
     *
    */
    //void LveBuffer::writeToIndex(void* data, int index)
    //{
    //    writeToBuffer(data, instanceSize, index * alignmentSize);
    //}
    //public unsafe void UpdateUbo(GlobalUbo ubo, int index)
    //{
    //    void* data;
    //    vk.MapMemory(device.VkDevice, memory, 0, (ulong)Unsafe.SizeOf<GlobalUbo>(), 0, &data);
    //    new Span<GlobalUbo>(data, 1)[0] = ubo;
    //    vk.UnmapMemory(device.VkDevice, memory);

    //    //data.CopyTo(new Span<T>(mapped, data.Length));
    //}
    public unsafe void WriteToIndex<T>(T[] data, int index)
    {
        //new Span<T>(data, (int)instanceCount)[index] = item;
        //var data = new T[] { item };
        //WriteToBuffer(data, instanceSize, (ulong)index * alignmentSize);
        var tmpSpan = new Span<T>(mapped, data.Length);
        data.AsSpan().CopyTo(tmpSpan[index..]);


        //void* data;
        //vk.MapMemory(device.VkDevice, memory, (ulong)index * instanceSize, (ulong)Unsafe.SizeOf<T>(), 0, &data);
        //new Span<T>(data, (int)instanceCount)[index] = item;
        //vk.UnmapMemory(device.VkDevice, memory);

        //data.CopyTo(new Span<T>(mapped, data.Length));
    }





    /**
     * Flush a memory range of the buffer to make it visible to the device
     *
     * @note Only required for non-coherent memory
     *
     * @param size (Optional) Size of the memory range to flush. Pass VK_WHOLE_SIZE to flush the
     * complete buffer range.
     * @param offset (Optional) Byte offset from beginning
     *
     * @return VkResult of the flush call
    */
    public Result Flush(ulong size = Vk.WholeSize, ulong offset = 0)
    {
        MappedMemoryRange mappedRange = new()
        {
            SType = StructureType.MappedMemoryRange,
            Memory = memory,
            Offset = offset,
            Size = size
        };
        return vk.FlushMappedMemoryRanges(device.VkDevice, 1, mappedRange);
    }






    /**
     * Invalidate a memory range of the buffer to make it visible to the host
     *
     * @note Only required for non-coherent memory
     *
     * @param size (Optional) Size of the memory range to invalidate. Pass VK_WHOLE_SIZE to invalidate
     * the complete buffer range.
     * @param offset (Optional) Byte offset from beginning
     *
     * @return VkResult of the invalidate call
    */
    public Result Invalidate(ulong size = Vk.WholeSize, ulong offset = 0)
    {
        MappedMemoryRange mappedRange = new()
        {
            SType = StructureType.MappedMemoryRange,
            Memory = memory,
            Offset = offset,
            Size = size
        };
        return vk.InvalidateMappedMemoryRanges(device.VkDevice, 1, mappedRange);
    }



    /**
     * Create a buffer info descriptor
     *
     * @param size (Optional) Size of the memory range of the descriptor
     * @param offset (Optional) Byte offset from beginning
     *
     * @return VkDescriptorBufferInfo of specified offset and range
    */
    public DescriptorBufferInfo DescriptorInfo(ulong size = Vk.WholeSize, ulong offset = 0)
    {
        return new()
        {
            Buffer = buffer,
            Offset = offset,
            Range = size
        };
    }



    /**
     *  Flush the memory range at index * alignmentSize of the buffer to make it visible to the device
     *
     * @param index Used in offset calculation
     *
     */
    public Result FlushIndex(int index) 
    { 
        return Flush(alignmentSize, (ulong)index * alignmentSize); 
    }

    /**
     * Create a buffer info descriptor
     *
     * @param index Specifies the region given by index * alignmentSize
     *
     * @return VkDescriptorBufferInfo for instance at index
     */
    public DescriptorBufferInfo DescriptorInfoForIndex(int index)
    {
        return DescriptorInfo(alignmentSize, (ulong)index * alignmentSize);
    }

    /**
     * Invalidate a memory range of the buffer to make it visible to the host
     *
     * @note Only required for non-coherent memory
     *
     * @param index Specifies the region to invalidate: index * alignmentSize
     *
     * @return VkResult of the invalidate call
     */
    public Result InvalidateIndex(int index)
    {
        return Invalidate(alignmentSize, (ulong)index * alignmentSize);
    }

    public unsafe void Dispose()
    {
        UnMap();
        vk.DestroyBuffer(device.VkDevice, buffer, null);
        vk.FreeMemory(device.VkDevice, memory, null);
        GC.SuppressFinalize(this);
    }
}

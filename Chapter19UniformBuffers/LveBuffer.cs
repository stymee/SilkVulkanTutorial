
namespace Chapter19UniformBuffers;

public unsafe class LveBuffer : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    private bool disposedValue;

    private ulong bufferSize;
    public ulong BufferSize => BufferSize;

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
        if (size == Vk.WholeSize)
        {
            return vk.MapMemory(device.VkDevice, memory, 0, bufferSize, 0, ref mapped);
        }
        else
        {
            return vk.MapMemory(device.VkDevice, memory, offset, size, 0, ref mapped);
        }
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
    //public void WriteToBuffer(void* data, ulong size = ulong.MaxValue, ulong offset = 0)
    //{
    //    if (mapped is null)
    //    {
    //        throw new InvalidOperationException("Cannot copy to unmapped buffer");
    //    }

    //    if (size == ulong.MaxValue)
    //    {
    //        Marshal.Copy(data, 0, mapped, (int)bufferSize);
    //    }
    //    else
    //    {
    //        IntPtr memOffset = IntPtr.Add(mapped, (int)offset);
    //        Marshal.Copy(data, 0, memOffset, (int)size);
    //    }
    //}
    public void WriteToBuffer<T>(T[] data, ulong size = Vk.WholeSize, ulong offset = 0)
    {
        if (size == Vk.WholeSize)
        {
            data.AsSpan().CopyTo(new Span<T>(mapped, data.Length));
        }
        else
        {
            throw new NotImplementedException("Can't handle offsets yet when writing to vertex buffers");
        }
    }

    //public void WriteVertexArrayToBuffer(Vertex[] vertices, ulong size = Vk.WholeSize, ulong offset = 0)
    //{
    //    if (size == Vk.WholeSize)
    //    {
    //        vertices.AsSpan().CopyTo(new Span<Vertex>(mapped, vertices.Length));
    //    }
    //    else
    //    {
    //        throw new NotImplementedException("Can't handle offsets yet when writing to vertex buffers");
    //    }
    //}
    //public void WriteIndexArrayToBuffer(uint[] indices, ulong size = Vk.WholeSize, ulong offset = 0)
    //{
    //    if (size == Vk.WholeSize)
    //    {
    //        indices.AsSpan().CopyTo(new Span<uint>(mapped, indices.Length));
    //    }
    //    else
    //    {
    //        throw new NotImplementedException("Can't handle offsets yet when writing to vertex buffers");
    //    }
    //}


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
     * Copies "instanceSize" bytes of data to the mapped buffer at an offset of index * alignmentSize
     *
     * @param data Pointer to the data to copy
     * @param index Used in offset calculation
     *
    */
    //public void WriteToIndex(void* data, int index)
    //{
    //    //writeToBuffer(data, instanceSize, index * alignmentSize);
    //}



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
            UnMap();
            vk.DestroyBuffer(device.VkDevice, buffer, null);
            vk.FreeMemory(device.VkDevice, memory, null);

            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~LveBuffer()
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

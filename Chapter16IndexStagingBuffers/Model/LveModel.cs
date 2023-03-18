
namespace Chapter16IndexStagingBuffers;


public class LveModel : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;

    private bool disposedValue;

    private Buffer vertexBuffer;
    private DeviceMemory vertexBufferMemory;
    private uint vertexCount;

    private bool hasIndexBuffer = false;
    private Buffer indexBuffer;
    private DeviceMemory indexBufferMemory;
    private uint indexCount;

    public LveModel(Vk vk, LveDevice device, Builder builder)
    {
        this.vk = vk;
        this.device = device;
        vertexCount = (uint)builder.Vertices.Length;
        createVertexBuffers(builder.Vertices);
        indexCount = (uint)builder.Indices.Length;
        if (indexCount > 0)
        {
            createIndexBuffers(builder.Indices);
        }
    }

    private unsafe void createVertexBuffers(Vertex[] vertices)
    {
        BufferCreateInfo bufferInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = (ulong)(sizeof(Vertex) * vertices.Length),
            Usage = BufferUsageFlags.VertexBufferBit,
            SharingMode = SharingMode.Exclusive,
        };

        fixed (Buffer* vertexBufferPtr = &vertexBuffer)
        {
            if (vk.CreateBuffer(device.VkDevice, bufferInfo, null, vertexBufferPtr) != Result.Success)
            {
                throw new Exception("failed to create vertex buffer!");
            }
        }

        MemoryRequirements memRequirements = new();
        vk.GetBufferMemoryRequirements(device.VkDevice, vertexBuffer, out memRequirements);

        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = device.FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit),
        };

        fixed (DeviceMemory* vertexBufferMemoryPtr = &vertexBufferMemory)
        {
            if (vk.AllocateMemory(device.VkDevice, allocateInfo, null, vertexBufferMemoryPtr) != Result.Success)
            {
                throw new Exception("failed to allocate vertex buffer memory!");
            }
        }

        vk.BindBufferMemory(device.VkDevice, vertexBuffer, vertexBufferMemory, 0);

        void* data;
        vk.MapMemory(device.VkDevice, vertexBufferMemory, 0, bufferInfo.Size, 0, &data);
        vertices.AsSpan().CopyTo(new Span<Vertex>(data, vertices.Length));
        vk.UnmapMemory(device.VkDevice, vertexBufferMemory);
    }


    private unsafe void createIndexBuffers(uint[] indices)
    {
        BufferCreateInfo bufferInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = (ulong)(sizeof(uint) * indices.Length),
            Usage = BufferUsageFlags.IndexBufferBit,
            SharingMode = SharingMode.Exclusive,
        };

        fixed (Buffer* indexBufferPtr = &indexBuffer)
        {
            if (vk.CreateBuffer(device.VkDevice, bufferInfo, null, indexBufferPtr) != Result.Success)
            {
                throw new Exception("failed to create index buffer!");
            }
        }

        MemoryRequirements memRequirements = new();
        vk.GetBufferMemoryRequirements(device.VkDevice, indexBuffer, out memRequirements);

        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = device.FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit),
        };

        fixed (DeviceMemory* indexBufferMemoryPtr = &indexBufferMemory)
        {
            if (vk.AllocateMemory(device.VkDevice, allocateInfo, null, indexBufferMemoryPtr) != Result.Success)
            {
                throw new Exception("failed to allocate index buffer memory!");
            }
        }

        vk.BindBufferMemory(device.VkDevice, indexBuffer, indexBufferMemory, 0);

        void* data;
        vk.MapMemory(device.VkDevice, indexBufferMemory, 0, bufferInfo.Size, 0, &data);
        indices.AsSpan().CopyTo(new Span<uint>(data, indices.Length));
        vk.UnmapMemory(device.VkDevice, indexBufferMemory);
    }

    public unsafe void Bind(CommandBuffer commandBuffer)
    {
        var vertexBuffers = new Buffer[] { vertexBuffer };
        var offsets = new ulong[] { 0 };

        fixed (ulong* offsetsPtr = offsets)
        fixed (Buffer* vertexBuffersPtr = vertexBuffers)
        {
            vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertexBuffersPtr, offsetsPtr);
        }

        if (hasIndexBuffer)
        {
            vk.CmdBindIndexBuffer(commandBuffer, indexBuffer, 0, IndexType.Uint32);
        }
    }

    public void Draw(CommandBuffer commandBuffer)
    {
        if (hasIndexBuffer)
        {
            vk.CmdDrawIndexed(commandBuffer, indexCount, 1, 0, 0, 0);
        }
        else
        {
            vk.CmdDraw(commandBuffer, vertexCount, 1, 0, 0);
        }
    }

    protected unsafe virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            vk.DestroyBuffer(device.VkDevice, vertexBuffer, null);
            vk.FreeMemory(device.VkDevice, vertexBufferMemory, null);

            if (hasIndexBuffer)
            {
                vk.DestroyBuffer(device.VkDevice, indexBuffer, null);
                vk.FreeMemory(device.VkDevice, indexBufferMemory, null);
            }
            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~LveModel()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

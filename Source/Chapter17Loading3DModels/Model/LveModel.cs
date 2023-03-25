
namespace Chapter17Loading3DModels;


public class LveModel : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;

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
            hasIndexBuffer = true;
            createIndexBuffers(builder.Indices);
        }
    }

    private unsafe void createVertexBuffers(Vertex[] vertices)
    {
        ulong bufferSize = (ulong)(Unsafe.SizeOf<Vertex>() * vertices.Length);

        Buffer stagingBuffer = default;
        DeviceMemory stagingBufferMemory = default;
        createBuffer(bufferSize, 
            BufferUsageFlags.TransferSrcBit, 
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, 
            ref stagingBuffer, ref stagingBufferMemory);

        void* data;
        vk!.MapMemory(device.VkDevice, stagingBufferMemory, 0, bufferSize, 0, &data);
        vertices.AsSpan().CopyTo(new Span<Vertex>(data, vertices.Length));
        vk!.UnmapMemory(device.VkDevice, stagingBufferMemory);

        createBuffer(bufferSize, 
            BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferDstBit, 
            MemoryPropertyFlags.DeviceLocalBit, 
            ref vertexBuffer, ref vertexBufferMemory);

        copyBuffer(stagingBuffer, vertexBuffer, bufferSize);

        vk!.DestroyBuffer(device.VkDevice, stagingBuffer, null);
        vk!.FreeMemory(device.VkDevice, stagingBufferMemory, null);
    }

    private unsafe void createIndexBuffers(uint[] indices)
    {
        ulong bufferSize = (ulong)(Unsafe.SizeOf<uint>() * indices.Length);

        Buffer stagingBuffer = default;
        DeviceMemory stagingBufferMemory = default;
        createBuffer(bufferSize, 
            BufferUsageFlags.TransferSrcBit, 
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, 
            ref stagingBuffer, ref stagingBufferMemory);

        void* data;
        vk!.MapMemory(device.VkDevice, stagingBufferMemory, 0, bufferSize, 0, &data);
        indices.AsSpan().CopyTo(new Span<uint>(data, indices.Length));
        vk!.UnmapMemory(device.VkDevice, stagingBufferMemory);

        createBuffer(bufferSize, 
            BufferUsageFlags.IndexBufferBit | BufferUsageFlags.TransferDstBit, 
            MemoryPropertyFlags.DeviceLocalBit, 
            ref indexBuffer, ref indexBufferMemory);

        copyBuffer(stagingBuffer, indexBuffer, bufferSize);

        vk!.DestroyBuffer(device.VkDevice, stagingBuffer, null);
        vk!.FreeMemory(device.VkDevice, stagingBufferMemory, null);
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



    // buffer helpers

    private unsafe void createBuffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags properties, ref Buffer buffer, ref DeviceMemory bufferMemory)
    {
        BufferCreateInfo bufferInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive,
        };

        fixed (Buffer* bufferPtr = &buffer)
        {
            if (vk!.CreateBuffer(device.VkDevice, bufferInfo, null, bufferPtr) != Result.Success)
            {
                throw new Exception("failed to create vertex buffer!");
            }
        }

        MemoryRequirements memRequirements = new();
        vk!.GetBufferMemoryRequirements(device.VkDevice, buffer, out memRequirements);

        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = device.FindMemoryType(memRequirements.MemoryTypeBits, properties),
        };

        fixed (DeviceMemory* bufferMemoryPtr = &bufferMemory)
        {
            if (vk!.AllocateMemory(device.VkDevice, allocateInfo, null, bufferMemoryPtr) != Result.Success)
            {
                throw new Exception("failed to allocate vertex buffer memory!");
            }
        }

        vk!.BindBufferMemory(device.VkDevice, buffer, bufferMemory, 0);
    }

    private unsafe void copyBuffer(Buffer srcBuffer, Buffer dstBuffer, ulong size)
    {
        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = device.GetCommandPool(),
            CommandBufferCount = 1,
        };

        CommandBuffer commandBuffer = default;
        vk!.AllocateCommandBuffers(device.VkDevice, allocateInfo, out commandBuffer);

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };

        vk!.BeginCommandBuffer(commandBuffer, beginInfo);

        BufferCopy copyRegion = new()
        {
            Size = size,
        };

        vk!.CmdCopyBuffer(commandBuffer, srcBuffer, dstBuffer, 1, copyRegion);

        vk!.EndCommandBuffer(commandBuffer);

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer,
        };

        vk!.QueueSubmit(device.GraphicsQueue, 1, submitInfo, default);
        vk!.QueueWaitIdle(device.GraphicsQueue);

        vk!.FreeCommandBuffers(device.VkDevice, device.GetCommandPool(), 1, commandBuffer);
    }


    public unsafe void Dispose()
    {
        vk.DestroyBuffer(device.VkDevice, vertexBuffer, null);
        vk.FreeMemory(device.VkDevice, vertexBufferMemory, null);

        if (hasIndexBuffer)
        {
            vk.DestroyBuffer(device.VkDevice, indexBuffer, null);
            vk.FreeMemory(device.VkDevice, indexBufferMemory, null);
        }
        GC.SuppressFinalize(this);
    }
}

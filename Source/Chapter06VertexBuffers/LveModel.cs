﻿
using Silk.NET.Vulkan;

namespace Chapter06VertexBuffers;

public struct Vertex
{
    public Vector2 position;

    public Vertex(float x, float y)
    {
        position.X = x;
        position.Y = y;
    }

    //public static VertexInputBindingDescription GetBindingDescription()
    //{
    //    VertexInputBindingDescription bindingDescription = new()
    //    {
    //        Binding = 0,
    //        Stride = (uint)Unsafe.SizeOf<Vertex>(),
    //        InputRate = VertexInputRate.Vertex,
    //    };

    //    return bindingDescription;
    //}

    public static VertexInputBindingDescription[] GetBindingDescriptions()
    {
        var bindingDescriptions = new[]
        {
            new VertexInputBindingDescription()
            {
                Binding = 0,
                Stride = (uint)Unsafe.SizeOf<Vertex>(),
                InputRate = VertexInputRate.Vertex,
            }
        };

        return bindingDescriptions;
    }

    public static VertexInputAttributeDescription[] GetAttributeDescriptions()
    {
        var attributeDescriptions = new[]
        {
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 0,
                Format = Format.R32G32Sfloat,
                Offset = 0, //(uint)Marshal.OffsetOf<Vertex>("position"),
            }
        };

        return attributeDescriptions;

    }
}

public class LveModel : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;

    private bool disposedValue;

    private Buffer vertexBuffer;
    private DeviceMemory vertexBufferMemory;
    private uint vertexCount;

    public LveModel(Vk vk, LveDevice device, Vertex[] vertices)
    {
        this.vk = vk;
        this.device = device;
        vertexCount = (uint)vertices.Length;
        createVertexBuffers(vertices);
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

    private unsafe void createVertexBuffersBad(Vertex[] vertices)
    {
        ulong bufferSize = (ulong)(Unsafe.SizeOf<Vertex>() * vertices.Length);

        Buffer stagingBuffer = default;
        DeviceMemory stagingBufferMemory = default;
        device.CreateBuffer(
            bufferSize,
            BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            ref stagingBuffer,
            ref stagingBufferMemory
            );

        void* data;
        vk.MapMemory(device.VkDevice, stagingBufferMemory, 0, bufferSize, 0, &data);
        vertices.AsSpan().CopyTo(new Span<Vertex>(data, vertices.Length));
        vk.UnmapMemory(device.VkDevice, stagingBufferMemory);

        device.CreateBuffer(
            bufferSize,
            BufferUsageFlags.TransferDstBit | BufferUsageFlags.VertexBufferBit,
            MemoryPropertyFlags.DeviceLocalBit,
            ref vertexBuffer,
            ref vertexBufferMemory
            );

        device.CopyBuffer(stagingBuffer, vertexBuffer, bufferSize);

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
    }

    public void Draw(CommandBuffer commandBuffer)
    {
        vk.CmdDraw(commandBuffer, vertexCount, 1, 0, 0);
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

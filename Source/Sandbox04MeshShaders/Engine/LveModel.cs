using JeremyAnsel.Media.WavefrontObj;

namespace Sandbox04MeshShaders;

public class LveModel : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;

    private LveBuffer vertexBuffer = null!;
    private uint vertexCount;

    private bool hasIndexBuffer = false;
    private LveBuffer indexBuffer = null!;
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
        var instanceSize = (ulong)Vertex.SizeOf();
        ulong bufferSize = instanceSize * (ulong)vertices.Length;

        LveBuffer stagingBuffer = new(vk, device,
            instanceSize, vertexCount,
            BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
            );
        stagingBuffer.Map();
        stagingBuffer.WriteToBuffer(vertices);

        vertexBuffer = new(vk, device,
            instanceSize, vertexCount,
            BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferDstBit,
            MemoryPropertyFlags.DeviceLocalBit
            );

        device.CopyBuffer(stagingBuffer.VkBuffer, vertexBuffer.VkBuffer, bufferSize);
    }

    private unsafe void createIndexBuffers(uint[] indices)
    {
        var instanceSize = (ulong)(Unsafe.SizeOf<uint>());
        ulong bufferSize = instanceSize * (ulong)indices.Length;

        LveBuffer stagingBuffer = new(vk, device,
            instanceSize, indexCount,
            BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
            );
        stagingBuffer.Map();
        stagingBuffer.WriteToBuffer(indices);

        indexBuffer = new(vk, device,
            instanceSize, indexCount,
            BufferUsageFlags.IndexBufferBit | BufferUsageFlags.TransferDstBit,
            MemoryPropertyFlags.DeviceLocalBit
            );

        device.CopyBuffer(stagingBuffer.VkBuffer, indexBuffer.VkBuffer, bufferSize);
    }

    public unsafe void Bind(CommandBuffer commandBuffer)
    {
        var vertexBuffers = new Buffer[] { vertexBuffer.VkBuffer };
        var offsets = new ulong[] { 0 };

        fixed (ulong* offsetsPtr = offsets)
        fixed (Buffer* vertexBuffersPtr = vertexBuffers)
        {
            vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertexBuffersPtr, offsetsPtr);
        }

        if (hasIndexBuffer)
        {
            vk.CmdBindIndexBuffer(commandBuffer, indexBuffer.VkBuffer, 0, IndexType.Uint32);
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
    public unsafe void Dispose()
    {
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
        GC.SuppressFinalize(this);
    }



    public struct Builder
    {
        public Vertex[] Vertices;
        public uint[] Indices;

        public Builder()
        {
            Vertices = Array.Empty<Vertex>();
            Indices = Array.Empty<uint>();
        }

        public unsafe void LoadModel(string path)
        {
            log.d("obj", $" loading {path}...");

            var objFile = ObjFile.FromFile(path);

            var vertexMap = new Dictionary<Vertex, uint>();
            var vertices = new List<Vertex>();
            var indices = new List<uint>();

            foreach (var face in objFile.Faces)
            {
                foreach (var vFace in face.Vertices)
                {
                    var vertexIndex = vFace.Vertex;
                    var vertex = objFile.Vertices[vertexIndex - 1];
                    var positionOut = new Vector3(vertex.Position.X, -vertex.Position.Y, vertex.Position.Z);
                    var colorOut = Vector3.Zero;
                    if (vertex.Color is not null)
                    {
                        colorOut = new(vertex.Color.Value.X, vertex.Color.Value.Y, vertex.Color.Value.Z);
                    }
                    else
                    {
                        colorOut = new(1f, 1f, 1f);
                    }

                    var normalIndex = vFace.Normal;
                    var normal = objFile.VertexNormals[normalIndex - 1];
                    var normalOut = new Vector3(normal.X, -normal.Y, normal.Z);

                    var textureIndex = vFace.Texture;
                    var texture = objFile.TextureVertices[textureIndex - 1];
                    //Flip Y for OBJ in Vulkan
                    var textureOut = new Vector2(texture.X, -texture.Y);

                    Vertex vertexOut = new()
                    {
                        Position = positionOut,
                        Color = colorOut,
                        Normal = normalOut,
                        UV = textureOut
                    };
                    if (vertexMap.TryGetValue(vertexOut, out var meshIndex))
                    {
                        indices.Add(meshIndex);
                    }
                    else
                    {
                        indices.Add((uint)vertices.Count);
                        vertexMap[vertexOut] = (uint)vertices.Count;
                        vertices.Add(vertexOut);
                    }
                }

            }

            Vertices = vertices.ToArray();
            Indices = indices.ToArray();

            log.d("obj", $" done {Vertices.Length} verts, {Indices.Length} indices");

        }

    }

}
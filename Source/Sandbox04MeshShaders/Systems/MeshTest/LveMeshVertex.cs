
namespace Sandbox04MeshShaders;

public struct LveMeshVertex
{
    public Vector4 Start;   //16  w is used for thickness (eventually)
    public Vector4 Stop;    //16  w is used for thickness (eventually)
    public Vector4 Color;   //16  w will be used for transparency (eventually)

    public LveMeshVertex(Vector4 start, Vector4 stop, Vector4 color)
    {
        Start = start;
        Stop = stop;
        Color = color;
    }



    public static uint SizeOf() => (uint)Unsafe.SizeOf<LveMeshVertex>();


    public static VertexInputBindingDescription[] GetBindingDescriptions()
    {
        var bindingDescriptions = new[]
        {
            new VertexInputBindingDescription()
            {
                Binding = 0,
                Stride = (uint)Unsafe.SizeOf<LveMeshVertex>(),
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
                Format = Format.R32G32B32A32Sfloat,
                Offset = (uint)Marshal.OffsetOf<LveMeshVertex>(nameof(Start)),
            },
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 1,
                Format = Format.R32G32B32A32Sfloat,
                Offset = (uint)Marshal.OffsetOf<LveMeshVertex>(nameof(Stop)),
            },
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 2,
                Format = Format.R32G32B32A32Sfloat,
                Offset = (uint)Marshal.OffsetOf<LveMeshVertex>(nameof(Color)),
            }
        };

        return attributeDescriptions;

    }
}
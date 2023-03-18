
namespace Chapter17Loading3DModels;

public struct Vertex
{
    public Vector3 Position;
    public Vector3 Color;
    public Vector3 Normal;
    public Vector2 UV;

    public Vertex(Vector3 pos, Vector3 color)
    {
        Position = pos;
        Color = color;
    }



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
                Format = Format.R32G32B32Sfloat,
                Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Position)),
            },
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 1,
                Format = Format.R32G32B32Sfloat,
                Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Color)),
            }
        };

        return attributeDescriptions;

    }
}
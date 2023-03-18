
namespace Chapter16IndexStagingBuffers;

public struct Builder
{
    public Vertex[] Vertices;
    public uint[] Indices;

    public Builder()
    {
        Vertices = Array.Empty<Vertex>();
        Indices = Array.Empty<uint>();
    }
}

using JeremyAnsel.Media.WavefrontObj;

namespace Chapter20zDescriptorSetsAlt;

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
                var normalOut = new Vector3(normal.X, normal.Y, normal.Z);

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

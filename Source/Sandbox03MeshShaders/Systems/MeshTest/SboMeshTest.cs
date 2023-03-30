
namespace Sandbox03MeshShaders;
public struct SboMeshTest
{
    public uint TriangleCount;
    public float TriangleSpacing;
    public float TriangleWidth;
    public float TriangleHeight;

    public SboMeshTest()
    {
        TriangleCount = 3;
        TriangleSpacing = 1f;
        TriangleWidth = 1f;
        TriangleHeight = 1f;
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<LineSegMeshPushConstantData>();
}

namespace Chapter20DescriptorSets;

public struct GlobalUbo
{
    public Matrix4x4 ProjectionView;
    public Vector4 LightDirection;

    public GlobalUbo()
    {
        ProjectionView = Matrix4x4.Identity;
        LightDirection = new(1f, -3f, 1f, 0f);
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<GlobalUbo>();

}

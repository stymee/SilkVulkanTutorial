namespace Chapter22FragmentLighting;

public struct GlobalUbo
{
    public Matrix4x4 ProjectionView;
    public Vector4 AmbientColor;
    public Vector4 LightPosition;
    public Vector4 LightColor;

    public GlobalUbo()
    {
        ProjectionView = Matrix4x4.Identity;
        AmbientColor = new(1f, 1f, 1f, 0.02f);
        LightPosition = new(-1f);
        LightColor = new(1f);
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<GlobalUbo>();

}

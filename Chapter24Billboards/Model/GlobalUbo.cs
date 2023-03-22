namespace Chapter24Billboards;

public struct GlobalUbo
{
    public Matrix4x4 Projection;
    public Matrix4x4 View;
    public Vector4 AmbientColor;
    public Vector4 LightPosition;
    public Vector4 LightColor;

    public GlobalUbo()
    {
        Projection = Matrix4x4.Identity;
        View = Matrix4x4.Identity;
        AmbientColor = new(1f, 1f, 1f, 0.02f);
        LightPosition = new(1f, 2f, 1f, 0f);
        LightColor = new(1f);
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<GlobalUbo>();

}

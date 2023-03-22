namespace Chapter25MultiplePointLights;

//[StructLayout(LayoutKind.Sequential)]
//public struct Struct1
//{
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = sizeOfarray)]
//    private Struct2[] arrayStruct;
//}

//[StructLayout(LayoutKind.Sequential)]
public struct GlobalUbo
{
    public Matrix4x4 Projection;
    public Matrix4x4 View;
    public Vector4 AmbientColor;
    public PointLight PointLight1;
    public PointLight PointLight2;
    public PointLight PointLight3;
    public PointLight PointLight4;
    public PointLight PointLight5;
    public PointLight PointLight6;
    public int NumLights;
    
    public GlobalUbo()
    {
        Projection = Matrix4x4.Identity;
        View = Matrix4x4.Identity;
        AmbientColor = new(1f, 1f, 1f, 0.02f);
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<GlobalUbo>();

}


public struct PointLight
{
    public Vector4 Position;
    public Vector4 Color;
}
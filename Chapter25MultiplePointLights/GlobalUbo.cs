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
    //[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 320)]
    //public Vector4[] PointLightPositions;
    //public Vector4[] PointLightColors;
    public PointLight PointLight1;
    public int NumLights;
    
    public GlobalUbo()
    {
        Projection = Matrix4x4.Identity;
        View = Matrix4x4.Identity;
        AmbientColor = new(1f, 1f, 1f, 0.02f);
        //PointLightPositions = new Vector4[10];
        //PointLightPositions[0] = new(1f, 2f, 1f, 0f);
        //PointLightPositions[1] = new(6f, 4f, 1f, 0f);
        //PointLightColors = new Vector4[10];
        //PointLightColors[0] = new(1f, 1f, 0.5f, .9f);
        //PointLightColors[1] = new(0.5f, 1f, 1f, .9f);
        //NumLights = 1;
        //PointLight1 = new PointLight()
        //{
        //    Position = new(1f, 2f, 1f, 0f),
        //    Color = new(1f, 1f, 0.5f, .9f)
        //};
        //PointLight2 = new PointLight()
        //{
        //    Position = new(6f, 4f, 1f, 0f),
        //    Color = new(0.5f, 1f, 1f, .9f)
        //};
    }

    public static uint SizeOf() => (uint)Unsafe.SizeOf<GlobalUbo>();

}


public struct PointLight
{
    public Vector4 Position;
    public Vector4 Color;
}
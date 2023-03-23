namespace Chapter27AlphaBlending;

public class GlobalUbo2
{
    private Matrix4x4 projection = Matrix4x4.Identity;      // 64
    private Matrix4x4 view = Matrix4x4.Identity;            // 64
    private Vector4 frontVec = Vector4.UnitZ;               // 16
    private Vector4 ambientColor = new(1f, 1f, 1f, 0.02f);  // 16
    private int numLights = 0;                              // 4
    private int padding1 = 0;                               // 4
    private int padding2 = 0;                               // 4
    private int padding3 = 0;                               // 4
    private PointLight2[] pointLights = new PointLight2[]   // 10 * 32 * 320
    {
        new(), new(), new(), new(), new(), new(), new(), new(), new(), new(),
    };

    // total size is 496

    public GlobalUbo2()
    {
        
    }
    //public GlobalUbo2() 
    //{
    //    projection = Matrix4x4.Identity;
    //    view = Matrix4x4.Identity;
    //    frontVec = Vector4.UnitZ;
    //    ambientColor = Vector4.One;
    //    numLights = 0;
    //    padding1 = 0;
    //    padding2 = 0;
    //    padding3 = 0;
    //    pointLights = new PointLight2[10];
    //    Array.Fill(pointLights, new());
    //}

    public void SetNumLights(int numLights)
    {
        this.numLights = numLights;
    }

    public void SetPointLightTranslation(int lightIndex, Vector3 translation)
    {
        pointLights[lightIndex].SetPosition(translation);
    }
    
    public void SetPointLightColor(int lightIndex, Vector4 color, float intensity)
    {
        pointLights[lightIndex].SetColor(color, intensity);
    }
    
    public byte[] AsBytes()
    {
        uint offset = 0;
        uint fsize = sizeof(float);
        uint vsize = fsize * 4;
        uint msize = vsize * 4;
        var bytes = new byte[SizeOf()];

        projection.AsBytes().CopyTo(bytes, offset);
        offset += msize;
        view.AsBytes().CopyTo(bytes, offset);
        offset += msize;

        frontVec.AsBytes().CopyTo(bytes, offset);
        offset += vsize;
        ambientColor.AsBytes().CopyTo(bytes, offset);
        offset += vsize;

        BitConverter.GetBytes(numLights).CopyTo(bytes, offset);
        offset += fsize;
        BitConverter.GetBytes(padding1).CopyTo(bytes, offset);
        offset += fsize;
        BitConverter.GetBytes(padding2).CopyTo(bytes, offset);
        offset += fsize;
        BitConverter.GetBytes(padding3).CopyTo(bytes, offset);
        offset += fsize;

        var pbytes = pointLights.AsBytes();
        pbytes.CopyTo(bytes, offset);

        return bytes;
    }

    public GlobalUbo2(byte[] bytes)
    {
        int offset = 0;
        for (int row=0; row < 4; row++)
        {
            for (int col=0; col < 4; col++)
            {
                projection[row, col] = BitConverter.ToSingle(bytes[offset..(offset + 4)]);
                offset += 4;
            }
        }
        for (int row=0; row < 4; row++)
        {
            for (int col=0; col < 4; col++)
            {
                view[row, col] = BitConverter.ToSingle(bytes[offset..(offset + 4)]);
                offset += 4;
            }
        }
        for (int col=0; col < 4; col++)
        {
            frontVec[col] = BitConverter.ToSingle(bytes[offset..(offset + 4)]);
            offset += 4;
        }
        for (int col=0; col < 4; col++)
        {
            ambientColor[col] = BitConverter.ToSingle(bytes[offset..(offset + 4)]);
            offset += 4;
        }
        numLights = BitConverter.ToInt32(bytes[offset..(offset + 4)]);
        offset += 4;
        padding1 = BitConverter.ToInt32(bytes[offset..(offset + 4)]);
        offset += 4;
        padding2 = BitConverter.ToInt32(bytes[offset..(offset + 4)]);
        offset += 4;
        padding3 = BitConverter.ToInt32(bytes[offset..(offset + 4)]);
        offset += 4;

        for (int pt=0; pt<10; pt++)
        {
            var ptpos = Vector4.Zero;
            for (int col = 0; col < 4; col++)
            {
                ptpos[col] = BitConverter.ToSingle(bytes[offset..(offset + 4)]);
                offset += 4;
            }
            var ptcol = Vector4.One;
            for (int col = 0; col < 4; col++)
            {
                ptcol[col] = BitConverter.ToSingle(bytes[offset..(offset + 4)]);
                offset += 4;
            }
            pointLights[pt] = new PointLight2(ptpos, ptcol);
        }

    }

    public void Update(Matrix4x4 projection, Matrix4x4 view, Vector4 frontVec)
    {
        this.projection = projection;
        this.view = view;
        this.frontVec = frontVec;
    }

    public static uint SizeOf() => 496;// (uint)Unsafe.SizeOf<GlobalUbo2>();

}


public class PointLight2
{
    private Vector4 position = Vector4.Zero;
    private Vector4 color = Vector4.One;

    public PointLight2()
    {
        
    }
    public PointLight2(Vector4 position, Vector4 color)
    {
        this.position = position;
        this.color = color;
    }

    public void SetPosition(Vector3 pos)
    {
        position = new Vector4(pos.X, pos.Y, pos.Z, 0f);
    }
    public void SetColor(Vector4 col, float intensity)
    {
        color = new Vector4(col.X, col.Y, col.Z, intensity);
    }

    public byte[] AsBytes()
    {
        var bytes = new byte[32];
        position.AsBytes().CopyTo(bytes, 0);
        color.AsBytes().CopyTo(bytes, 16);

        return bytes;
    }

    public static uint SizeOf() => 32;// (uint)Unsafe.SizeOf<PointLight2>();

    public override string ToString()
    {
        return $"p:{position}, c:{color}";
    }
}



public static class TypeExtensions
{
    

    public static byte[] AsBytes(this Vector4 vec)
    {
        uint offset = 0;
        uint fsize = 4;
        var bytes = new byte[16];
        BitConverter.GetBytes(vec.X).CopyTo(bytes, offset);
        BitConverter.GetBytes(vec.Y).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(vec.Z).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(vec.W).CopyTo(bytes, offset += fsize);

        return bytes;
    }
    
    public static byte[] AsBytes(this Matrix4x4 mat)
    {
        uint offset = 0;
        uint fsize = 4;
        var bytes = new byte[64];
        BitConverter.GetBytes(mat.M11).CopyTo(bytes, offset);
        BitConverter.GetBytes(mat.M12).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M13).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M14).CopyTo(bytes, offset += fsize);

        BitConverter.GetBytes(mat.M21).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M22).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M23).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M24).CopyTo(bytes, offset += fsize);

        BitConverter.GetBytes(mat.M31).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M32).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M33).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M34).CopyTo(bytes, offset += fsize);

        BitConverter.GetBytes(mat.M41).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M42).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M43).CopyTo(bytes, offset += fsize);
        BitConverter.GetBytes(mat.M44).CopyTo(bytes, offset += fsize);

        return bytes;
    }


    public static byte[] AsBytes(this PointLight2[] pts)
    {
        uint offset = 0;
        //uint psize = 32;
        var bytes = new byte[320];
        for (uint i = 0; i < 10; i++)
        {
            pts[i].AsBytes().CopyTo(bytes, offset);
            offset += 32;
        }

        return bytes;
    }
}
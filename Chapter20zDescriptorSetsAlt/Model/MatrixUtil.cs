
namespace Chapter20zDescriptorSetsAlt;

public static class MatrixUtil
{

    public static string PrintRows(this Matrix4x4 m)
    {
        const int dt = 8;
        var ret = new StringBuilder();
        ret.AppendLine($"M11{m.M11,dt:+0.000;-0.000;0}    M12{m.M12,dt:+0.000;-0.000;0}    M13{m.M13,dt:+0.000;-0.000;0}    M14{m.M14,dt:+0.000;-0.000;0}");
        ret.AppendLine($"M21{m.M21,dt:+0.000;-0.000;0}    M22{m.M22,dt:+0.000;-0.000;0}    M23{m.M23,dt:+0.000;-0.000;0}    M24{m.M24,dt:+0.000;-0.000;0}");
        ret.AppendLine($"M31{m.M31,dt:+0.000;-0.000;0}    M32{m.M32,dt:+0.000;-0.000;0}    M33{m.M33,dt:+0.000;-0.000;0}    M34{m.M34,dt:+0.000;-0.000;0}");
        ret.AppendLine($"M41{m.M41,dt:+0.000;-0.000;0}    M42{m.M42,dt:+0.000;-0.000;0}    M43{m.M43,dt:+0.000;-0.000;0}    M44{m.M44,dt:+0.000;-0.000;0}");
        return ret.ToString();
    }
    
    public static string PrintCols(this Matrix4x4 m)
    {
        const int dt = 8;
        var ret = new StringBuilder();
        ret.AppendLine($"M11{m.M11,dt:+0.000;-0.000;0}    M21{m.M21,dt:+0.000;-0.000;0}    M31{m.M31,dt:+0.000;-0.000;0}    M41{m.M41,dt:+0.000;-0.000;0}");
        ret.AppendLine($"M11{m.M12,dt:+0.000;-0.000;0}    M22{m.M22,dt:+0.000;-0.000;0}    M32{m.M32,dt:+0.000;-0.000;0}    M42{m.M42,dt:+0.000;-0.000;0}");
        ret.AppendLine($"M11{m.M13,dt:+0.000;-0.000;0}    M23{m.M23,dt:+0.000;-0.000;0}    M33{m.M33,dt:+0.000;-0.000;0}    M43{m.M43,dt:+0.000;-0.000;0}");
        ret.AppendLine($"M11{m.M14,dt:+0.000;-0.000;0}    M24{m.M24,dt:+0.000;-0.000;0}    M34{m.M34,dt:+0.000;-0.000;0}    M44{m.M44,dt:+0.000;-0.000;0}");
        return ret.ToString();
    }
    

    public static string Print(this Silk.NET.Maths.Matrix4X4<float> m)
    {
        const int dt = 8;
        var ret = new StringBuilder();
        ret.AppendLine($"M11{m.M11,dt:+0.000;-0.000;0}    M12{m.M12,dt:+0.000;-0.000;0}    M13{m.M13,dt:+0.000;-0.000;0}    M14{m.M14,dt:+0.000;-0.000;0}");
        ret.AppendLine($"M21{m.M21,dt:+0.000;-0.000;0}    M22{m.M22,dt:+0.000;-0.000;0}    M23{m.M23,dt:+0.000;-0.000;0}    M24{m.M24,dt:+0.000;-0.000;0}");
        ret.AppendLine($"M31{m.M31,dt:+0.000;-0.000;0}    M32{m.M32,dt:+0.000;-0.000;0}    M33{m.M33,dt:+0.000;-0.000;0}    M34{m.M34,dt:+0.000;-0.000;0}");
        ret.AppendLine($"M41{m.M41,dt:+0.000;-0.000;0}    M42{m.M42,dt:+0.000;-0.000;0}    M43{m.M43,dt:+0.000;-0.000;0}    M44{m.M44,dt:+0.000;-0.000;0}");
        return ret.ToString();
    }

}

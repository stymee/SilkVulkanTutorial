namespace Chapter14CameraViewTransform;

public class LveCamera
{

    private Matrix4x4 projectionMatrix = Matrix4x4.Identity;
    public Matrix4x4 GetProjection() => projectionMatrix;

    public Matrix4x4 viewMatrix = Matrix4x4.Identity;
    public Matrix4x4 GetView() => viewMatrix;

    private Vector3 up = -Vector3.UnitY;
    private Vector3 front = Vector3.UnitZ;

    public LveCamera()
    {

    }

    public void SetViewDirection(Vector3 position, Vector3 direction, Vector3 up)
    {
        Vector3 w = Vector3.Normalize(direction);
        Vector3 u = Vector3.Normalize(Vector3.Cross(w, up));
        Vector3 v = Vector3.Cross(w, u);

        viewMatrix = Matrix4x4.Identity with
        {
            M11 = u.X,
            M21 = u.Y,
            M31 = u.Z,
            M12 = v.X,
            M22 = v.Y,
            M32 = v.Z,
            M13 = w.X,
            M23 = w.Y,
            M33 = w.Z,
            M41 = -Vector3.Dot(u, position),
            M42 = -Vector3.Dot(v, position),
            M43 = -Vector3.Dot(w, position),
        };
    }

    //public void SetViewDirection(Vector3 position, Vector3 direction, Vector3 up)
    //{
    //    Vector3 w = Vector3.Normalize(direction);
    //    Vector3 u = Vector3.Normalize(Vector3.Cross(w, up));
    //    Vector3 v = Vector3.Cross(w, u);

    //    viewMatrix = Matrix4x4.Identity with
    //    {
    //        M11 = u.X,
    //        M12 = u.Y,
    //        M13 = u.Z,
    //        M21 = v.X,
    //        M22 = v.Y,
    //        M23 = v.Z,
    //        M31 = w.X,
    //        M32 = w.Y,
    //        M33 = w.Z,
    //        M14 = -Vector3.Dot(u, position),
    //        M24 = -Vector3.Dot(v, position),
    //        M34 = -Vector3.Dot(w, position),
    //    };
    //}

    public void SetViewTarget(Vector3 position, Vector3 target, Vector3 up)
    {
        SetViewDirection(position, target - position, up);
    }

    public void SetViewYXZ(Vector3 position, Vector3 rotation)
    {
        float c3 = MathF.Cos(rotation.Z);
        float s3 = MathF.Sin(rotation.Z);
        float c2 = MathF.Cos(rotation.X);
        float s2 = MathF.Sin(rotation.X);
        float c1 = MathF.Cos(rotation.Y);
        float s1 = MathF.Sin(rotation.Y);
        Vector3 u = new((c1 * c3 + s1 * s2 * s3), (c2 * s3), (c1 * s2 * s3 - c3 * s1));
        Vector3 v = new((c3 * s1 * s2 - c1 * s3), (c2 * c3), (c1 * c3 * s2 + s1 * s3));
        Vector3 w = new((c2 * s1), (-s2), (c1 * c2));

        viewMatrix = Matrix4x4.Identity with
        {
            M11 = u.X,
            M21 = u.Y,
            M31 = u.Z,
            M12 = v.X,
            M22 = v.Y,
            M32 = v.Z,
            M13 = w.X,
            M23 = w.Y,
            M33 = w.Z,
            M41 = -Vector3.Dot(u, position),
            M42 = -Vector3.Dot(v, position),
            M43 = -Vector3.Dot(w, position),
        };
    }

    //public void SetViewYXZ(Vector3 position, Vector3 rotation)
    //{
    //    float c3 = (float)Math.Cos(rotation.Z);
    //    float s3 = (float)Math.Sin(rotation.Z);
    //    float c2 = (float)Math.Cos(rotation.X);
    //    float s2 = (float)Math.Sin(rotation.X);
    //    float c1 = (float)Math.Cos(rotation.Y);
    //    float s1 = (float)Math.Sin(rotation.Y);
    //    Vector3 u = new Vector3((c1 * c3 + s1 * s2 * s3), (c2 * s3), (c1 * s2 * s3 - c3 * s1));
    //    Vector3 v = new Vector3((c3 * s1 * s2 - c1 * s3), (c2 * c3), (c1 * c3 * s2 + s1 * s3));
    //    Vector3 w = new Vector3((c2 * s1), (-s2), (c1 * c2));

    //    viewMatrix = Matrix4x4.Identity with
    //    {
    //        M11 = u.X,
    //        M12 = u.Y,
    //        M13 = u.Z,
    //        M21 = v.X,
    //        M22 = v.Y,
    //        M23 = v.Z,
    //        M31 = w.X,
    //        M32 = w.Y,
    //        M33 = w.Z,
    //        M14 = -Vector3.Dot(u, position),
    //        M24 = -Vector3.Dot(v, position),
    //        M34 = -Vector3.Dot(w, position),
    //    };
    //}

    public void SetOrthographicProjection(float left, float right, float top, float bottom, float near, float far)
    {
        projectionMatrix = Matrix4x4.Identity with
        {
            M11 = 2.0f / (right - left),
            M22 = 2.0f / (bottom - top),
            M33 = 1.0f / (far - near),
            M41 = -(right + left) / (right - left),
            M42 = -(bottom + top) / (bottom - top),
            M43 = -near / (far - near),
        };
    }

    public void SetPerspectiveProjection(float fovy, float aspect, float near, float far)
    {
        Debug.Assert(MathF.Abs(aspect - float.Epsilon) > 0.0f);
        float tanHalfFovy = MathF.Tan(fovy / 2.0f);
        projectionMatrix = new()
        {
            M11 = 1.0f / (aspect * tanHalfFovy),
            M22 = 1.0f / tanHalfFovy,
            M33 = far / (far - near),
            M34 = 1.0f,
            M43 = -(far * near) / (far - near)
        };
    }



}

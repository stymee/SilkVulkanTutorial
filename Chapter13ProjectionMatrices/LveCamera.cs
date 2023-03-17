namespace Chapter13ProjectionMatrices;

public class LveCamera
{

    private Matrix4x4 projectionMatrix = Matrix4x4.Identity;
    public Matrix4x4 GetProjection() => projectionMatrix;

    public LveCamera()
    {
        
    }

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

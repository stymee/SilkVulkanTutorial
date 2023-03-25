namespace Chapter19zNoCoherentAomSizeFix;

public struct GlobalUbo
{
    public Matrix4x4 ProjectionView;
    public Vector3 LightDirection;

    public GlobalUbo()
    {
        ProjectionView = Matrix4x4.Identity;
        LightDirection = new(1f, -3f, 1f);
    }
}

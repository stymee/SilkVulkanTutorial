
namespace Sandbox01MultiSampling;

public class LveGameObject
{
    static uint currentId = 0;

    // Id prop
    private uint id = 0;
    public uint Id => id;

    // other props
    private LveModel? model = null;
    public LveModel? Model { get => model; set => model = value; }

    private PointLightComponent? pointLight = null;
    public PointLightComponent? PointLight { get => pointLight; set => pointLight = value; }

    private Vector4 color = new(1.0f);
    public Vector4 Color { get => color; set => color = value; }


    private TransformComponent transform;
    public TransformComponent Transform { get => transform; set => transform = value; }
    

    public class TransformComponent
    {
        public Vector3 Translation;
        public Vector3 Rotation;
        public Vector3 Scale;

        public TransformComponent()
        {
            Translation = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
        }        

        
        public Matrix4x4 Mat4Old()
        {
            var matTranslate = Matrix4x4.CreateTranslation(Translation);
            var matScale = Matrix4x4.CreateScale(Scale);
            var matRot = Matrix4x4.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            return matScale * matRot * matTranslate;
        }

        public Matrix4x4 Mat4()
        {
            float c3 = MathF.Cos(Rotation.Z);
            float s3 = MathF.Sin(Rotation.Z);
            float c2 = MathF.Cos(Rotation.X);
            float s2 = MathF.Sin(Rotation.X);
            float c1 = MathF.Cos(Rotation.Y);
            float s1 = MathF.Sin(Rotation.Y);

            return new(
                Scale.X * (c1 * c3 + s1 * s2 * s3),
                Scale.X * (c2 * s3),
                Scale.X * (c1 * s2 * s3 - c3 * s1),
                0.0f,
                Scale.Y * (c3 * s1 * s2 - c1 * s3),
                Scale.Y * (c2 * c3),
                Scale.Y * (c1 * c3 * s2 + s1 * s3),
                0.0f,
                Scale.Z * (c2 * s1),
                Scale.Z * (-s2),
                Scale.Z * (c1 * c2),
                0.0f,
                Translation.X, Translation.Y, Translation.Z, 1.0f
            );
        }

        public Matrix4x4 NormalMatrix()
        {
            float c3 = MathF.Cos(Rotation.Z);
            float s3 = MathF.Sin(Rotation.Z);
            float c2 = MathF.Cos(Rotation.X);
            float s2 = MathF.Sin(Rotation.X);
            float c1 = MathF.Cos(Rotation.Y);
            float s1 = MathF.Sin(Rotation.Y);

            var invScale = new Vector3(1f/Scale.X, 1f/Scale.Y, 1f/Scale.Z);

            return new(
                invScale.X * (c1 * c3 + s1 * s2 * s3),
                invScale.X * (c2 * s3),
                invScale.X * (c1 * s2 * s3 - c3 * s1),
                0.0f,
                invScale.Y * (c3 * s1 * s2 - c1 * s3),
                invScale.Y * (c2 * c3),
                invScale.Y * (c1 * c3 * s2 + s1 * s3),
                0.0f,
                invScale.Z * (c2 * s1),
                invScale.Z * (-s2),
                invScale.Z * (c1 * c2),
                0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );
        }
    }
    
    
    public static LveGameObject MakePointLight(float intensity, float radius, Vector4 color)
    {
        LveGameObject gameObj = CreateGameObject();
        gameObj.Color = color;
        gameObj.Transform.Scale.X = radius;
        gameObj.PointLight = new PointLightComponent(intensity);
        return gameObj;
    }

    public static LveGameObject CreateGameObject()
    {
        currentId++;
        return new LveGameObject(currentId);
    }

    private LveGameObject(uint id)
    {
        this.id = id;
        transform = new();
    }
}

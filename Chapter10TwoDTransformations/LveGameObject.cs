
namespace Chapter10TwoDTransformations;

public class LveGameObject
{
    static uint currentId = 0;

    // Id prop
    private uint id = 0;
    public uint Id => id;

    // other props
    private LveModel model = null!;
    public LveModel Model { get => model; set => model = value; }

    private Vector4 color = new(1.0f);
    public Vector4 Color { get => color; set => color = value; }


    private Transform2dComponent transform2d;
    public Transform2dComponent Transform2d { get => transform2d; set => transform2d = value; }
    

    public struct Transform2dComponent
    {
        public Vector2 Translation;
        public float Rotation;
        public Vector2 Scale;

        public Transform2dComponent()
        {
            Translation = Vector2.Zero;
            Rotation = 0f;
            Scale = Vector2.One;
        }        

        
        public Matrix2X2<float> Mat2()
        {
            //var ret = Matrix2X2<float>.Identity;
            float s = MathF.Sin(Rotation);
            float c = MathF.Cos(Rotation);
            Matrix2X2<float> rotMatrix = new(c, s, -s, c);

            Matrix2X2<float> scaleMatrix = new(Scale.X, 0.0f, 0.0f, Scale.Y);
            // Silk.Net maths must be backwards from tutorial
            return scaleMatrix * rotMatrix;
        }
    }
    

    public static LveGameObject CreateGameObject()
    {
        currentId++;
        return new LveGameObject(currentId);
    }

    private LveGameObject(uint id)
    {
        this.id = id;
    }
}

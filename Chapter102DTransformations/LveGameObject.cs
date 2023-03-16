
namespace Chapter102DTransformations;

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

        public Matrix2X2<float> Mat2() => Matrix2X2<float>.Identity;
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

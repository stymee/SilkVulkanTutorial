
namespace Sandbox03MeshShaders;

public class LveMeshObject : IDisposable
{

    static uint currentId = 0;

    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    private readonly ExtMeshShader extMeshShader = null!;

    public uint Id { get; init; }


    public Vector3 Translation { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }



    public LveMeshObject(Vk vk, LveDevice device)
    {
        this.vk = vk;
        this.device = device;
        Translation = Vector3.Zero;
        Rotation = Vector3.Zero;
        Scale = Vector3.One;

        Id = LveGameObject.GetNextID();
        vk.TryGetDeviceExtension(device.Instance, device.VkDevice, out extMeshShader);

    }



    public Matrix4x4 TransformationMatrix()
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

        var invScale = new Vector3(1f / Scale.X, 1f / Scale.Y, 1f / Scale.Z);

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

    public void Bind(CommandBuffer commandBuffer)
    {
    }

    public void Draw(CommandBuffer commandBuffer)
    {
        extMeshShader.CmdDrawMeshTask(commandBuffer, 1, 1, 1);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static uint GetNextId()
    {
        currentId++;
        return currentId;
    }
}


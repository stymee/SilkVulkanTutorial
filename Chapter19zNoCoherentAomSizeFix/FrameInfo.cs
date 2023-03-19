
namespace Chapter19zNoCoherentAomSizeFix;

public class FrameInfo
{
    public int FrameIndex { get; set; }
    public float FrameTime { get; set; }
    public CommandBuffer CommandBuffer { get; init; }

    public OrthographicCamera Camera { get; init; } = null!;


}



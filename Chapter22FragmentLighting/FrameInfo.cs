﻿
namespace Chapter22FragmentLighting;

public struct FrameInfo
{
    public int FrameIndex;// { get; set; }
    public float FrameTime;// { get; set; }
    public CommandBuffer CommandBuffer;// { get; init; }
    public OrthographicCamera Camera;// { get; init; } = null!;
    public DescriptorSet GlobalDescriptorSet;// { get; init; }
    public Dictionary<uint, LveGameObject> GameObjects;
}



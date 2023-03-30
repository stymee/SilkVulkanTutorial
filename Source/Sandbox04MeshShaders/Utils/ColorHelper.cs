namespace Sandbox04MeshShaders;

public static class Color4
{
    public static Vector4 White => new(1f, 1f, 1f, 1f);
    public static Vector4 Black => new(0f, 0f, 0f, 1f);

    // my colors
    public static Vector4 RoughGreen => new(0f, 0.6f, 0f, 1f);

    // main
    public static Vector4 Red => new(1f, 0f, 0f, 1f);
    public static Vector4 Green => new(0f, 1f, 0f, 1f);
    public static Vector4 Blue => new(0f, 0f, 1f, 1f);

    // mix1
    public static Vector4 Yellow => new(1f, 1f, 0f, 1f);
    public static Vector4 Magenta => new(1f, 0f, 1f, 1f);
    public static Vector4 Cyan => new(0f, 1f, 1f, 1f);
    public static Vector4 Gray => new(0.5f, 0.5f, 0.5f, 1f);

    // others
    public static Vector4 Pink => new(1f, 0.75f, 0.8f, 1f);
    public static Vector4 Orange => new(1f, 0.65f, 0f, 1f);
    public static Vector4 Purple => new(0.6f, 0.4f, 0.8f, 1f);
    public static Vector4 Brown => new(0.6f, 0.4f, 0.2f, 1f);
    public static Vector4 Beige => new(0.96f, 0.96f, 0.86f, 1f);
    public static Vector4 Olive => new(0.5f, 0.5f, 0f, 1f);
    public static Vector4 Maroon => new(0.5f, 0f, 0f, 1f);
    public static Vector4 Navy => new(0f, 0f, 0.5f, 1f);
    public static Vector4 Teal => new(0f, 0.5f, 0.5f, 1f);

    // others2
    public static Vector4 Lime => new(0.75f, 1f, 0f, 1f);
    public static Vector4 Turquoise => new(0.25f, 0.88f, 0.82f, 1f);
    public static Vector4 Lavender => new(0.9f, 0.9f, 0.98f, 1f);
    public static Vector4 Coral => new(1f, 0.5f, 0.31f, 1f);
    public static Vector4 Salmon => new(0.98f, 0.5f, 0.45f, 1f);
    public static Vector4 Peach => new(1f, 0.8f, 0.64f, 1f);
    public static Vector4 Mint => new(0.6f, 1f, 0.8f, 1f);
    public static Vector4 PowderBlue => new(0.69f, 0.88f, 0.9f, 1f);
    public static Vector4 LightGray => new(0.83f, 0.83f, 0.83f, 1f);

    public static Vector4 WhiteSmoke => new(0.96f, 0.96f, 0.96f, 1f);
}


public static class ColorHex
{
    public const uint White = 0xFFFFFFFF;
    public const uint Black = 0x000000FF;

    // my colors
    public const uint RoughGreen = 0x009900FF;

    // main
    public const uint Red = 0xFF0000FF;
    public const uint Green = 0x00FF00FF;
    public const uint Blue = 0x0000FFFF;

    // mix1
    public const uint Yellow = 0xFFFF00FF;
    public const uint Magenta = 0xFF00FFFF;
    public const uint Cyan = 0x00FFFFFF;
    public const uint Gray = 0x808080FF;

    // others
    public const uint Pink = 0xFFB5C5FF;
    public const uint Orange = 0xFFA500FF;
    public const uint Purple = 0x663399FF;
    public const uint Brown = 0xA52A2AFF;
    public const uint Beige = 0xF5F5DCFF;
    public const uint Olive = 0x808000FF;
    public const uint Maroon = 0x800000FF;
    public const uint Navy = 0x000080FF;
    public const uint Teal = 0x008080FF;

    // others2
    public const uint Lime = 0xBFFF00FF;
    public const uint Turquoise = 0x40E0D0FF;
    public const uint Lavender = 0xE6E6FAFF;
    public const uint Coral = 0xFF7F50FF;
    public const uint Salmon = 0xFA8072FF;
    public const uint Peach = 0xFFDAB9FF;
    public const uint Mint = 0x98FB98FF;
    public const uint PowderBlue = 0xB0E0E6FF;
    public const uint LightGray = 0xD3D3D3FF;

    public const uint WhiteSmoke = 0xF5F5F5FF;
}


public static class Color3
{
    public static Vector3 White => new(1f, 1f, 1f);
    public static Vector3 Black => new(0f, 0f, 0f);

    // my colors
    public static Vector3 RoughGreen => new(0f, 0.6f, 0f);

    // main
    public static Vector3 Red => new(1f, 0f, 0f);
    public static Vector3 Green => new(0f, 1f, 0f);
    public static Vector3 Blue => new(0f, 0f, 1f);
    
    // mix1
    public static Vector3 Yellow => new(1f, 1f, 0f);
    public static Vector3 Magenta => new(1f, 0f, 1f);
    public static Vector3 Cyan => new(0f, 1f, 1f);
    public static Vector3 Gray => new(0.5f, 0.5f, 0.5f);

    // others
    public static Vector3 Pink => new(1f, 0.75f, 0.8f);
    public static Vector3 Orange => new(1f, 0.65f, 0f);
    public static Vector3 Purple => new(0.6f, 0.4f, 0.8f);
    public static Vector3 Brown => new(0.6f, 0.4f, 0.2f);
    public static Vector3 Beige => new(0.96f, 0.96f, 0.86f);
    public static Vector3 Olive => new(0.5f, 0.5f, 0f);
    public static Vector3 Maroon => new(0.5f, 0f, 0f);
    public static Vector3 Navy => new(0f, 0f, 0.5f);
    public static Vector3 Teal => new(0f, 0.5f, 0.5f);

    // others2
    public static Vector3 Lime => new(0.75f, 1f, 0f);
    public static Vector3 Turquoise => new(0.25f, 0.88f, 0.82f);
    public static Vector3 Lavender => new(0.9f, 0.9f, 0.98f);
    public static Vector3 Coral => new(1f, 0.5f, 0.31f);
    public static Vector3 Salmon => new(0.98f, 0.5f, 0.45f);
    public static Vector3 Peach => new(1f, 0.8f, 0.64f);
    public static Vector3 Mint => new(0.6f, 1f, 0.8f);
    public static Vector3 PowderBlue => new(0.69f, 0.88f, 0.9f);
    public static Vector3 LightGray => new(0.83f, 0.83f, 0.83f);
    
    public static Vector3 WhiteSmoke => new(0.96f, 0.96f, 0.96f);
}



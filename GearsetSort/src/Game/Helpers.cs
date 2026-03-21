using Dalamud.Interface.Textures;

namespace GearsetSort;

public partial class Util
{
    private static string GetIconPath(uint iconId, bool hq = false)
        => $"ui/icon/{iconId / 1000 * 1000:000000}{(hq ? "/hq" : "")}/{iconId:000000}.tex";

    public static ISharedImmediateTexture? GetTextureFromIcon(uint icon, bool hq = false)
        => Texture.GetFromGame(GetIconPath(icon, hq));

    public static ISharedImmediateTexture? GetJobIcon(byte? jobId)
        => jobId is > 0 ? Texture.GetFromGameIcon(62100 + (uint)jobId) : null;

    public static Vector4 JobToColour(ushort jobId)
    {
        (int r, int g, int b) = jobId switch
        {
            42 => (252, 146, 225),  
            41 => (16, 130, 16),
            40 => (128, 160, 240),
            39 => (150, 90, 144),
            38 => (226, 176, 175),
            37 => (121, 109, 48),
            36 => (65, 100, 205),
            35 => (232, 123, 123),
            34 => (228, 109, 4),
            33 => (255, 231, 74),
            32 => (209, 38, 204),
            31 => (110, 225, 214),
            30 => (175, 25, 100),
            28 => (134, 87, 255),
            27 => (45, 155, 120),
            25 => (165, 121, 214),
            24 => (255, 240, 220),
            23 => (145, 186, 94),
            22 => (65, 100, 205),
            21 => (207, 38, 33),
            20 => (214, 156, 0),
            19 => (168, 210, 230),
            _ => (102, 204, 255)
        };
        return ImEx.RGB(r, g, b);
    }
}
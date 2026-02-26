using System.Linq;
using Dalamud.Interface.Textures;
using Lumina.Excel.Sheets;

namespace GearsetSort;

public partial class Core
{
    private static string GetIconPath(uint iconId, bool hq = false)
        => $"ui/icon/{iconId / 1000 * 1000:000000}{(hq ? "/hq" : "")}/{iconId:000000}.tex";

    private static ISharedImmediateTexture? GetTextureFromIcon(uint icon, bool hq = false)
        => Texture.GetFromGame(GetIconPath(icon, hq));
}
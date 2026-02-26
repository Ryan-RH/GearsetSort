
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace GearsetSort.UI;

public static class Util
{
    public static void SetTableBackgroundColour(Vector4 colour)
    {
        uint uintColour = ImGui.GetColorU32(colour);
        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, uintColour);
    }

    public static void CentreText(string text, Vector4 colour, bool centreY = false)
    {
        var childWindowSize = ImGui.GetContentRegionAvail();
        var cursorPos = ImGui.GetCursorPos();
        var textSize = ImGui.CalcTextSize(text);
        ImGui.SetCursorPosX(cursorPos.X + (childWindowSize.X - textSize.X) / 2);
        if (centreY)
            ImGui.SetCursorPosY((childWindowSize.Y - textSize.Y) / 2);
        ImGui.TextColored(colour, text);
    }

    public static bool ColourButton(string label, Vector2 size, Vector4 idle, bool disabled = false)
    {
        float Clamp(float value) => value<0f ? 0f : value;

        Vector4 DarkenButton(Vector4 colour, float sub)
            => new Vector4(Clamp(colour.X-sub), Clamp(colour.Y-sub), Clamp(colour.Z-sub), colour.W);

        var hover = DarkenButton(idle, 0.15f);
        var active = DarkenButton(idle, 0.3f);

        using var colourButton = ImRaii.PushColor(ImGuiCol.Button, idle)
            .Push(ImGuiCol.ButtonHovered, hover)
            .Push(ImGuiCol.ButtonActive, active);
        if (disabled)
        {
            using var _ = ImRaii.Disabled(); // don't need to scope cos end if scope returns
            ImGui.Button(label, Vec2(size));
            return false;
        }

        return ImGui.Button(label, Vec2(size));
    }

    public static void HoverToolTip(string message)
    {
        if (!ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) return;

        using var ToolTip = ImRaii.Tooltip();

        if (!ToolTip) return;

        ImGui.Text(message);
    }

    public static Vector2 Vec2(float x, float y)
        => new(x * ImGuiHelpers.GlobalScale, y * ImGuiHelpers.GlobalScale);
    
    public static Vector2 Vec2(Vector2 vec2)
        => new(vec2.X * ImGuiHelpers.GlobalScale, vec2.Y * ImGuiHelpers.GlobalScale);

    public static Vector4 RGB(int R, int G, int B)
        => new(R/255f, G/255f, B/255f, 1f);

    public static Vector4 JobToColour(ushort jobId)
        => jobId switch
        {
            42 => RGB(252, 146, 225),  
            41 => RGB(16, 130, 16),
            40 => RGB(128, 160, 240),
            39 => RGB(150, 90, 144),
            38 => RGB(226, 176, 175),
            37 => RGB(121, 109, 48),
            36 => RGB(65, 100, 205),
            35 => RGB(232, 123, 123),
            34 => RGB(228, 109, 4),
            33 => RGB(255, 231, 74),
            32 => RGB(209, 38, 204),
            31 => RGB(110, 225, 214),
            30 => RGB(175, 25, 100),
            28 => RGB(134, 87, 255),
            27 => RGB(45, 155, 120),
            25 => RGB(165, 121, 214),
            24 => RGB(255, 240, 220),
            23 => RGB(145, 186, 94),
            22 => RGB(65, 100, 205),
            21 => RGB(207, 38, 33),
            20 => RGB(214, 156, 0),
            19 => RGB(168, 210, 230),
            _ => RGB(102, 204, 255)
        };
}

public static class Colour
{
    public static readonly Vector4 Green = new(0.2f, 0.6f, 0.2f, 1f);
    public static readonly Vector4 Yellow = new(0.9f, 0.7f, 0.2f, 1f);
    public static readonly Vector4 Blue = new(0.2f, 0.6f, 0.8f, 1f);
    public static readonly Vector4 Red = new(0.5f, 0.1f, 0.1f, 1f);    
}
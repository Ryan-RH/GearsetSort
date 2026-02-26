
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

        var sizeScaled = new Vector2(size.X * ImGuiHelpers.GlobalScale, size.Y * ImGuiHelpers.GlobalScale);

        if (disabled)
        {
            using var _ = ImRaii.Disabled(); // don't need to scope cos end if scope returns
            ImGui.Button(label, sizeScaled);
            return false;
        }

        return ImGui.Button(label, sizeScaled);
    }

    public static void HoverToolTip(string message)
    {
        if (!ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) return;

        using var ToolTip = ImRaii.Tooltip();

        if (!ToolTip) return;

        ImGui.Text(message);
    }
}

public static class Colour
{
    public static readonly Vector4 Green   = new(0.2f, 0.6f, 0.2f, 1f);
    public static readonly Vector4 Yellow  = new(0.9f, 0.7f, 0.2f, 1f);
    public static readonly Vector4 Blue    = new(0.2f, 0.6f, 0.8f, 1f);
    public static readonly Vector4 Red     = new(0.5f, 0.1f, 0.1f, 1f);
}
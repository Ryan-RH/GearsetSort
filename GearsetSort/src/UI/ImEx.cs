using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace GearsetSort;

public static class ImEx
{
    public static void SetTableBackgroundColour(Vector4 colour)
    {
        uint uintColour = ImGui.GetColorU32(colour);
        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, uintColour);
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
            using var _ = ImRaii.Disabled(); 
            ImGui.Button(label, Vec2(size));
            return false;
        }

        return ImGui.Button(label, Vec2(size));
    }

    public static void HoverToolTip(Action content)
    {
        if (!ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) return;

        using var ToolTip = ImRaii.Tooltip();
        if (!ToolTip) return;

        content();
    }

    public static void HoverToolTip(string message)
        => HoverToolTip(() => Text(message));

    public static Vector2 Vec2(float x, float y)
        => new(x * ImGuiHelpers.GlobalScale, y * ImGuiHelpers.GlobalScale);
    
    public static Vector2 Vec2(Vector2 vec2)
        => new(vec2.X * ImGuiHelpers.GlobalScale, vec2.Y * ImGuiHelpers.GlobalScale);

    public static Vector4 RGB(int R, int G, int B)
        => new(R/255f, G/255f, B/255f, 1f);

    public static void Text<T>(T payload, Vector4? col = null, ImExFlags flags = ImExFlags.None)
    {
        var text = payload?.ToString() ?? string.Empty;

        using var _ = col.HasValue ? ImRaii.PushColor(ImGuiCol.Text, col.Value) : null;

        var contentRegionAvail = ImGui.GetContentRegionAvail();
        var cursorPos = ImGui.GetCursorPos();
        var textSize = ImGui.CalcTextSize(text);

        if (flags.Has(ImExFlags.SameLine))
            ImGui.SameLine();
        if (flags.Has(ImExFlags.RightAlign))
            ImGui.SetCursorPosX(cursorPos.X + contentRegionAvail.X - textSize.X);
        if (flags.Has(ImExFlags.CentreX))
            ImGui.SetCursorPosX(cursorPos.X + (contentRegionAvail.X - textSize.X) / 2);
        if (flags.Has(ImExFlags.CentreY))
            ImGui.SetCursorPosY(cursorPos.Y + (contentRegionAvail.Y - textSize.Y) / 2);

        ImGui.TextUnformatted(text);
    }

    private static bool Has(this ImExFlags flags, ImExFlags value)
       => (flags & value) != 0;

    public static bool InputText(string label, ref string buffer, int maxLength = 255, bool noPad = false, Vector2? size = null, ImGuiInputTextFlags Flags = ImGuiInputTextFlags.None)
    {
        using var padding = noPad ? ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(0, 0)) : null;
        return ImGui.InputTextEx(label, "", ref buffer, maxLength, Vec2(size ?? default), Flags);
    }

    public static void ItemMouseClicked(ImGuiMouseButton button, Delegate handler, bool doubleClick = false , params object[] args)
    {
        if (!ImGui.IsItemHovered()) return;
        if (!(doubleClick ? ImGui.IsMouseDoubleClicked(button) : ImGui.IsItemClicked(button)))
            return;

        handler.DynamicInvoke(args);
    }

    private static double HoldStart = -1;
    private static double Progress = 0;
    public static bool HoldLeftClick(double time, Vector2? pos = null, Vector4? colour = null)
    {
        if (!(ImGui.IsItemHovered() && ImGui.IsMouseDown(ImGuiMouseButton.Left)))
        {
            HoldStart = -1;
            Progress = 0;
            return false;
        }

        if (HoldStart < 0) HoldStart = ImGui.GetTime();

        var timeElapsed = ImGui.GetTime() - HoldStart;
        Progress = Math.Clamp(timeElapsed/time, 0, 1);

        if (timeElapsed > time)
        {
            HoldStart = -1;
            return true;
        }

        if (Progress > 0)
        {
            var width = ImGui.GetContentRegionAvail().X;
            var position = pos ?? ImGui.GetCursorPos();
            ImGui.SetCursorPos(Vec2(position.X-1, position.Y));

            using var _ = ImRaii.PushColor(ImGuiCol.PlotHistogram, colour ?? Colour.Red);
            ImGui.ProgressBar((float)Progress, Vec2(width, 16), string.Empty);
        }
        return false;
    }

    public static void Image(IDalamudTextureWrap? texture, Vector2 size)
    {
        if (texture == null) return;
        ImGui.Image(texture.Handle, Vec2(size));
    }

    public static void FontAwesomeIcon(string icon, Vector4? colour = null, ImExFlags flags = ImExFlags.None)
    {
        using var _ = ImRaii.PushFont(UiBuilder.IconFont);
        Text(icon, colour, flags);
    }
}

[Flags]
public enum ImExFlags
{
    None = 0,
    SameLine = 1,
    RightAlign = 2,
    CentreX = 4,
    CentreY = 8
}

public static class Colour
{
    public static readonly Vector4 Green = new(0.2f, 0.6f, 0.2f, 1f);
    public static readonly Vector4 Yellow = new(0.9f, 0.7f, 0.2f, 1f);
    public static readonly Vector4 Blue = new(0.2f, 0.6f, 0.8f, 1f);
    public static readonly Vector4 Red = new(0.75f, 0.1f, 0.1f, 1f);    
    public static readonly Vector4 TransparentGrey = new(0.2f, 0.2f, 0.2f, 0.8f);
    public static readonly Vector4 SoftBlue = new(0.605f, 0.755f, 0.746f, 1f);
}

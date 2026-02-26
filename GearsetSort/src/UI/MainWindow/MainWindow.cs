using System.Text;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace GearsetSort.UI;

public partial class MainWindow : Window
{
    public MainWindow() : base($"GearsetSort {P.GetType().Assembly.GetName().Version} ###GearsetSortMainWindow")
    {
        Flags = ImGuiWindowFlags.NoResize;

        SizeConstraints = new()
        {
            MinimumSize = new Vector2(565,505),
            MaximumSize = new Vector2(565,505)  
        };

        P.windowSystem.AddWindow(this);
        AllowPinning = false;
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    public Core.Gearset? selectedGearset = null;

    public override void OnClose()
    {
        selectedGearset = null;
    }

    public override void Draw()
    {
        ScrollableBlock();

        ImGui.SameLine();

        PreviewBlock();

        if (Util.ColourButton("Sort", new Vector2(190, 30), Colour.Green))
        {
            Core.ApplyChange();
        }

        ImGui.SameLine();

        var cursorPosMode = ImGui.GetCursorPos();
        ImGui.SetCursorPosY(cursorPosMode.Y + ImGuiHelpers.GlobalScale * 4);
        ImGui.SetCursorPosX(cursorPosMode.X + ImGuiHelpers.GlobalScale * 110);
        ImGui.Text("Mode:");
        ImGui.SameLine();

        ImGui.SetCursorPosY(cursorPosMode.Y + ImGuiHelpers.GlobalScale * 3);

        if (Util.ColourButton(P.config.Insert ? "Insert" : "Swap", 
            new Vector2(60, 25), 
            P.config.Insert ? Colour.Blue : Colour.Red))
        {
            P.config.Insert = !P.config.Insert;
        }
    }
}


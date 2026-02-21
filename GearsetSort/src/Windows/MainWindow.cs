namespace GearsetSort.Windows;

public unsafe class MainWindow : Window
{
    public MainWindow() : base($"GearsetSort {P.GetType().Assembly.GetName().Version} ###GearsetSortMainWindow")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize;

        SizeConstraints = new()
        {
          MaximumSize = new Vector2(300,700)  
        };

        P.windowSystem.AddWindow(this);
        AllowPinning = false;
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    private static ImGuiEx.RealtimeDragDrop<GearsetManager.Gearset> DragDrop = new("GearsetOrder", x => x.id.ToString());

    public unsafe override void Draw()
    {
        DragDrop.Begin();
        if (ImGui.BeginTable("GearsetReorder", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("##ctrl");
            ImGui.TableSetupColumn("Order");
            ImGui.TableSetupColumn("Gearset Name");
            ImGui.TableHeadersRow();
            for (var index = 0; index < GearsetManager.gearsets.Count; index++)
            {
                var entry = GearsetManager.gearsets[index];
                ImGui.PushID(entry.id);
                if (entry.id == 255)
                    break;
                ImGui.TableNextRow();
                DragDrop.NextRow();
                DragDrop.SetRowColor(entry.id.ToString());
                ImGui.TableNextColumn();
                DragDrop.DrawButtonDummy(entry, GearsetManager.gearsets, index);
                ImGui.TableNextColumn();
                ImGui.Text((entry.id + 1).ToString());
                ImGui.TableNextColumn();
                ImGui.Text(entry.name);
                ImGui.PopID();
            }
            ImGui.EndTable();
        }
        DragDrop.End();

        var width = ImGui.GetContentRegionAvail().X;
        if (ImGui.Button("Apply", new Vector2(width,30)))
        {
            GearsetManager.ApplyChange();
        }
    }
}

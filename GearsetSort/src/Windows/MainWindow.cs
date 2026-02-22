using System.Text;

namespace GearsetSort.Windows;

public class MainWindow : Window
{
    public MainWindow() : base($"GearsetSort {P.GetType().Assembly.GetName().Version} ###GearsetSortMainWindow")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize;

        SizeConstraints = new()
        {
            MinimumSize = new Vector2(175,200),
            MaximumSize = new Vector2(300,700)  
        };

        P.windowSystem.AddWindow(this);
        AllowPinning = false;
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    public unsafe override void Draw()
    {
        // Learnt a lot of the process from ECommons' DragDrop class. ImGui's dragdrop is not good


        if (ImGui.BeginTable("GearsetReorder", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Order");
            ImGui.TableSetupColumn("Gearset Name");
            ImGui.TableHeadersRow();

            for (var index = 0; index < GearsetManager.gearsets.Count; index++)
            {
                var entry = GearsetManager.gearsets[index];
                var uniqueId = entry.id.ToString();
                if (entry.id == 255) break;


                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Selectable($"{entry.id + 1}", false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap);

                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceNoPreviewTooltip))
                {
                    // why is this so awful, who made this
                    byte[] payload = Encoding.UTF8.GetBytes(uniqueId);
                    ImGui.SetDragDropPayload("GearsetOrder", payload);
                    uint col = ImGui.GetColorU32(new Vector4(0f, 1f, 0f, 0.25f));

                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, col);
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, col);
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, col);
                    ImGui.EndDragDropSource();
                }
 
                if (ImGui.BeginDragDropTarget())
                {
                    var payLoadAcceptBefore = ImGui.AcceptDragDropPayload("GearsetOrder", ImGuiDragDropFlags.AcceptNoDrawDefaultRect | ImGuiDragDropFlags.AcceptBeforeDelivery);
                    if ((ImGuiPayload*)payLoadAcceptBefore != null)
                    {
                        uint col = ImGui.GetColorU32(new Vector4(1f, 0f, 0f, 0.25f));

                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, col);
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, col);
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, col);
                    }

                    var payLoadNormal = ImGui.AcceptDragDropPayload("GearsetOrder", ImGuiDragDropFlags.AcceptNoDrawDefaultRect);
                    if ((ImGuiPayload*)payLoadNormal != null)
                    {
                        string payloadString = Encoding.UTF8.GetString((byte*)payLoadNormal.Data, payLoadNormal.DataSize);
                        var sourceIndex = GearsetManager.gearsets.FindIndex(x => x.id.ToString() == payloadString);
                        var item = GearsetManager.gearsets[sourceIndex];
                        GearsetManager.gearsets.RemoveAt(sourceIndex);
                        GearsetManager.gearsets.Insert(index, item);
                    }
                    ImGui.EndDragDropTarget();
                }
                ImGui.TableNextColumn();
                ImGui.Text(entry.name);
            }
            ImGui.EndTable();
        }

        var width = ImGui.GetContentRegionAvail().X;
        if (ImGui.Button("Apply", new Vector2(width,30)))
        {
            GearsetManager.ApplyChange();
        }
    }
}

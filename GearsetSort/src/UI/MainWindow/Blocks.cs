using Dalamud.Game.Text;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Dalamud.Interface;

namespace GearsetSort.UI;

public partial class MainWindow : Window
{
    public void ScrollableBlock()
    {
        using var ScrollableBlock = ImRaii.Child("##ScrollableBlock", ImEx.Vec2(190, 432), true);

        if (!ScrollableBlock) return;

        using var GearsetTable = ImRaii.Table("##GearsetTable", 3, 
            ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PadOuterX);

        if (!GearsetTable) return;

        ImGui.TableSetupColumn("##Order");
        ImGui.TableSetupColumn("##JobIcon");
        ImGui.TableSetupColumn("##GearsetName");

        for (var index = 0; index < Core.gearsets.Count; index++)
        {
            DrawRow(index);
        }
    }

    private void DrawRow(int index)
    {
        var gearset = Core.gearsets[index];
        var gearsetId = gearset.id;
        if (gearsetId == 255) return;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        if (ImGui.Selectable($"{gearsetId+1}", false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap))
            selectedGearsetId = index;
        
        ImEx.ItemMouseClicked(ImGuiMouseButton.Left, Core.EquipGearset, true, gearsetId);

        DrawContextMenu(gearset);

        DragDropSource(gearsetId.ToString());
        DragDropTarget(index);

        ImGui.TableNextColumn();
        
        if (Util.GetJobIcon(gearset.classJob)?.GetWrapOrEmpty() is { } wrap)
            ImEx.Image(wrap, new(20, 20));

        ImGui.TableNextColumn();

        GearsetNameHandler(gearset);
    }

    public void PreviewBlock()
    {
        using var PreviewBlock = ImRaii.Child("##GearsetPreview", ImEx.Vec2(350, 432), true);

        if (!PreviewBlock) return;

        if (selectedGearsetId == null)
        {
            ImEx.Text("Select a gearset", Colour.SoftBlue, ImExFlags.CentreX | ImExFlags.CentreY);
            return;
        }

        var selectedGearset = Core.gearsets[selectedGearsetId.Value];

        ImEx.Text(selectedGearset.name, Util.JobToColour(selectedGearset.classJob), ImExFlags.CentreX);

        ImEx.Text($"{(char)SeIconChar.ItemLevel} {selectedGearset.itemLevel}", null, ImExFlags.SameLine | ImExFlags.RightAlign);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var cursorPos = ImGui.GetCursorPos();
        var blockSize = ImGui.GetContentRegionAvail();
        if (selectedGearset.items.Count == 0) return;
        var storedCategoryChange = selectedGearset.items[0].majorCategory;
        foreach (var item in selectedGearset.items)
        {
            ImGui.Spacing();
            if (item.majorCategory != storedCategoryChange)
            {
                ImGui.Separator();
                ImGui.Spacing();
                storedCategoryChange = item.majorCategory;
            }

            if (Util.GetTextureFromIcon(item.iconId, ItemUtil.IsHighQuality(item.iconId))?.GetWrapOrEmpty() is not { } wrap) continue;
                
            using (var TextBlock = ImRaii.Group())
            {
                if (!TextBlock) continue;

                ImEx.Image(wrap, new(20,20));
                ImEx.Text(item.name, null, ImExFlags.SameLine);
            }
            ImEx.HoverToolTip(() =>
            {
                if (item.materia.Count == 0)
                {
                    ImEx.Text("No Materia");
                    return;
                }
                foreach (var materia in item.materia)
                {
                    if (Util.GetTextureFromIcon(materia.iconId)?.GetWrapOrEmpty() is not { } wrap) continue;
                    ImEx.Image(wrap, new(15, 15));
                    ImEx.Text(materia.name, flags: ImExFlags.SameLine);
                }
            });

            // I don't actually know what Unknown02 is still. But I had a missing item that was using it instead of ItemMissing(???)
            if ((item.content.Flags & (RaptureGearsetModule.GearsetItemFlag.ItemMissing | RaptureGearsetModule.GearsetItemFlag.Unknown02)) != 0)
            {
                ImEx.FontAwesomeIcon(FontAwesomeIcon.ExclamationTriangle.ToIconString(), Colour.Red, ImExFlags.SameLine | ImExFlags.RightAlign);
                ImEx.HoverToolTip("Missing Item");
            }
        }
    }
}

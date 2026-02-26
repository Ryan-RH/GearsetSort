using System.Reflection.Metadata;
using System.Text;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace GearsetSort.UI;

public partial class MainWindow : Window
{
    private float blockHeight = ImGuiHelpers.GlobalScale * 27 * ImGui.GetTextLineHeight();

    public void ScrollableBlock()
    {
        using var ScrollableBlock = ImRaii.Child("##ScrollableBlock", 
            new Vector2(ImGuiHelpers.GlobalScale * 190, blockHeight), 
            true);

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

        if (ImGui.Selectable($"{gearsetId + 1}", false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap))
            selectedGearset = gearset;

        DragDropSource(gearsetId.ToString());
        DragDropTarget(index);

        ImGui.TableNextColumn();
        var wrap = gearset.classJobIcon.GetWrapOrDefault();
        if (wrap!= null)
            ImGui.Image(wrap.Handle, new Vector2(20,20));

        ImGui.TableNextColumn();
        ImGui.Text(gearset.name);
    }

    public void PreviewBlock()
    {
        using var PreviewBlock = ImRaii.Child("##GearsetPreview", new Vector2(350 * ImGuiHelpers.GlobalScale, blockHeight), true);

        if (!PreviewBlock) return;

        if (selectedGearset == null)
        {
            Util.CentreText("Select a gearset", new Vector4(0.605f, 0.755f, 0.746f, 1f), true);
            return;
        }

        Util.CentreText(selectedGearset.name, new Vector4(0.4f, 0.8f, 1f, 1f), false);

        ImGui.SameLine();


        var scaledCursorPosX = ImGui.GetCursorPosX() -ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(selectedGearset.name).X;
        ImGui.SetCursorPosX(scaledCursorPosX + 275 * ImGuiHelpers.GlobalScale);
        ImGui.Text($"{(char)SeIconChar.ItemLevel} {selectedGearset.itemLevel.ToString()}");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var cursorPos = ImGui.GetCursorPos();
        var blockSize = ImGui.GetContentRegionAvail();
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
            var wrap = item.texture.GetWrapOrEmpty();
            if (wrap == null) continue;
                
            using (var TextBlock = ImRaii.Group())
            {
                if (!TextBlock) continue;

                ImGui.Image(wrap.Handle, new Vector2(20,20));
                ImGui.SameLine();
                ImGui.Text(item.name);
            }
            if (ImGui.IsItemHovered())
            {
                ToolTip(item);
            }
        }

        ImGui.SetCursorPos(
            new Vector2(cursorPos.X + ImGuiHelpers.GlobalScale * 200, 
            cursorPos.Y + blockSize.Y - ImGuiHelpers.GlobalScale * 25));

        if (Util.ColourButton("Update", new Vector2(60, 25), Colour.Yellow, !ImGui.IsKeyDown(ImGuiKey.LeftCtrl)))
        {
            if (selectedGearset != null)
                Core.ChangeGearset(selectedGearset.id);
        }
        Util.HoverToolTip("Hold \"Left-Ctrl\" and click the button\nto change this gearset with your currently equipped.");
        

        ImGui.SameLine();

        if (Util.ColourButton("Equip", new Vector2(60, 25), Colour.Green))
        {   
            if (selectedGearset != null)
                Core.EquipGearset(selectedGearset.id);
        }
    }

    private void ToolTip(Core.GearItem item)
    {
        using var ToolTip = ImRaii.Tooltip();
                    
        if (!ToolTip) return;

        if (item.materia.Count == 0)
        {
            ImGui.Text("No Materia");
            return;
        }

        foreach (var materia in item.materia)
        {
            var wrapMateria = materia.texture.GetWrapOrEmpty();
            if (wrapMateria == null) return;

            ImGui.Image(wrapMateria.Handle, new Vector2(15, 15));
            ImGui.SameLine();
            ImGui.Text(materia.name);
        }
    }
}
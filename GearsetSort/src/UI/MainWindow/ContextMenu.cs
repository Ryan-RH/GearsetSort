using Dalamud.Interface.Utility.Raii;
using GearsetSort.Models;

namespace GearsetSort.UI;

public partial class MainWindow : Window
{
    public void DrawContextMenu(Gearset gearset)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.PopupBg, Colour.TransparentGrey);
        
        using var contextMenu = ImRaii.ContextPopupItem($"contextMenu##{gearset.id}");
        if (!contextMenu) return;
        
        if (ImGui.MenuItem("Equip"))
        {
            Core.EquipGearset(gearset.id);
        }

        if (ImGui.MenuItem("Rename"))
        {
            editBuffer = gearset.name;
            indexToRename = gearset.id;
        }

        using (var group = ImRaii.Group())
        {
            var ctrlDown = ImGui.IsKeyDown(ImGuiKey.LeftCtrl);
            using var disabled = ImRaii.Disabled(!ctrlDown);
            using (var updateColour = ImRaii.PushColor(ImGuiCol.Text, Colour.Yellow))
            {
                if (ImGui.MenuItem("Update")) Core.UpdateGearset(gearset.id);
            }
            
            using var deleteColour = ImRaii.PushColor(ImGuiCol.Text, Colour.Red);
            var pos = ImGui.GetCursorPos();
            ImGui.Selectable("Delete", false, ImGuiSelectableFlags.DontClosePopups);
            if (ImEx.HoldLeftClick(2, pos))
            {
                ImGui.CloseCurrentPopup();
                Core.DeleteGearset(gearset.id);
                selectedGearsetId = null;
            }
        }
        ImEx.HoverToolTip("Hold Ctrl to Select");
    }

    private string editBuffer = string.Empty;
    private int indexToRename = -1;
    public void GearsetNameHandler(Gearset gearset)
    {
        if (indexToRename != gearset.id)
        {
            ImEx.Text(gearset.name);
        }
        else 
        {
            ImGui.SetKeyboardFocusHere();
            if (!ImEx.InputText($"##{gearset.id}", ref editBuffer, 15, true, new Vector2(100, 18), ImGuiInputTextFlags.EnterReturnsTrue) || editBuffer == "") return;
            if (editBuffer != gearset.name)
            {
                Core.RenameGearset(gearset.id, editBuffer);
            }
            editBuffer = string.Empty;
            indexToRename = -1;
        }
    }
}

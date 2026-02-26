

using System.Text;
using Dalamud.Interface.Utility.Raii;

namespace GearsetSort.UI;

public partial class MainWindow : Window
{
    public void DragDropSource(string payloadSend)
    {
        using (var DragDropSource = ImRaii.DragDropSource(ImGuiDragDropFlags.SourceNoPreviewTooltip))
        {
            if (!DragDropSource) return;

            var payload = Encoding.UTF8.GetBytes(payloadSend);
            ImGui.SetDragDropPayload("GearsetPayload", payload);
        }

        Util.SetTableBackgroundColour(new Vector4(0.262f, 0.844f, 0.178f, 0.25f));
    }

    private unsafe void DragDropTarget(int indexToDeliver)
    {
        using (var DragDropTarget = ImRaii.DragDropTarget())
        {
            if (!DragDropTarget) return;
            
            var payLoadDrop = ImGui.AcceptDragDropPayload("GearsetPayload", ImGuiDragDropFlags.AcceptNoDrawDefaultRect);
            if ((ImGuiPayload*)payLoadDrop != null)
            {
                string payloadString = Encoding.UTF8.GetString((byte*)payLoadDrop.Data, payLoadDrop.DataSize);
                var sourceIndex = Core.gearsets.FindIndex(x => x.id.ToString() == payloadString);
                var sourceGearset = Core.gearsets[sourceIndex];

                if (P.config.Insert)
                {
                    Core.gearsets.RemoveAt(sourceIndex);
                    Core.gearsets.Insert(indexToDeliver, sourceGearset);
                }
                else
                {
                    Core.gearsets[sourceIndex] = Core.gearsets[indexToDeliver];
                    Core.gearsets[indexToDeliver] = sourceGearset;
                }
            }
        }
        Util.SetTableBackgroundColour(new Vector4(0.28f, 0.84f, 0.76f, 0.25f));
    }
}
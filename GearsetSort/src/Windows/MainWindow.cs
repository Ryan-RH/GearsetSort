using System.Text;

namespace GearsetSort.Windows;

public class MainWindow : Window
{
    public MainWindow() : base($"GearsetSort {P.GetType().Assembly.GetName().Version} ###GearsetSortMainWindow")
    {
        Flags = ImGuiWindowFlags.NoResize;

        SizeConstraints = new()
        {
            MinimumSize = new Vector2(540,475),
            MaximumSize = new Vector2(540,475)  
        };

        P.windowSystem.AddWindow(this);
        AllowPinning = false;
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    public GearsetManager.Gearset? selectedGearset = null;

    public override void OnClose()
    {
        selectedGearset = null;
    }

    public unsafe override void Draw()
    {
        // Learnt a lot of the process from ECommons' DragDrop class. ImGui's dragdrop is not good
        
        ImGui.BeginChild("ScrollableTable", new Vector2(190, 20 * ImGui.GetTextLineHeightWithSpacing()), true, ImGuiWindowFlags.AlwaysAutoResize);
        if (ImGui.BeginTable("GearsetReorder", 3, ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PadOuterX))
        {
            ImGui.TableSetupColumn("##Order");
            ImGui.TableSetupColumn("##JobIcon");
            ImGui.TableSetupColumn("##GearsetName");

            for (var index = 0; index < GearsetManager.gearsets.Count; index++)
            {
                var entry = GearsetManager.gearsets[index];
                var uniqueId = entry.id.ToString();
                if (entry.id == 255) continue;


                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{entry.id + 1}", false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap))
                {
                    if (ImGui.IsKeyDown(ImGuiKey.LeftShift))
                        Log.Debug($"Test");
                    selectedGearset = entry;
                }

                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceNoPreviewTooltip))
                {
                    // why is this so awful, who made this
                    byte[] payload = Encoding.UTF8.GetBytes(uniqueId);
                    ImGui.SetDragDropPayload("GearsetOrder", payload);
                    uint col = ImGui.GetColorU32(new Vector4(0.262f, 0.844f, 0.178f, 0.15f));

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
                        uint col = ImGui.GetColorU32(new Vector4(0.28f, 0.84f, 0.76f, 0.15f));

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
                        if (P.config.Insert)
                        {
                            GearsetManager.gearsets.RemoveAt(sourceIndex);
                            GearsetManager.gearsets.Insert(index, item);
                        }
                        else
                        {
                            GearsetManager.gearsets[sourceIndex] = GearsetManager.gearsets[index];
                            GearsetManager.gearsets[index] = item;
                        }
                    }
                    ImGui.EndDragDropTarget();
                }
                ImGui.TableNextColumn();
                var wrap = entry.classJob.GetWrapOrDefault();
                if (wrap!= null)
                    ImGui.Image(wrap.Handle, new Vector2(20,20));
                ImGui.TableNextColumn();
                ImGui.Text(entry.name);
            }
            ImGui.EndTable();
        }
        ImGui.EndChild();
        ImGui.SameLine();
        ImGui.BeginChild("##GearsetPreview", new Vector2(325, 400), true);
        var childWindowSize = ImGui.GetContentRegionAvail();
        if (selectedGearset == null)
        {
            string placeholderText = "Select a gearset";
            var cursorPos = ImGui.GetCursorPos();
            var textSize = ImGui.CalcTextSize(placeholderText);
            ImGui.SetCursorPosX(cursorPos.X + childWindowSize.X / 2 - textSize.X / 2);
            ImGui.SetCursorPosY(childWindowSize.Y / 2 - textSize.Y / 2);
            ImGui.TextColored(new Vector4(0.605f, 0.755f, 0.746f, 1f),placeholderText);
        }
        else
        {
            var cursorPos = ImGui.GetCursorPos();
            var textSize = ImGui.CalcTextSize(selectedGearset.name);
            ImGui.SetCursorPosX(cursorPos.X + childWindowSize.X / 2 - textSize.X / 2);
            ImGui.TextColored(new Vector4(0.4f, 0.8f, 1f, 1f),selectedGearset.name);
            ImGui.Separator();
            ImGui.Spacing();
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
                if (wrap != null)
                {
                    ImGui.Image(wrap.Handle, new Vector2(15,15));
                    ImGui.SameLine();
                    ImGui.Text(item.name);
                }
            }
            ImGui.SetCursorPos(new Vector2(cursorPos.X+175, cursorPos.Y + childWindowSize.Y - 25));
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.87f, 0.67f, 0.17f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.67f, 0.47f, 0f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.67f, 0.47f, 0f, 1f));
            if (!ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                ImGui.BeginDisabled();
                ImGui.Button("Update", new Vector2(60, 25));
                ImGui.EndDisabled();
            }
            else if (ImGui.Button("Update", new Vector2(60, 25)))
            {
                if (selectedGearset != null)
                    GearsetManager.ChangeGearset(selectedGearset.id);
            }
            ImGui.PopStyleColor(3);
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0f, 0.45f, 0f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0f, 0.25f, 0f, 1f));
            if (ImGui.Button("Equip", new Vector2(60, 25)))
            {   
                if (selectedGearset != null)
                    GearsetManager.EquipGearset(selectedGearset.id);
            }
            ImGui.PopStyleColor(3);
        }
        ImGui.EndChild();
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0f, 0.45f, 0f, 1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0f, 0.25f, 0f, 1f));
        if (ImGui.Button("Apply", new Vector2(190,30)))
        {
            GearsetManager.ApplyChange();
        }
        ImGui.PopStyleColor(3);

        ImGui.SameLine();
        var cursorPosMode = ImGui.GetCursorPos();
        ImGui.SetCursorPosY(cursorPosMode.Y + 4);
        ImGui.SetCursorPosX(cursorPosMode.X + 90);
        ImGui.Text("Mode:");
        ImGui.SameLine();

        ImGui.SetCursorPosY(cursorPosMode.Y + 3);
        ImGui.PushStyleColor(ImGuiCol.Button, P.config.Insert ? new Vector4(0.2f, 0.6f, 0.8f, 1f) : new Vector4(0.5f, 0.1f, 0.1f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, P.config.Insert ? new Vector4(0.4f, 0.8f, 1f, 1f) : new Vector4(0.7f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, P.config.Insert ? new Vector4(0.4f, 0.8f, 1f, 1f) : new Vector4(0.7f, 0.3f, 0.3f, 1.0f));
        if (ImGui.Button(P.config.Insert ? "Insert" : "Swap", new Vector2(60, 25)))
            P.config.Insert = !P.config.Insert;
        ImGui.PopStyleColor(3);
    }
}


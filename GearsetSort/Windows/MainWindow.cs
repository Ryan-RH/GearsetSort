using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Ipc.Exceptions;
using Dalamud.Plugin.SelfTest;
using ECommons.Configuration;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;


namespace GearsetSort.Windows;

public unsafe class MainWindow : Window
{
    public class Gearset
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public static List<Gearset> gearsets = new();

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

    private static ImGuiEx.RealtimeDragDrop<Gearset> DragDrop = new("GearsetOrder", x => x.Id.ToString());

    public unsafe override void Draw()
    {
        DragDrop.Begin();
        if (ImGui.BeginTable("GearsetReorder", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("##ctrl");
            ImGui.TableSetupColumn("Order");
            ImGui.TableSetupColumn("Gearset Name");
            ImGui.TableHeadersRow();
            for (var index = 0; index < gearsets.Count; index++)
            {
                var entry = gearsets[index];
                ImGui.PushID(entry.Id);
                if (entry.Id == 255)
                    break;
                ImGui.TableNextRow();
                DragDrop.NextRow();
                DragDrop.SetRowColor(entry.Id.ToString());
                ImGui.TableNextColumn();
                DragDrop.DrawButtonDummy(entry, gearsets, index);
                ImGui.TableNextColumn();
                ImGui.Text(entry.Id.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(entry.Name);
                ImGui.PopID();
            }
            ImGui.EndTable();
        }
        DragDrop.End();

        var width = ImGui.GetContentRegionAvail().X;
        if (ImGui.Button("Apply", new Vector2(width,30)))
        {
            Svc.Log.Info("Applying");
            var instance = RaptureGearsetModule.Instance();
            int[] targetIndexes = new int[gearsets.Count];
            for (int index = 0; index < gearsets.Count; index++)
            {
                targetIndexes[gearsets[index].Id] = index;
            }

            for (int i = 0; i < gearsets.Count; i++)
            {
                for (int j = 0; j < targetIndexes.Length; j++)
                {
                    if (targetIndexes[j] == i)
                    {
                        if (j != i)
                        {
                            instance->ReassignGearsetId(j, i);
                            (targetIndexes[j], targetIndexes[i]) = (targetIndexes[i], targetIndexes[j]);
                        }
                        break;
                    }
                }
            }
            FetchGearsets();
        }
    }
}

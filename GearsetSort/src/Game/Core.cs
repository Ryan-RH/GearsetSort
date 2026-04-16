using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using GearsetSort.Models;
using Dalamud.Game.Gui.Toast;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace GearsetSort;

public unsafe class Core
{
    public static List<Gearset> gearsets = new();

    public static void ApplyChange()
    {
        ResortGearsets();
        FetchGearsets();
    }

    public static void FetchGearsets()
    {
        if (!GetRaptureGS(out var gearsetModule)) return;
        var itemSheet = Data.GetExcelSheet<Item>();
        var materiaSheet = Data.GetExcelSheet<Materia>();

        gearsets.Clear();


        foreach (var entry in gearsetModule->Entries)
        {
            if (entry.Id == 255 || entry.NameString == "" || !gearsetModule->IsValidGearset(entry.Id)) continue;
                
            List<SetItem> items = new();

            foreach (var item in entry.Items)
            {
                var itemId = item.ItemId;
                var itemRow = itemSheet.GetRow(itemId % 100000);
                if (itemRow.RowId == 0 || itemRow.EquipSlotCategory.RowId == 17) continue;

                List<SetMateria> materia = new();
                for (int i = 0; i < item.Materia.Length; i++)
                {
                    var matRow = materiaSheet.GetRow(item.Materia[i]);
                    if (matRow.RowId == 0) continue;
                    var matObj = matRow.Item[item.MateriaGrades[i]];
                    materia.Add(new SetMateria(matObj.Value.Icon, matObj.Value.Name.ToString()));
                }
                
                SetItem foundItem = new(itemRow.Icon, itemRow.Name.ToString(), itemRow.ItemUICategory.Value.OrderMajor, materia, item);
                items.Add(foundItem);
            }
            Gearset gearset = new(entry.Id, entry.NameString, entry.ClassJob, items, entry.ItemLevel);
            gearsets.Add(gearset);
        }
    }

    private static void ResortGearsets(bool log = true)
    {
        Log.Info("Applying");
        P.memory.EnableLogHook();
        for (int i = 0; i < gearsets.Count; i++)
        {
            var interest = gearsets[i];
            if (interest.id == i) continue;

            var toSwap = gearsets.FindIndex(x => x.id == i);
            var temp = interest.id;
            gearsets[i] = gearsets[i] with { id = i };
            if (toSwap != -1)
            {
                gearsets[toSwap] = gearsets[toSwap] with { id = temp };
            }
            ReassignGearsetId(i, temp);
        }
        P.memory.DisableLogHook();
        if (log) ToastGui.ShowNormal("Gearset Order Changed", new ToastOptions { Speed = ToastSpeed.Fast });
        FetchGearsets();
    }

    public static void ReassignGearsetId(int newGearsetId, int gearsetId)
    {
        if (!GetAgentGS(out var instance)) return;
        instance->ReassignGearsetId(gearsetId, newGearsetId);
    }

    public static void EquipGearset(int gearsetId)
    {
        if (!GetRaptureGS(out var instance)) return;
        instance->EquipGearset(gearsetId);
    }

    public static void UpdateGearset(int gearsetId)
    {
        if (!GetAgentGS(out var instance)) return;
        instance->UpdateGearset(gearsetId);
        FetchGearsets(); 
    }

    public static void DeleteGearset(int gearsetId) 
    {
        if (!GetAgentGS(out var instance)) return;

        instance->DeleteGearset(gearsetId);
        var slot = gearsets.FindIndex(x => x.id == gearsetId);
        if (slot != -1) gearsets.RemoveAt(slot);
    }

    public static void RenameGearset(int gearsetId, string name)
    {
        if (!GetAgentGS(out var instance)) return;
        instance->RenameGearset(gearsetId, name);
        FetchGearsets();
    }

    private static bool GetAgentGS(out AgentGearSet* instance)
    {
        instance = AgentGearSet.Instance();
        if (instance == null)
        {
            Log.Error("AgentGearSet was not found.");
            return false;
        }
        return true;
    }

    private static bool GetRaptureGS(out RaptureGearsetModule* instance)
    {
        instance = RaptureGearsetModule.Instance();
        if (instance == null)
        {
            Log.Error("RaptureGearsetModule was not found.");
            return false;
        }
        return true;
    }
}

using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace GearsetSort;

public class GearsetManager
{
    public static List<Gearset> gearsets = new();

    public record Gearset
    (
        int id,
        string name
    );

    public static void ApplyChange()
    {
        ResortGearsets();
        RefreshAddon();
        FetchGearsets();
    }

    public static unsafe void FetchGearsets()
    {
        var gearsetModule = RaptureGearsetModule.Instance();
        var entries = gearsetModule->Entries;

        gearsets.Clear();

        foreach (var entry in entries)
        {
            // Skip invalid / empty gearsets if needed
            if (entry.Id == 255 || entry.NameString == "")
                break;
            if (!gearsetModule->IsValidGearset(entry.Id))
                break;

            var gearset = new Gearset(entry.Id, entry.NameString);
            gearsets.Add(gearset);
        }
    }

    public static unsafe void ResortGearsets()
    {
        Svc.Log.Info("Applying");
        var gearsetModule = RaptureGearsetModule.Instance();
        var hotbarModule = RaptureHotbarModule.Instance();
        int[] targetIndexes = new int[gearsets.Count];
        for (int index = 0; index < gearsets.Count; index++)
        {
            targetIndexes[gearsets[index].id] = index;
        }

        for (int i = 0; i < gearsets.Count; i++)
        {
            for (int j = 0; j < targetIndexes.Length; j++)
            {
                if (targetIndexes[j] == i)
                {
                    if (j != i)
                    {
                        
                        gearsetModule->ReassignGearsetId(j, i);
                        hotbarModule->ReassignGearsetId(j, i);
                        (targetIndexes[j], targetIndexes[i]) = (targetIndexes[i], targetIndexes[j]);
                    
                    }
                    break;
                }
            }
        }
    }

    private static unsafe void RefreshAddon()
    {
        var addon = Svc.GameGui.GetAddonByName("GearSetList");
        if (addon != null)
        {
            var addonBase = (AtkUnitBase*)addon.Address;
            var atkUnitManager = RaptureAtkUnitManager.Instance();
            atkUnitManager->RefreshAddon(addonBase, addonBase->AtkValuesCount, addonBase->AtkValues);
        }
    }
}
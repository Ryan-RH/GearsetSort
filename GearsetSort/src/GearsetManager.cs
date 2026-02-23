using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Textures;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace GearsetSort;

public class GearsetManager
{
    public static List<Gearset> gearsets = new();

    public record GearItem
    (
        ISharedImmediateTexture texture,
        string name,
        byte majorCategory
    );

    public record Gearset
    (
        int id,
        string name,
        ISharedImmediateTexture classJob,
        List<GearItem> items
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
            List<GearItem> items = new List<GearItem>();
            foreach (var item in entry.Items)
            {
                var itemId = item.ItemId;
                var itemRow = Data.GetExcelSheet<Item>().FirstOrDefault(x => x.RowId == itemId % 100000);
                if (!itemRow.Equals(default(Item)) && itemRow.RowId != 0 && itemRow.EquipSlotCategory.RowId != 17)
                {
                    string? path = null;
                    if (!ItemUtil.IsHighQuality(itemId))
                    {
                        path = $"ui/icon/{itemRow.Icon / 1000 * 1000:000000}/{itemRow.Icon:000000}.tex";
                    }
                    else
                    {
                        path = $"ui/icon/{itemRow.Icon / 1000 * 1000:000000}/hq/{itemRow.Icon:000000}.tex";
                    }
                    var texture = Texture.GetFromGame(path);
                    if (texture != null)
                    {
                        GearItem foundItem = new GearItem(texture, ItemUtil.GetItemName(itemId).ToString(), itemRow.ItemUICategory.Value.OrderMajor);
                        items.Add(foundItem);
                    }
                }
            }
            var jobTexture = Texture.GetFromGame($"ui/icon/062000/0621{entry.ClassJob:00}.tex");
            var gearset = new Gearset(entry.Id, entry.NameString, jobTexture, items);
            gearsets.Add(gearset);
        }
    }

    public static unsafe void ResortGearsets()
    {
        Log.Info("Applying");
        ToastHandler.handled = true;
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
        var addon = GameGui.GetAddonByName("GearSetList");
        if (addon != null)
        {
            var addonBase = (AtkUnitBase*)addon.Address;
            var atkUnitManager = RaptureAtkUnitManager.Instance();
            atkUnitManager->RefreshAddon(addonBase, addonBase->AtkValuesCount, addonBase->AtkValues);
        }
    }
}
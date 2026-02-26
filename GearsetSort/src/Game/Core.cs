using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Interface.Textures;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using GearsetSort.Models;

namespace GearsetSort;

public partial class Core
{
    public static List<Gearset> gearsets = new();

    public record CurrentAndTarget
    {
        public int current { get; set; }
        public int target { get; }

        public CurrentAndTarget(int current, int target)
        {
            this.current = current;
            this.target = target;
        }
    }
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
            if (entry.Id == 255 || entry.NameString == "")
                continue;
            if (!gearsetModule->IsValidGearset(entry.Id))
                continue;
            List<SetItem> items = new();
            var itemSheet = Data.GetExcelSheet<Item>();
            var materiaSheet = Data.GetExcelSheet<Materia>();
            foreach (var item in entry.Items)
            {
                var itemId = item.ItemId;
                var itemRow = itemSheet.FirstOrDefault(x => x.RowId == itemId % 100000);
                if (itemRow.Equals(default(Item)) || itemRow.RowId == 0 || itemRow.EquipSlotCategory.RowId == 17)
                    continue;

                var texture = GetTextureFromIcon(itemRow.Icon, ItemUtil.IsHighQuality(itemId));
                if (texture == null)
                    continue;

                List<SetMateria> materia = new();
                for (int i = 0; i < item.Materia.Length; i++)
                {
                    var matObj = materiaSheet.FirstOrDefault(x => x.RowId == item.Materia[i]).Item[item.MateriaGrades[i]];
                    if (matObj.RowId == 0)
                        break;

                    var matTexture = GetTextureFromIcon(matObj.Value.Icon);
                    if (matTexture != null)
                        materia.Add(new SetMateria(matTexture, matObj.Value.Name.ToString()));
                }

                SetItem foundItem = new SetItem(texture, ItemUtil.GetItemName(itemId).ToString(), itemRow.ItemUICategory.Value.OrderMajor, materia);
                items.Add(foundItem);
            }
            var jobTexture = Texture.GetFromGame($"ui/icon/062000/0621{entry.ClassJob:00}.tex");
            SetClassJob classJob = new(entry.ClassJob, jobTexture);
            var gearset = new Gearset(entry.Id, entry.NameString, classJob, items, entry.ItemLevel);
            gearsets.Add(gearset);
        }
    }

    private static unsafe void ResortGearsets()
    {
        // This is an intensive function and most likely inefficient
        // Due to the fact it is a button not to be used constantly, I believe its okay to be like this
        // First it fills empty ids. A user can delete a gearset and cause a gap in ids "23, 24, 26, 27"
        // After this it then uses the indexes of the gearsets list as the target of the gearset
        // it then does something i actually forgot, but it works. I lost the text document that explained it
        // think its literally just a bubble sort, it made sense at the time


        Log.Info("Applying");
        ToastHandler.handled = true;
        var gearsetModule = RaptureGearsetModule.Instance();
        var hotbarModule = RaptureHotbarModule.Instance();

        var orderedIds = gearsets.OrderBy(x => x.id).Select(x => x.id).ToList();

        var length = orderedIds.Count;
        for (int i = 0; i < length; i++)
        {
            if (orderedIds[i] != i)
            {
                gearsetModule->ReassignGearsetId(i, orderedIds[length-1]);
                hotbarModule->ReassignGearsetId(i, orderedIds[length-1]);
                var changedIndex = gearsets.FindIndex(x => x.id ==  orderedIds[length-1]);
                gearsets[changedIndex] = gearsets[changedIndex] with { id = i };
                orderedIds[length-1] = i;
                orderedIds.Sort();
            }
        }

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

    public static unsafe void EquipGearset(int index)
    {
        var gearsetModule = RaptureGearsetModule.Instance();
        gearsetModule->EquipGearset(index);
    }

    public static unsafe void ChangeGearset(int index)
    {
        var gearsetModule = RaptureGearsetModule.Instance();
        gearsetModule->UpdateGearset(index);
        FetchGearsets();
    }

}
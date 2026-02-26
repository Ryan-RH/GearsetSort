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

namespace GearsetSort;

public class Core
{
    public static List<Gearset> gearsets = new();

    public record GearMateria
    (
        ISharedImmediateTexture texture,
        string name
    );

    public record GearItem
    (
        ISharedImmediateTexture texture,
        string name,
        byte majorCategory,
        List<GearMateria> materia
    );

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
    public record Gearset
    (
        int id,
        string name,
        ISharedImmediateTexture classJobIcon,
        List<GearItem> items,
        int itemLevel
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
            if (entry.Id == 255 || entry.NameString == "")
                continue;
            if (!gearsetModule->IsValidGearset(entry.Id))
                continue;
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
                    if (texture == null)
                        continue;

                    List<GearMateria> materia = new();
                    for (int i = 0; i < item.Materia.Length; i++)
                    {
                        var mat = item.Materia[i];
                        var matGrade = item.MateriaGrades[i];
                        var matObj = Data.GetExcelSheet<Materia>().FirstOrDefault(x => x.RowId == mat).Item[matGrade];
                        if (matObj.RowId == 0)
                            break;
                        var matIcon = matObj.Value.Icon;
                        var matTexture = Texture.GetFromGame($"ui/icon/{matIcon / 1000 * 1000:000000}/{matIcon:000000}.tex");
                        if (matTexture != null)
                            materia.Add(new GearMateria(matTexture, matObj.Value.Name.ToString()));
                    }

                    GearItem foundItem = new GearItem(texture, ItemUtil.GetItemName(itemId).ToString(), itemRow.ItemUICategory.Value.OrderMajor, materia);
                    items.Add(foundItem);
                }
            }
            var jobTexture = Texture.GetFromGame($"ui/icon/062000/0621{entry.ClassJob:00}.tex");
            var gearset = new Gearset(entry.Id, entry.NameString, jobTexture, items, entry.ItemLevel);
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
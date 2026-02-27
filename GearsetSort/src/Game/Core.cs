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

    public static void ApplyChange()
    {
        ResortGearsets();
        RefreshAddon();
        FetchGearsets();
    }

    public static unsafe void FetchGearsets()
    {
        var gearsetModule = RaptureGearsetModule.Instance();
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
                    

                var texture = GetTextureFromIcon(itemRow.Icon, ItemUtil.IsHighQuality(itemId));
                if (texture == null) continue;
                    

                List<SetMateria> materia = new();
                for (int i = 0; i < item.Materia.Length; i++)
                {
                    var matRow = materiaSheet.GetRow(item.Materia[i]);
                    if (matRow.RowId == 0) continue;
                    var matObj = matRow.Item[item.MateriaGrades[i]];

                    var matTexture = GetTextureFromIcon(matObj.Value.Icon);
                    if (matTexture == null) continue;

                    materia.Add(new SetMateria(matTexture, matObj.Value.Name.ToString()));
                }

                SetItem foundItem = new(texture, itemRow.Name.ToString(), itemRow.ItemUICategory.Value.OrderMajor, materia);
                items.Add(foundItem);
            }
            var jobTexture = Texture.GetFromGame($"ui/icon/062000/0621{entry.ClassJob:00}.tex");
            if (jobTexture == null) continue;

            SetClassJob classJob = new(entry.ClassJob, jobTexture);
            Gearset gearset = new(entry.Id, entry.NameString, classJob, items, entry.ItemLevel);
            gearsets.Add(gearset);
        }
    }

    private static unsafe void ResortGearsets()
    {
        Log.Info("Applying");
        var gearsetModule = RaptureGearsetModule.Instance();
        var hotbarModule = RaptureHotbarModule.Instance();

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
            gearsetModule->ReassignGearsetId(i, temp);
            hotbarModule->ReassignGearsetId(i, temp);
            ToastHandler.toastsToHandle++;
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
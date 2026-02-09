using Dalamud.Game.Command;
using ECommons.Configuration;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using GearsetSort.Windows;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace GearsetSort;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "GearsetSort";
    internal static Plugin P = null!;
    public static Config C => P.config;
    public Config config;


    // Windows
    internal WindowSystem windowSystem;
    internal MainWindow mainWindow;

    public Plugin(IDalamudPluginInterface pi)
    {
        P = this;
        ECommonsMain.Init(pi, P, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions);
        new ECommons.Schedulers.TickScheduler(Load);
    }

    public void Load()
    {
        EzConfig.Migrate<Config>();
        config = EzConfig.Init<Config>();


        windowSystem = new();
        mainWindow = new();

        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenMainUi += () =>
        {
            mainWindow.IsOpen = true;
            FetchGearsets();
        };

        EzCmd.Add("/gearsort", OnCommand);
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        ECommonsMain.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        if (args == "")
        {
            mainWindow.IsOpen = !mainWindow.IsOpen;
            FetchGearsets();
            return;
        }
    }

    public static unsafe void FetchGearsets()
    {
        var gearsetModule = RaptureGearsetModule.Instance();
        var entries = gearsetModule->Entries;

        MainWindow.gearsets.Clear();

        foreach (var entry in entries)
        {
            // Skip invalid / empty gearsets if needed
            if (entry.Id == 255 || entry.NameString == "")
                break;
            if (gearsetModule->IsValidGearset(entry.Id))
                break;
            var gearset = new MainWindow.Gearset
            {
                Id = entry.Id,
                Name = entry.NameString // depends on type
            };

            MainWindow.gearsets.Add(gearset);
        }
    }

}

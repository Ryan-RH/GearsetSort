using Dalamud.Game.Command;
using GearsetSort.Windows;

namespace GearsetSort;

public sealed class Plugin : IDalamudPlugin
{
    internal static Plugin P = null!;
    public Config config { get; init; }


    // Windows
    internal WindowSystem windowSystem;
    internal MainWindow mainWindow;

    public Plugin(IDalamudPluginInterface pi)
    {
        P = this;
        Svc.Init(pi);
        config = Svc.PluginInterface.GetPluginConfig() as Config ?? new Config();

        windowSystem = new();
        mainWindow = new();

        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenMainUi += () =>
        {
            mainWindow.IsOpen = true;
            GearsetManager.FetchGearsets();
        };

        Svc.Commands.AddHandler("/gearsort", new CommandInfo(OnCommand));
        Svc.Commands.AddHandler("/gearsetsort", new CommandInfo(OnCommand));
    }


    public void Dispose()
    {
        windowSystem.RemoveAllWindows();
        Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        mainWindow.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        if (args == "")
        {
            mainWindow.IsOpen = !mainWindow.IsOpen;
            GearsetManager.FetchGearsets();
        }
    }
}

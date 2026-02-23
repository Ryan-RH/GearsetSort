using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using GearsetSort.Windows;

namespace GearsetSort;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IToastGui ToastGui { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog  Log { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IDataManager Data { get; private set; } = null!;
    [PluginService] internal static ITextureProvider Texture { get; private set; } = null!;



    internal static Plugin P = null!;
    public Config config { get; init; }


    // Windows
    internal WindowSystem windowSystem;
    internal MainWindow mainWindow;

    public Plugin(IDalamudPluginInterface pi)
    {
        P = this;
        config = PluginInterface.GetPluginConfig() as Config ?? new Config();

        windowSystem = new();
        mainWindow = new();

        ToastGui.Toast += ToastHandler.HandleGearsetToast;

        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += () =>
        {
            mainWindow.IsOpen = true;
            GearsetManager.FetchGearsets();
        };

        CommandManager.AddHandler("/gearsort", new CommandInfo(OnCommand));
        CommandManager.AddHandler("/gearsetsort", new CommandInfo(OnCommand));
    }


    public void Dispose()
    {
        ToastGui.Toast -= ToastHandler.HandleGearsetToast;
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
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

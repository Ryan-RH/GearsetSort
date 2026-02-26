using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using GearsetSort.UI;

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


    public readonly WindowSystem windowSystem = new("GearsetSort");
    private MainWindow mainWindow;

    public Plugin(IDalamudPluginInterface pi)
    {
        P = this;
        config = PluginInterface.GetPluginConfig() as Config ?? new Config();

        mainWindow = new();

        ToastGui.Toast += ToastHandler.HandleGearsetToast;

        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += () => mainWindow.IsOpen = true;
        PluginInterface.UiBuilder.OpenMainUi += () => mainWindow.IsOpen = true;

        CommandManager.AddHandler("/gearsort", new CommandInfo(OnCommand)
        {
            HelpMessage = "Open GearsetSort Interface"
        });
    }


    public void Dispose()
    {
        ToastGui.Toast -= ToastHandler.HandleGearsetToast;
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= () => mainWindow.IsOpen = true;
        PluginInterface.UiBuilder.OpenMainUi -= () => mainWindow.IsOpen = true;

        windowSystem.RemoveAllWindows();
        mainWindow.Dispose();

        CommandManager.RemoveHandler("/gearsort");
        CommandManager.RemoveHandler("/gearsetsort");
    }

    private void OnCommand(string command, string args)
        => mainWindow.IsOpen = !mainWindow.IsOpen;
}

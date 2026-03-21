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
    [PluginService] internal static IDataManager Data { get; private set; } = null!;
    [PluginService] internal static ITextureProvider Texture { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    internal static Plugin P = null!;
    public Config config { get; init; }
    public readonly WindowSystem windowSystem = new("GearsetSort");
    private MainWindow mainWindow;
    public Memory memory;

    public Plugin(IDalamudPluginInterface pi)
    {
        P = this;
        config = PluginInterface.GetPluginConfig() as Config ?? new Config();

        memory = new Memory();

        mainWindow = new();

        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += () => mainWindow.IsOpen = true;
        PluginInterface.UiBuilder.OpenMainUi += () => mainWindow.IsOpen = true;

        CommandManager.AddHandler("/gearsetsort", new CommandInfo(OnCommand)
        {
            HelpMessage = "Open GearsetSort Interface"
        });
        CommandManager.AddHandler("/gss", new CommandInfo(OnCommand)
        {
            HelpMessage = "Shortcut to GearsetSort Interface"
        });
    }


    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= () => mainWindow.IsOpen = true;
        PluginInterface.UiBuilder.OpenMainUi -= () => mainWindow.IsOpen = true;

        memory.Dispose();
        windowSystem.RemoveAllWindows();
        mainWindow.Dispose();

        CommandManager.RemoveHandler("/gearsetsort");
        CommandManager.RemoveHandler("/gss");
    }

    private void OnCommand(string command, string args)
        => mainWindow.IsOpen = !mainWindow.IsOpen;
}

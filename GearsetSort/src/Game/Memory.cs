using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
namespace GearsetSort;

public unsafe class Memory : IDisposable
{
    private Hook<RaptureGearsetModule.Delegates.ShowLogMessage>? showLogMessageHook;

    public Memory()
    {
        showLogMessageHook = GameInteropProvider.HookFromAddress<RaptureGearsetModule.Delegates.ShowLogMessage>(RaptureGearsetModule.Addresses.ShowLogMessage.Value, (_,_,_,_) => { });
    }

    public void EnableLogHook() => showLogMessageHook?.Enable();
    public void DisableLogHook() => showLogMessageHook?.Disable();

    public void Dispose()
    {
        showLogMessageHook?.Dispose();
        showLogMessageHook = null;
    }
}

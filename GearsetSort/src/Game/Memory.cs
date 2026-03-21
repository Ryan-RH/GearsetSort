using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using InteropGenerator.Runtime;
using System.Text;

namespace GearsetSort;

public unsafe class Memory : IDisposable
{
    private delegate void ShowLogMessageDelegate(RaptureGearsetModule* thisPtr, uint logMessageId, int gearsetId, CStringPointer gearsetName); // TODO: Replace with CS in 7.5
    [Signature("E8 ?? ?? ?? ?? 48 8D 8C 24 ?? ?? ?? ?? B3", DetourName = nameof(ShowLogMessageDetour))]
    private Hook<ShowLogMessageDelegate> showLogMessageHook = null!;

    private delegate bool ReassignGearsetIdDelegate(AgentGearSet* thisPtr, int gearsetId, int newGearsetId); // TODO: Replace with CS in 7.5
    [Signature("E9 ?? ?? ?? ?? 48 FF C9 48 3B D1")]
    private ReassignGearsetIdDelegate reassignGearsetIdCall = null!;

    private delegate bool UpdateGearsetDelegate(AgentGearSet* thisPtr, int gearsetId); // TODO: Replace with CS in 7.5
    [Signature("E9 ?? ?? ?? ?? 8B D7 48 8B CE E8 ?? ?? ?? ?? 48 8B 5C 24")]
    private UpdateGearsetDelegate updateGearsetCall = null!;

    private delegate bool RenameGearsetDelegate(AgentGearSet* thisPtr, int gearsetId, CStringPointer newGearsetName); // TODO: Replace with CS in 7.5
    [Signature("48 89 5C 24 ?? 55 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B F1 49 8B F8")]
    private RenameGearsetDelegate renameGearsetCall = null!;

    private delegate bool DeleteGearsetDelegate(AgentGearSet* thisPtr, int gearsetId); // TODO: Replace with CS in 7.5
    [Signature("40 56 41 54 41 56 48 83 EC ?? 48 8B 05")]
    private DeleteGearsetDelegate deleteGearsetCall = null!;

    private void ShowLogMessageDetour(RaptureGearsetModule* thisPtr, uint logMessageId, int gearsetId, CStringPointer gearsetName)
        => showLogMessageHook.Disable(); // this is safe as game is single threaded

    public Memory()
    {
        GameInteropProvider.InitializeFromAttributes(this);
    }

    public bool ReassignGearsetId(int gearsetId, int newGearsetId)
    {
        showLogMessageHook.Enable();
        return reassignGearsetIdCall(AgentGearSet.Instance(), gearsetId, newGearsetId);
    }

    public bool UpdateGearset(int gearsetId)
        => updateGearsetCall(AgentGearSet.Instance(), gearsetId);

    public bool RenameGearset(int gearsetId, string gearsetName)
    {
        fixed (byte* stringPtr = Encoding.UTF8.GetBytes(gearsetName + '\0')) // builds CStringPointer, imgui textinput is limited to 15 so will be safe
        {
            return renameGearsetCall(AgentGearSet.Instance(), gearsetId, stringPtr);
        }
    }

    public bool DeleteGearset(int gearsetId)
        => deleteGearsetCall(AgentGearSet.Instance(), gearsetId);

    public void Dispose()
        => showLogMessageHook.Dispose();
}

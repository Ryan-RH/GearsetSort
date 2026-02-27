using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;

namespace GearsetSort;

public static class ToastHandler
{
    public static int toastsToHandle = 0;

    public static void HandleGearsetToast(ref SeString message, ref ToastOptions options, ref bool isHandled)
    {
        if (message.TextValue.Contains("Gear set number changed"))
        {
            isHandled = toastsToHandle > 0;
            toastsToHandle--;
        }
    }
}
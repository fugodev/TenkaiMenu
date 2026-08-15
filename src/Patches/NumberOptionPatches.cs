using HarmonyLib;
using AmongUs.GameOptions;
using UnityEngine;

namespace TenkaiMenu;

// Helper class for NumberOption patches
public static class NumberOptionPatchHelper
{
    // Resets custom impostor and speed values to prevent anti-cheat kick
    public static void ResetCustomGameOptions()
    {
        try
        {
            var normalOpt = GameOptionsManager.Instance?.currentNormalGameOptions;
            if (normalOpt == null) return;

            // Game default limits: Impostors 1-3, Speed 0.5-3.0
            bool impostorsOutOfBounds = normalOpt.NumImpostors > 3 || normalOpt.NumImpostors < 1;
            bool speedOutOfBounds = normalOpt.PlayerSpeedMod > 3.0f || normalOpt.PlayerSpeedMod < 0.5f;

            // Only reset and notify if values are outside game limits
            if (!(impostorsOutOfBounds || speedOutOfBounds)) return;

            // Reset to safe default values (3 impostors, 2.0 speed)
            normalOpt.NumImpostors = 3;
            normalOpt.PlayerSpeedMod = 2.0f;

            // Only notify host to prevent spam from other players
            if (!Utils.isHost) return;

            // Add chat notification (local only, not broadcast)
            try
            {
                PlayerControl localPlayer = PlayerControl.LocalPlayer;
                if (localPlayer != null)
                {
                    // Add message to chat locally
                    ChatController chatController = DestroyableSingleton<ChatController>.Instance;
                    if (chatController != null)
                    {
                        string chatMsg = "Because you changed Game/Role settings, custom Impostor/Speed values were reset. You can set them again.";
                        chatController.AddChat(localPlayer, chatMsg, false);
                    }
                }
            }
            catch { }

            // Add game notification at bottom-left
            try
            {
                if (DestroyableSingleton<HudManager>.Instance != null)
                {
                    DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage("<color=#ffff00>Custom Impostor/Speed values were reset. You can set them again.</color>");
                }
            }
            catch { }
        }
        catch { }
    }
}

// Found here: https://github.com/astra1dev/AUnlocker/blob/main/src/OptionsPatches.cs

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
public static class NumberOption_Increase
{
    // Increases the value of a numerical game option without limits
    public static bool Prefix(NumberOption __instance)
    {
        if (!CheatToggles.noOptionsLimits) return true;

        // If trying to change a non-speed/impostor setting, reset custom speed/imp first to prevent anti-cheat kick
        if (!Utils.isHideNSeek && __instance.Title is not StringNames.GameNumImpostors and not StringNames.GamePlayerSpeed)
        {
            NumberOptionPatchHelper.ResetCustomGameOptions();
        }

        // Avoid bypassing imp amount and player speed restrictions in non-HnS games
        // due to anticheat restrictions
        if (!Utils.isHideNSeek && __instance.Title is StringNames.GameNumImpostors or StringNames.GamePlayerSpeed)
        {
            return true;
        }

        __instance.Value += __instance.Increment;
        __instance.UpdateValue();
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();

        return false;
    }
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
public static class NumberOption_Decrease
{
    // Decreases the value of a numerical game option without limits
    public static bool Prefix(NumberOption __instance)
    {
        if (!CheatToggles.noOptionsLimits) return true;

        // If trying to change a non-speed/impostor setting, reset custom speed/imp first to prevent anti-cheat kick
        if (!Utils.isHideNSeek && __instance.Title is not StringNames.GameNumImpostors and not StringNames.GamePlayerSpeed)
        {
            NumberOptionPatchHelper.ResetCustomGameOptions();
        }

        // Avoids bypassing imp amount and player speed restrictions in non-HnS games
        // due to anticheat restrictions
        if (!Utils.isHideNSeek && __instance.Title is StringNames.GameNumImpostors or StringNames.GamePlayerSpeed)
        {
            return true;
        }

        __instance.Value -= __instance.Increment;
        __instance.UpdateValue();
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();

        return false;
    }
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
public static class NumberOption_Initialize
{
    // Sets the valid range of a numerical game option to be practically unlimited
    public static void Postfix(NumberOption __instance)
    {
        if (!CheatToggles.noOptionsLimits) return;

        // Avoids bypassing imp amount and player speed restrictions in non-HnS games
        // due to anticheat restrictions
        if (!Utils.isHideNSeek && __instance.Title is StringNames.GameNumImpostors or StringNames.GamePlayerSpeed) return;

        __instance.ValidRange = new FloatRange(-999f, 999f);
    }
}

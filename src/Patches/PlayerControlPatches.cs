using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;

namespace TenkaiMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControl_SetKillTimer
{
    // Prefix patch of PlayerControl.SetKillTimer to remove kill cooldown for the host
    public static void Prefix(PlayerControl __instance, ref float time)
    {
        if (!__instance.AmOwner || !Utils.isHost || !CheatToggles.noKillCd) return;

        time = 0f;
    }
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer
{
    // Prefix patch of PlayerControl.MurderPlayer to log on ConsoleUI when a player tries to kill another player,
    // along with who the killer and target are, and where the kill happened.
    // Also logs when a kill gets saved by a guardian angel.
    public static void Prefix(PlayerControl __instance, PlayerControl target)
    {
        if (!CheatToggles.logDeaths || target == null) return;

        var (realKillerName, displayKillerName, isDisguised) = Utils.GetPlayerIdentity(__instance);
        var targetName = $"<color=#{ColorUtility.ToHtmlStringRGB(target.Data.Color)}>{target.CurrentOutfit.PlayerName}</color>";

        var room = Utils.GetRoomFromPosition(target.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        if (target.protectedByGuardianId != -1)
        {
            ConsoleUI.Log(isDisguised ? $"{realKillerName} (as {displayKillerName}) tried to kill {targetName} in {roomName} (Protected)"
                : $"{realKillerName} tried to kill {targetName} in {roomName} (Protected)");
        }
        else
        {
            ConsoleUI.Log(isDisguised ? $"{realKillerName} (as {displayKillerName}) killed {targetName} in {roomName}"
                : $"{realKillerName} killed {targetName} in {roomName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
public static class PlayerControl_TurnOnProtection
{
    // Prefix patch of PlayerControl.TurnOnProtection to make all protections visible
    public static void Prefix(ref bool visible)
    {
		if (CheatToggles.seeGhosts)
        {
            visible = true;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
public static class PlayerControl_CmdCheckShapeshift
{
    // Prefix patch of PlayerControl.CmdCheckShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate)
    {
        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
public static class PlayerControl_CmdCheckRevertShapeshift
{
    // Prefix patch of PlayerControl.CmdCheckRevertShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate){

        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControl_Shapeshift
{
    // Postfix patch of PlayerControl.Shapeshift to log on ConsoleUI when a player shapeshifts into another player,
    // and who they shapeshifted into. Also logs when a shapeshift gets reverted.
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        if (!CheatToggles.logShapeshifts) return;

        if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup) return;

        var targetPlayerInfo = targetPlayer.Data;

        var room = Utils.GetRoomFromPosition(__instance.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
        {
            ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> undid their shapeshift in {roomName}");
        }
        else
        {
            ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> shapeshifted into " +
                          $"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId)._object.Data.PlayerName}</color> in {roomName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class PlayerControl_RpcSyncSettings
{
    // Prefix patch of PlayerControl.RpcSyncSettings to prevent the anti-cheat from kicking you
    // for some settings that are out of the "original" valid range
    public static bool Prefix(PlayerControl __instance, byte[] optionsByteArray)
    {
        // If custom impostor/speed limits are active, check if values exceed game limits
        if (CheatToggles.noOptionsLimits && Utils.isHost)
        {
            try
            {
                var normalOpt = GameOptionsManager.Instance?.currentNormalGameOptions;
                if (normalOpt != null)
                {
                    // Game default limits: Impostors 1-3, Speed 0.5-3.0
                    bool impostorsOutOfBounds = normalOpt.NumImpostors > 3 || normalOpt.NumImpostors < 1;
                    bool speedOutOfBounds = normalOpt.PlayerSpeedMod > 3.0f || normalOpt.PlayerSpeedMod < 0.5f;

                    // Only reset and notify if values are outside game limits
                    if (impostorsOutOfBounds || speedOutOfBounds)
                    {
                        // Reset to safe default values before syncing
                        normalOpt.NumImpostors = 3;
                        normalOpt.PlayerSpeedMod = 2.0f;

                        // Add notifications
                        try
                        {
                            ChatController chatController = DestroyableSingleton<ChatController>.Instance;
                            if (chatController != null)
                            {
                                string chatMsg = "Because you changed Game/Role settings, custom Impostor/Speed values were reset. You can set them again.";
                                chatController.AddChat(__instance, chatMsg, false);
                            }
                        }
                        catch { }

                        try
                        {
                            if (DestroyableSingleton<HudManager>.Instance != null)
                            {
                                DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage("<color=#ffff00>Custom Impostor/Speed values were reset. You can set them again.</color>");
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        return !CheatToggles.noOptionsLimits;
    }
}

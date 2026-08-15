using HarmonyLib;
using UnityEngine;

namespace TenkaiMenu;

public static class ImmortalityPatches
{
    // Virtual vent identifier
    private const int VirtualVentKey = 0x1F3F; 
    private static bool _isProtectionActive;

    public static bool Enabled
    {
        get => _isProtectionActive;
        set => ToggleProtection(value);
    }

    private static void ToggleProtection(bool state)
    {
        if (_isProtectionActive == state) return;
        _isProtectionActive = state;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.inVent) return;

        DispatchVentSignal(state ? VentilationSystem.Operation.Enter : VentilationSystem.Operation.Exit);
    }

    private static void DispatchVentSignal(VentilationSystem.Operation op)
    {
        VentilationSystem.Update(op, VirtualVentKey);
    }

    // --- Harmony Hooks ---

    [HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Update))]
    private static class VentOperationFilter
    {
        private static bool Prefix(VentilationSystem.Operation op, int ventId)
        {
            if (!_isProtectionActive || ventId == VirtualVentKey) 
                return true;

            bool isMovementOp = op is VentilationSystem.Operation.Enter or VentilationSystem.Operation.Exit or VentilationSystem.Operation.Move;
            return !isMovementOp;
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
    private static class SessionStartHandler
    {
        private static void Postfix()
        {
            if (_isProtectionActive)
            {
                DispatchVentSignal(VentilationSystem.Operation.Enter);
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    private static class PostMeetingHandler
    {
        private static void Postfix()
        {
            if (!_isProtectionActive) return;
            
            bool isAlive = PlayerControl.LocalPlayer?.Data?.IsDead == false;
            if (isAlive)
            {
                DispatchVentSignal(VentilationSystem.Operation.Enter);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    private static class MurderAttemptNotifier
    {
        private static void Postfix(PlayerControl __instance, PlayerControl target)
        {
            if (!_isProtectionActive || target != PlayerControl.LocalPlayer) return;

            string attackerName = __instance?.Data?.PlayerName ?? "An imposter";
            HudManager.Instance?.Notifier?.AddDisconnectMessage($"[Warning] {attackerName} attempted to kill you!");
        }
    }
}

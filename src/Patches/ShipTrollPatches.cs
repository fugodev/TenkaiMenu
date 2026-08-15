using HarmonyLib;
using System;
using UnityEngine;

namespace TenkaiMenu;

public static class ShipTrollPatches
{
    [HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Deserialize))]
    public static class DisableVents
    {
        public static bool Enabled { get; set; }

        private static void Postfix(VentilationSystem __instance)
        {
            if (!Enabled || __instance == null || ShipStatus.Instance == null) return;

            try
            {
                var allPlayers = PlayerControl.AllPlayerControls;
                var allVents = ShipStatus.Instance.AllVents;

                if (allPlayers == null || allVents == null) return;
                if (__instance.PlayersInsideVents.Count >= allPlayers.Count) return;

                foreach (byte ventId in __instance.PlayersInsideVents.Values)
                {
                    if (ventId >= allVents.Count) continue;

                    VentilationSystem.Update(VentilationSystem.Operation.StartCleaning, ventId);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"DisableVents patch error: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.Deserialize))]
    public static class BlockSabotages
    {
        public static bool Enabled { get; set; }

        private static void Postfix(SabotageSystemType __instance)
        {
            if (!Enabled || __instance == null || ShipStatus.Instance == null) return;

            try
            {
                if (__instance.Timer <= 0.1f)
                {
                    // 255 clears active sabotage state across the ship
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, 255);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"BlockSabotages patch error: {ex.Message}");
            }
        }
    }
}
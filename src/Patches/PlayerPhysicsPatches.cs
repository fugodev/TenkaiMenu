using System;
using HarmonyLib;
using UnityEngine;
using System.Linq;

namespace TenkaiMenu;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
public static class PlayerPhysics_LateUpdate
{
    public static void Postfix(PlayerPhysics __instance)
    {
        TenkaiESP.PlayerNametags(__instance);
        TenkaiESP.SeeGhostsCheat(__instance);

        TenkaiCheats.NoClipCheat();
        TenkaiCheats.ProtectCheat();
        TenkaiCheats.KillAllCheat();
        TenkaiCheats.KillAllCrewCheat();
        TenkaiCheats.KillAllImpsCheat();
        TenkaiCheats.SpamTpImpsCheat();
        TenkaiCheats.SpamTpAllCheat();
        TenkaiCheats.DestroyInGameCheat();
        TenkaiCheats.LevelFarmCheat();
        TenkaiCheats.ForceStartGameCheat();
        TenkaiCheats.TeleportCursorCheat();
        TenkaiCheats.SpeedHackCheat();
        TenkaiCheats.InvisibilityCheat();
        
        TenkaiCheats.DoorHallucinationAllCheat();
        TenkaiCheats.UpdateFollowSelected();
        TenkaiCheats.AutoKillCheat();
        MoveWithMouse.HandleMoveWithMouse();
        TenkaiCheats.CompleteMyTasksCheat();
        TenkaiCheats.PlayAnimationCheat();
        TenkaiCheats.PlayScannerCheat();

        TenkaiPPMCheats.EjectPlayerPPM();
        TenkaiPPMCheats.SpectatePPM();
        TenkaiPPMCheats.KillPlayerPPM();
        TenkaiPPMCheats.TelekillPlayerPPM();
        TenkaiPPMCheats.TeleportPlayerPPM();
        TenkaiPPMCheats.VoteKickPPM();
        TenkaiPPMCheats.CopyOutfitPPM();
        TenkaiPPMCheats.CopyLevelPPM();
        TenkaiPPMCheats.SetFakeRolePPM();
        TenkaiPPMCheats.SetFakeAlivePPM();

        TracersHandler.DrawPlayerTracer(__instance);

        GameObject[] bodyObjects = GameObject.FindGameObjectsWithTag("DeadBody");
        foreach(GameObject bodyObject in bodyObjects) // Finds and loops through all dead bodies
        {
            DeadBody deadBody = bodyObject.GetComponent<DeadBody>();

            if (!deadBody || deadBody.Reported) continue;  // Only draw tracers for unreported dead bodies
            TracersHandler.DrawBodyTracer(deadBody);
        }

        try
        {
            if (CheatToggles.invertControls)
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
            else
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
        } catch (NullReferenceException) { }

        // Cleanup: if a targeted player disconnects, reset related toggles
        try
        {
            // Whisper target cleanup
            var wt = ChatMimic_RpcSendChat_Patch.whisperTarget;
            if (wt != null)
            {
                bool found = PlayerControl.AllPlayerControls.ToArray().Any(p => p == wt && p != null && p.Data != null && !p.Data.Disconnected);
                if (!found)
                {
                    ChatMimic_RpcSendChat_Patch.whisperTarget = null;
                }
            }

            // Follow selected cleanup (extra safety; UpdateFollowSelected also handles this)
            if (CheatToggles.followSelectedPlayer && CheatToggles.followSelectedPlayerId >= 0)
            {
                bool found = PlayerControl.AllPlayerControls.ToArray().Any(p => p != null && p.Data != null && !p.Data.Disconnected && p.PlayerId == CheatToggles.followSelectedPlayerId);
                if (!found)
                {
                    CheatToggles.followSelectedPlayer = false;
                    CheatToggles.followSelectedPlayerId = -1;
                }
            }

            // Destroy in-game cleanup (extra safety; DestroyInGameCheat also handles this)
            if (CheatToggles.destroyInGame && CheatToggles.destroyInGamePlayerId >= 0)
            {
                bool found = PlayerControl.AllPlayerControls.ToArray().Any(p => p != null && p.Data != null && !p.Data.Disconnected && p.PlayerId == CheatToggles.destroyInGamePlayerId);
                if (!found)
                {
                    CheatToggles.destroyInGame = false;
                    CheatToggles.destroyInGamePlayerId = -1;
                }
            }
        }
        catch { }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
public static class PlayerPhysics_HandleAnimation
{
    // Prefix patch of PlayerPhysics.HandleAnimation to disable walking animation
    public static bool Prefix(PlayerPhysics __instance)
    {
        if (CheatToggles.moonWalk && __instance.AmOwner)
        {
            __instance.ResetAnimState();

            return false;
        }

        return true;
    }
}

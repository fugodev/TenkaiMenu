using System;
using System.Collections.Generic;
using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;

namespace TenkaiMenu
{
    public static class AnticheatGuard
    {
        public static bool IsEnabled { get; set; } = true;
        public static bool CheckPlatformSpoofing { get; set; } = true;
        public static bool SendVisualAlerts { get; set; } = true;
        public static bool DropMaliciousPackets { get; set; } = true;
        
        public enum PenaltyMode
        {
            None,
            Kick,
            ForceDisconnect,
            Ban
        }

        public static PenaltyMode CurrentPenalty = PenaltyMode.None;

        private const string AlertPrefix = "<color=#00FFCC><b>[TenkaiMenu Security]</b></color>";

        #region Harmony Network Patches

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        private static class PatchPlayerControlRpc
        {
            private static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
            {
                return ValidateRpc(typeof(PlayerControl), __instance, (RpcCalls)callId, reader);
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
        private static class PatchPlayerPhysicsRpc
        {
            private static bool Prefix(PlayerPhysics __instance, byte callId, MessageReader reader)
            {
                return ValidateRpc(typeof(PlayerPhysics), __instance.myPlayer, (RpcCalls)callId, reader);
            }
        }

        [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
        private static class PatchNetworkTransformRpc
        {
            private static bool Prefix(CustomNetworkTransform __instance, byte callId, MessageReader reader)
            {
                return ValidateRpc(typeof(CustomNetworkTransform), __instance.myPlayer, (RpcCalls)callId, reader);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
        private static class PatchShipStatusRpc
        {
            private static bool Prefix(byte callId, MessageReader reader)
            {
                return ValidateRpc(typeof(ShipStatus), null, (RpcCalls)callId, reader);
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleGameData))]
        private static class PatchGameData
        {
            private static bool Prefix(InnerNetClient __instance, MessageReader parentReader)
            {
                try
                {
                    while (parentReader.BytesRemaining > 0)
                    {
                        MessageReader subReader = parentReader.ReadMessageAsNewBuffer();
                        ProcessIncomingGameData(__instance, subReader, ++__instance.msgNum);
                    }
                }
                finally
                {
                    parentReader.Recycle();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
        private static class PatchPlayerStart
        {
            private static void Postfix(PlayerControl __instance)
            {
                if (!IsEnabled || !CheckPlatformSpoofing) return;

                ClientData client = AmongUsClient.Instance.GetClientFromCharacter(__instance);
                if (client?.PlatformData == null) return;

                if (!IsPlatformDataValid(client.PlatformData))
                {
                    TriggerViolation(__instance, $"Platform spoof detected for {client.PlayerName} ({client.PlatformData.Platform}).");
                }
            }
        }

        #endregion

        #region Core Logic & Validation Pipelines

        private static void ProcessIncomingGameData(InnerNetClient client, MessageReader reader, int sequence)
        {
            GameDataTypes dataType = (GameDataTypes)reader.Tag;
            bool shouldProcess = true;

            if (IsEnabled && dataType == GameDataTypes.ReadyFlag)
            {
                int bufferPos = reader.Position;
                int targetClientId = reader.ReadPackedInt32();
                ClientData targetClient = AmongUsClient.Instance.FindClientById(targetClientId);

                if (targetClient == null)
                {
                    TriggerViolation($"Discarded ReadyFlag from non-existent client ID: {targetClientId}.");
                    shouldProcess = false;
                }
                else if (targetClient.IsReady)
                {
                    TriggerViolation(targetClient.Character, $"Duplicate ReadyFlag received from {targetClient.Character.Data.PlayerName}.");
                    shouldProcess = false;
                }

                reader.Position = bufferPos;
            }

            if (!shouldProcess && DropMaliciousPackets)
            {
                reader.Recycle();
                return;
            }

            client.StartCoroutine(client.HandleGameDataInner(reader, sequence));
        }

        private static bool ValidateRpc(Type targetType, PlayerControl sender, RpcCalls call, MessageReader reader)
        {
            if (!IsEnabled) return true;

            int initialPos = reader.Position;
            bool isInvalid = false;

            switch (call)
            {
                case RpcCalls.PlayAnimation:
                    isInvalid = !ValidateAnimation(sender, reader);
                    break;
                case RpcCalls.CompleteTask:
                    isInvalid = !ValidateTaskCompletion(sender, reader);
                    break;
                case RpcCalls.Exiled:
                    TriggerViolation(sender, $"{sender?.Data?.PlayerName} attempted unauthorized Exiled RPC execution.");
                    isInvalid = true;
                    break;
                case RpcCalls.CheckName:
                case RpcCalls.SetName:
                    isInvalid = !ValidateNameChange(sender, reader, call);
                    break;
                case RpcCalls.SetColor:
                    isInvalid = !ValidateColorChange(sender, reader);
                    break;
                case RpcCalls.ReportDeadBody:
                    if (GameManager.Instance.IsHideAndSeek())
                    {
                        TriggerViolation(sender, $"Illegal body report by {sender?.Data?.PlayerName} in Hide & Seek mode.");
                        isInvalid = true;
                    }
                    break;
                case RpcCalls.SetScanner:
                    isInvalid = !ValidateMedicalScanner(sender, reader);
                    break;
                case RpcCalls.SetStartCounter:
                    isInvalid = !ValidateStartCounter(sender, reader);
                    break;
                case RpcCalls.EnterVent:
                case RpcCalls.ExitVent:
                    isInvalid = !ValidateVentAction(sender, call == RpcCalls.EnterVent);
                    break;
                case RpcCalls.SnapTo:
                    if (LobbyBehaviour.Instance != null)
                    {
                        TriggerViolation(sender, $"Position snap exploit flagged for {sender?.Data?.PlayerName} inside lobby.");
                        isInvalid = true;
                        if (AmongUsClient.Instance.AmHost && !IsModdedClientPresent())
                        {
                            sender.NetTransform.RpcSnapTo(sender.transform.position);
                        }
                    }
                    break;
                case RpcCalls.AddVote:
                    int sourceId = reader.ReadPackedInt32();
                    if (AmongUsClient.Instance.FindClientById(sourceId) == null)
                    {
                        TriggerViolation(sender, $"Malformed vote request originating from client ID {sourceId}.");
                        isInvalid = true;
                    }
                    break;
                case RpcCalls.CloseDoorsOfType:
                    if (GameManager.Instance.IsHideAndSeek())
                    {
                        TriggerViolation("Door operation blocked during Hide & Seek.");
                        isInvalid = true;
                    }
                    break;
                case RpcCalls.UsePlatform:
                    isInvalid = !ValidatePlatformUsage(sender);
                    break;
                case RpcCalls.UpdateSystem:
                    isInvalid = !ValidateSystemUpdate(sender, reader);
                    break;
                case RpcCalls.SetLevel:
                    isInvalid = !ValidateLevelAssignment(sender, reader);
                    break;
            }

            reader.Position = initialPos;
            return !isInvalid || !DropMaliciousPackets;
        }

        #endregion

        #region Individual RPC Handlers

        private static bool ValidateAnimation(PlayerControl player, MessageReader reader)
        {
            TaskTypes animationType = (TaskTypes)reader.ReadByte();
            if (LobbyBehaviour.Instance)
            {
                TriggerViolation(player, $"Lobby animation trigger detected ({animationType}) from {player.Data.PlayerName}.");
                return false;
            }
            if (RoleManager.IsImpostorRole(player.Data.RoleType))
            {
                TriggerViolation(player, $"Impostor visual animation exploit blocked ({animationType}) for {player.Data.PlayerName}.");
                return false;
            }
            if (!GameManager.Instance.LogicOptions.GetVisualTasks())
            {
                TriggerViolation(player, $"Disabled visual task execution ({animationType}) blocked for {player.Data.PlayerName}.");
                return false;
            }
            return true;
        }

        private static bool ValidateTaskCompletion(PlayerControl player, MessageReader reader)
        {
            uint taskIndex = reader.ReadPackedUInt32();
            if (ShipStatus.Instance == null)
            {
                TriggerViolation(player, $"Task completion ({taskIndex}) failed: No active ShipStatus context.");
                return false;
            }
            if (RoleManager.IsImpostorRole(player.Data.RoleType))
            {
                TriggerViolation(player, $"Impostor task completion exploit flagged for {player.Data.PlayerName}.");
                return false;
            }
            if (taskIndex >= player.Data.Tasks.Count)
            {
                TriggerViolation(player, $"Invalid task index {taskIndex} completed by {player.Data.PlayerName} (Total: {player.Data.Tasks.Count}).");
                return false;
            }
            return true;
        }

        private static bool ValidateNameChange(PlayerControl player, MessageReader reader, RpcCalls call)
        {
            if (call == RpcCalls.SetName)
            {
                uint netId = reader.ReadUInt32();
                uint expectedId = IsModdedClientPresent() ? player.NetId : player.Data.NetId;
                if (netId != expectedId)
                {
                    TriggerViolation(player, $"Spoofed Network ID on name update for {player.Data.PlayerName}.");
                    return false;
                }
            }

            string targetName = reader.ReadString();
            int maxLength = call == RpcCalls.CheckName ? 10 : 12;

            if (targetName.Length > maxLength || targetName.Contains("<"))
            {
                TriggerViolation(player, $"Illegal character sequence or name length violation: '{targetName}'.");
                return false;
            }
            return true;
        }

        private static bool ValidateColorChange(PlayerControl player, MessageReader reader)
        {
            uint netId = reader.ReadUInt32();
            byte colorId = reader.ReadByte();

            if (netId != player.Data.NetId || colorId >= Palette.ColorNames.Length)
            {
                TriggerViolation(player, $"Invalid color packet sent by {player.Data.PlayerName}.", false);
                player.SetColor((byte)CrewmateColor.Red);
                return false;
            }
            return true;
        }

        private static bool ValidateMedicalScanner(PlayerControl player, MessageReader reader)
        {
            bool isStarting = reader.ReadBoolean();
            if (!isStarting) return true;

            if (ShipStatus.Instance == null || RoleManager.IsImpostorRole(player.Data.RoleType) || !GameManager.Instance.LogicOptions.GetVisualTasks())
            {
                TriggerViolation(player, $"Unauthorized Medbay scan state requested by {player.Data.PlayerName}.");
                return false;
            }

            bool hasScanTask = false;
            foreach (var task in player.Data.Tasks)
            {
                if (task.Id == (byte)TaskTypes.SubmitScan)
                {
                    hasScanTask = true;
                    break;
                }
            }

            if (!hasScanTask)
            {
                TriggerViolation(player, $"Scan request rejected: {player.Data.PlayerName} has no Medbay Scan assignment.");
                return false;
            }
            return true;
        }

        private static bool ValidateStartCounter(PlayerControl player, MessageReader reader)
        {
            reader.ReadPackedInt32();
            sbyte duration = reader.ReadSByte();

            if (player.OwnerId != AmongUsClient.Instance.HostId && duration != -1)
            {
                TriggerViolation(player, $"Unauthorized lobby start timer modification by {player.Data.PlayerName}.");
                if (AmongUsClient.Instance.AmHost)
                {
                    PlayerControl.LocalPlayer.RpcSetStartCounter(-1);
                }
                return false;
            }
            return true;
        }

        private static bool ValidateVentAction(PlayerControl player, bool isEntering)
        {
            if (ShipStatus.Instance == null)
            {
                TriggerViolation(player, $"Vent event rejected: Active map instance not found.");
                return false;
            }
            if (!player.Data.IsDead && !player.Data.Role.CanVent)
            {
                TriggerViolation(player, $"Role '{player.Data.RoleType}' for {player.Data.PlayerName} is not permitted to use vents.");
                return false;
            }
            if (GameManager.Instance.IsHideAndSeek() && RoleManager.IsImpostorRole(player.Data.RoleType))
            {
                TriggerViolation(player, $"Impostor vent usage in Hide & Seek mode blocked for {player.Data.PlayerName}.");
                return false;
            }
            return true;
        }

        private static bool ValidatePlatformUsage(PlayerControl player)
        {
            if ((MapNames)Utils.GetCurrentMapID() != MapNames.Airship || ShipStatus.Instance == null || GameManager.Instance.IsHideAndSeek())
            {
                TriggerViolation(player, $"Illegal platform movement event received from {player.Data.PlayerName}.");
                return false;
            }
            return true;
        }

        private static bool ValidateSystemUpdate(PlayerControl player, MessageReader reader)
        {
            SystemTypes targetSystem = (SystemTypes)reader.ReadByte();
            PlayerControl targetPlayer = reader.ReadNetObject<PlayerControl>();

            if (!ShipStatus.Instance.Systems.ContainsKey(targetSystem))
            {
                TriggerViolation(targetPlayer, $"System update targeting unavailable system {targetSystem}.");
                return false;
            }

            if (targetPlayer.Data.IsDead && targetSystem != SystemTypes.MedBay && targetSystem != SystemTypes.Sabotage &&
                targetSystem != SystemTypes.Security && targetSystem != SystemTypes.Ventilation)
            {
                TriggerViolation(targetPlayer, $"Dead player attempted prohibited system interaction on {targetSystem}.");
                return false;
            }

            if (targetSystem == SystemTypes.Sabotage)
            {
                SystemTypes sabotageType = (SystemTypes)reader.ReadByte();
                if (!RoleManager.IsImpostorRole(targetPlayer.Data.RoleType) || GameManager.Instance.IsHideAndSeek() || !IsValidSabotageType(sabotageType))
                {
                    TriggerViolation(targetPlayer, $"Illegal sabotage request for system {sabotageType}.");
                    return false;
                }
            }
            else if (targetSystem == SystemTypes.Electrical)
            {
                byte switchState = reader.ReadByte();
                if ((switchState & 128) != 0 || switchState > 5 || MeetingHud.Instance)
                {
                    TriggerViolation(targetPlayer, $"Prohibited electrical switch update from {targetPlayer.Data.PlayerName}.");
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateLevelAssignment(PlayerControl player, MessageReader reader)
        {
            int requestedLevel = reader.ReadPackedInt32();
            if (requestedLevel > 100000)
            {
                TriggerViolation(player, $"Out-of-range level packet ({requestedLevel}) from {player.Data.PlayerName}.");
                return false;
            }

            if (player != null && player != PlayerControl.LocalPlayer && AmongUsClient.Instance.AmHost)
            {
                int trueLevel = requestedLevel + 1;
                if (CheatToggles.anticheatEnabled && CheatToggles.anticheatDetectPlayerLevels && trueLevel > CheatToggles.anticheatDetectPlayerLevelAbove)
                {
                    TriggerViolation(player, $"Player level {trueLevel} exceeded monitoring threshold.", false);
                }

                if (CheatToggles.anticheatEnabled && CheatToggles.anticheatKickPlayerLevels && trueLevel <= CheatToggles.anticheatKickPlayerLevelBelow)
                {
                    TriggerViolation($"{player.Data.PlayerName} auto-kicked (Level {trueLevel} below minimum requirement).");
                    ExecutePunishment(player);
                }
            }
            return true;
        }

        #endregion

        #region Helper Utilities

        public static void TriggerViolation(PlayerControl player, string logDetails, bool executePenalty = true)
        {
            if (player == PlayerControl.LocalPlayer) return;

            if (SendVisualAlerts)
            {
                DispatchNotification(logDetails);
            }

            if (AmongUsClient.Instance.AmHost && executePenalty)
            {
                ExecutePunishment(player);
            }
        }

        public static void TriggerViolation(string logDetails)
        {
            if (SendVisualAlerts)
            {
                DispatchNotification(logDetails);
            }
        }

        private static void DispatchNotification(string message)
        {
            if (HudManager.Instance?.Notifier != null)
            {
                HudManager.Instance.Notifier.AddDisconnectMessage($"{AlertPrefix} {message}");
            }
            else
            {
                Debug.Log($"[TenkaiMenu Security] {message}");
            }
        }

        private static void ExecutePunishment(PlayerControl player)
        {
            switch (CurrentPenalty)
            {
                case PenaltyMode.Kick:
                    AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
                    break;
                case PenaltyMode.ForceDisconnect:
                    if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                    {
                        AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
                    }
                    else
                    {
                        AmongUsClient.Instance.SendLateRejection(player.OwnerId, DisconnectReasons.ClientTimeout);
                    }
                    break;
                case PenaltyMode.Ban:
                    AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
                    break;
            }
        }

        public static bool IsModdedClientPresent()
        {
            if (Constants.IsVersionModded() || PlayerControl.LocalPlayer?.Data == null) return false;
            return PlayerControl.LocalPlayer.Data.OwnerId != AmongUsClient.Instance.HostId;
        }

        public static bool IsValidSabotageType(SystemTypes type)
        {
            return type == SystemTypes.Electrical || type == SystemTypes.LifeSupp || type == SystemTypes.Comms ||
                   type == SystemTypes.Reactor || type == SystemTypes.Laboratory || type == SystemTypes.HeliSabotage ||
                   type == SystemTypes.MushroomMixupSabotage || type == SystemTypes.Sabotage;
        }

        public static bool IsPlatformDataValid(PlatformSpecificData data)
        {
            string name = data.PlatformName;
            ulong xuid = data.XboxPlatformId;
            ulong psid = data.PsnPlatformId;

            switch (data.Platform)
            {
                case Platforms.StandaloneEpicPC:
                case Platforms.StandaloneSteamPC:
                case Platforms.StandaloneMac:
                case Platforms.StandaloneItch:
                case Platforms.IPhone:
                case Platforms.Android:
                    return name == "TESTNAME" && xuid == 0 && psid == 0;
                case Platforms.StandaloneWin10:
                    return name == "TESTNAME" && xuid != 0 && psid == 0;
                case Platforms.Xbox:
                    return name != "TESTNAME" && name.Length >= 3 && name.Length <= 16 && xuid != 0 && psid == 0;
                case Platforms.Playstation:
                    return name != "TESTNAME" && xuid == 0 && psid != 0;
                case Platforms.Switch:
                    return name != "TESTNAME" && xuid == 0 && psid == 0;
                case (Platforms)255:
                    return AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
                default:
                    return false;
            }
        }

        #endregion
    }
}

using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using InnerNet;

namespace TenkaiMenu;

public static class HostCheats
{
    private static bool isSkeldFlipped;

    public static void SetReversedSkeld(bool enabled)
    {
        if (AmongUsClient.Instance == null || AmongUsClient.Instance.ShipPrefabs == null) return;
        if (AmongUsClient.Instance.ShipPrefabs.Count <= 3) return;

        // Prevent re-executing if already in target state
        if (isSkeldFlipped == enabled) return;

        try
        {
            // Swap Index 0 (Skeld) and Index 3 (dlekS ehT)
            AssetReference normalSkeld = AmongUsClient.Instance.ShipPrefabs[0];
            AmongUsClient.Instance.ShipPrefabs[0] = AmongUsClient.Instance.ShipPrefabs[3];
            AmongUsClient.Instance.ShipPrefabs[3] = normalSkeld;

            isSkeldFlipped = enabled;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TenkaiMenu] Failed to toggle Reversed Skeld: {ex}");
        }
    }

    // Optional helper to reset state on lobby disconnect/leave
    public static void ResetReversedSkeldState()
    {
        if (isSkeldFlipped)
        {
            SetReversedSkeld(false);
        }
    }

    public static void EndGameImmediately()
    {
        try
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
            if (GameManager.Instance == null) return;
            GameManager.Instance.RpcEndGame((GameOverReason)1, false);
        }
        catch { }
    }

    public static void DestroyLobby()
    {
        try
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

            LobbyBehaviour lobby = LobbyBehaviour.Instance;
            if (lobby != null)
            {
                lobby.Despawn();
                LobbyBehaviour.Instance = null;
            }
        }
        catch { }
    }

    public static void RecreateLobby()
    {
        try
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

            if (LobbyBehaviour.Instance != null)
            {
                Debug.LogWarning("Lobby already exists");
                return;
            }

            GameStartManager gsm = DestroyableSingleton<GameStartManager>.Instance;
            if (gsm != null && gsm.LobbyPrefab != null)
            {
                LobbyBehaviour.Instance = UnityEngine.Object.Instantiate(gsm.LobbyPrefab);
                AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance, -2, 0);
            }
        }
        catch { }
    }

    public static void SetLobbyEngineGlitch(bool enabled)
    {
        try
        {
            TenkaiMenu.spoofPlatform.Value = enabled ? "PlayStation" : "";
            TenkaiMenu.spoofPlatform.ConfigFile.Save();
        }
        catch { }
    }

    public static void IncreaseImpostors(int amount = 1)
    {
        if (!CheatToggles.noOptionsLimits)
        {
            CheatToggles.noOptionsLimits = true;
        }
        var opt = GameOptionsManager.Instance?.currentNormalGameOptions;
        if (opt != null) opt.NumImpostors += amount;
    }

    public static void DecreaseImpostors(int amount = 1)
    {
        if (!CheatToggles.noOptionsLimits)
        {
            CheatToggles.noOptionsLimits = true;
        }
        var opt = GameOptionsManager.Instance?.currentNormalGameOptions;
        if (opt != null) opt.NumImpostors -= amount;
    }

    public static void IncreasePlayerSpeed(float delta = 0.25f)
    {
        if (!CheatToggles.noOptionsLimits)
        {
            CheatToggles.noOptionsLimits = true;
        }
        var opt = GameOptionsManager.Instance?.currentNormalGameOptions;
        if (opt != null) opt.PlayerSpeedMod += delta;
    }

    public static void DecreasePlayerSpeed(float delta = 0.25f)
    {
        if (!CheatToggles.noOptionsLimits)
        {
            CheatToggles.noOptionsLimits = true;
        }
        var opt = GameOptionsManager.Instance?.currentNormalGameOptions;
        if (opt != null) opt.PlayerSpeedMod -= delta;
    }

    public static int lobbyColorId;
    public static bool lobbyDiscoMode;
    private static float _lastLobbyColorUpdate;

    public static void ForceImpostor()
    {
        if (!Utils.isHost) return;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null) continue;
            DestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Impostor);
        }
    }

    public static readonly Dictionary<byte, RoleTypes> RoleAssignments = new();

    public static RoleTypes[] GetAssignableRoles()
    {
        var manager = RoleManager.Instance;
        if (manager == null || manager.AllRoles == null)
        {
            return Array.Empty<RoleTypes>();
        }

        return manager.AllRoles.ToArray()
            .Where(role => role != null)
            .Select(role => role.Role)
            .Distinct()
            .OrderBy(roleType => (int)roleType)
            .ToArray();
    }

    public static void SetRoleAssignment(byte playerId, RoleTypes role)
    {
        if (RoleAssignments.ContainsKey(playerId))
        {
            RoleAssignments[playerId] = role;
        }
        else
        {
            RoleAssignments.Add(playerId, role);
        }
    }

    public static void RemoveRoleAssignment(byte playerId)
    {
        RoleAssignments.Remove(playerId);
    }

    public static void ClearRoleAssignments()
    {
        RoleAssignments.Clear();

        if (CheatToggles.alwaysImpostor && PlayerControl.LocalPlayer != null)
        {
            RoleAssignments[PlayerControl.LocalPlayer.PlayerId] = RoleTypes.Impostor;
        }
    }

    public static void SetAlwaysImpostor(bool enabled)
    {
        if (!Utils.isHost || PlayerControl.LocalPlayer == null)
        {
            return;
        }

        byte hostId = PlayerControl.LocalPlayer.PlayerId;
        if (enabled)
        {
            SetRoleAssignment(hostId, RoleTypes.Impostor);
        }
        else
        {
            RemoveRoleAssignment(hostId);
        }
    }

    public static void EnsureForcedRoleAssignments()
    {
        if (!Utils.isHost || PlayerControl.LocalPlayer == null)
        {
            return;
        }

        if (CheatToggles.alwaysImpostor)
        {
            SetRoleAssignment(PlayerControl.LocalPlayer.PlayerId, RoleTypes.Impostor);
        }
    }

    public static bool HasRoleAssignment(byte playerId)
    {
        return RoleAssignments.ContainsKey(playerId);
    }

    [HarmonyPatch(typeof(LogicRoleSelectionNormal), nameof(LogicRoleSelectionNormal.AssignRolesFromList))]
    public static class RoleDistributionInterceptor
    {
        private static void Prefix(ref Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> players,
            ref Il2CppSystem.Collections.Generic.List<RoleTypes> roleList, ref int rolesAssigned)
        {
            if (!Utils.isHost || players == null || roleList == null)
            {
                return;
            }

            EnsureForcedRoleAssignments();

            if (RoleAssignments.Count == 0)
            {
                return;
            }

            foreach (var assignment in RoleAssignments.ToArray())
            {
                byte targetId = assignment.Key;
                RoleTypes targetRole = assignment.Value;

                PlayerControl targetPlayer = PlayerControl.AllPlayerControls.ToArray()
                    .FirstOrDefault(player => player != null && player.PlayerId == targetId);

                if (targetPlayer == null)
                {
                    continue;
                }

                Il2CppSystem.Predicate<NetworkedPlayerInfo> playerPredicate = (Il2CppSystem.Predicate<NetworkedPlayerInfo>)(player => player != null && player.PlayerId == targetId);
                int playerIndex = players.FindIndex(playerPredicate);
                if (playerIndex >= 0)
                {
                    players.RemoveAt(playerIndex);
                }

                Il2CppSystem.Predicate<RoleTypes> rolePredicate = (Il2CppSystem.Predicate<RoleTypes>)(roleType => roleType == targetRole);
                int roleIndex = roleList.FindIndex(rolePredicate);
                if (roleIndex >= 0)
                {
                    roleList.RemoveAt(roleIndex);
                }

                if (RoleManager.IsGhostRole(targetRole) && players.Count == 0)
                {
                    RoleTypes fallbackRole = RoleManager.IsImpostorRole(targetRole) ? RoleTypes.Impostor : RoleTypes.Crewmate;
                    targetPlayer.RpcSetRole(fallbackRole);
                }

                targetPlayer.RpcSetRole(targetRole);
                rolesAssigned++;
            }
        }
    }

    public static void SetEveryoneSameColor()
    {
        if (!Utils.isHost || PlayerControl.LocalPlayer == null) return;

        lobbyColorId = PlayerControl.LocalPlayer.CurrentOutfit.ColorId;
        SetLobbyColorForEveryone(lobbyColorId);
    }

    public static void CycleLobbyColor()
    {
        if (!Utils.isHost) return;

        int maxColors = Palette.PlayerColors.Length;
        if (maxColors <= 0) return;

        lobbyColorId = (lobbyColorId + 1) % maxColors;
        SetLobbyColorForEveryone(lobbyColorId);
    }

    public static void SetLobbyColorForEveryone(int colorId)
    {
        if (!Utils.isHost) return;

        int maxColors = Palette.PlayerColors.Length;
        if (maxColors <= 0) return;

        lobbyColorId = ((colorId % maxColors) + maxColors) % maxColors;
        var colorValue = Palette.PlayerColors[lobbyColorId];

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null) continue;

            try
            {
                player.CurrentOutfit.ColorId = lobbyColorId;
            }
            catch { }

            try
            {
                var data = player.Data;
                if (data != null)
                {
                    var prop = data.GetType().GetProperty("Color");
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(data, colorValue);
                    }
                }
            }
            catch { }

            TryInvokePlayerColorRpc(player, lobbyColorId);
        }
    }

    public static void ApplyCustomSeekers()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || PlayerControl.LocalPlayer == null) return;
        if (!Utils.isHideNSeek) return;

        int seekersCount = CheatToggles.seekersCount;
        if (seekersCount <= 1) return;

        int currentSeekers = 0;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.IsDead) continue;
            try
            {
                if (player.Data.Role != null && player.Data.Role.TeamType == RoleTeamTypes.Impostor)
                {
                    currentSeekers++;
                }
            }
            catch { }
        }

        int neededSeekers = Mathf.Max(0, seekersCount - currentSeekers);
        if (neededSeekers <= 0) return;

        var candidates = new List<PlayerControl>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.IsDead) continue;
            bool isSeeker = false;
            try
            {
                if (player.Data.Role != null && player.Data.Role.TeamType == RoleTeamTypes.Impostor)
                {
                    isSeeker = true;
                }
            }
            catch { }

            if (!isSeeker)
            {
                candidates.Add(player);
            }
        }

        if (candidates.Count == 0) return;

        var random = new System.Random();
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int index = random.Next(i + 1);
            var temp = candidates[i];
            candidates[i] = candidates[index];
            candidates[index] = temp;
        }

        int assignCount = Mathf.Min(neededSeekers, candidates.Count);
        for (int i = 0; i < assignCount; i++)
        {
            var seeker = candidates[i];
            if (seeker != null && seeker.Data != null && !seeker.Data.IsDead)
            {
                seeker.RpcSetRole(RoleTypes.Impostor);
            }
        }
    }

    public static void KickOrBan(PlayerControl player, bool ban)
    {
        if (!Utils.isHost || player == null || player.AmOwner) return;
        int clientId = Utils.getClientIdByPlayer(player);
        if (clientId < 0) return;

        try
        {
            AmongUsClient.Instance.KickPlayer(clientId, ban);
        }
        catch { }
    }

    public static void EjectPlayer(PlayerControl player)
    {
        if (player == null || player.Data == null) return;
        var hud = DestroyableSingleton<HudManager>.Instance;
        if (hud == null) return;

        if (MeetingHud.Instance == null)
        {
            MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(hud.MeetingPrefab);
            AmongUsClient.Instance.Spawn(MeetingHud.Instance, -2, 0);
        }

        if (MeetingHud.Instance != null)
        {
            MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();
            MeetingHud.Instance.RpcVotingComplete(votes, player.Data, false);
            MeetingHud.Instance.RpcClose();
        }
    }

    public static void ForceMeeting(PlayerControl target)
    {
        if (target == null)
        {
            target = PlayerControl.LocalPlayer;
        }

        // Defensive checks
        if (target == null) return;
        if (PlayerControl.LocalPlayer == null) return;
        if (target.Data == null || target.Data.Disconnected) return;

        if (Utils.isHost)
        {
            if (MeetingRoomManager.Instance == null || DestroyableSingleton<HudManager>.Instance == null)
            {
                return;
            }

            try
            {
                MeetingRoomManager.Instance.AssignSelf(target, null);
                DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(target);
                target.RpcStartMeeting(null);
            }
            catch { }
        }
        else
        {
            try
            {
                target.CmdReportDeadBody(null);
            }
            catch { }
        }
    }

    public static void SelfReport(PlayerControl target)
    {
        if (target == null)
        {
            target = PlayerControl.LocalPlayer;
        }
        // Defensive checks
        if (target == null) return;
        if (PlayerControl.LocalPlayer == null) return;
        if (target.Data == null || target.Data.Disconnected) return;

        PlayerControl random = CheatHelpers.GetRandomPlayer(target);
        if (random == null || random.Data == null || random.Data.Disconnected) return;

        if (Utils.isHost)
        {
            try
            {
                if (MeetingHud.Instance == null && MeetingRoomManager.Instance != null && DestroyableSingleton<HudManager>.Instance != null)
                {
                    MeetingRoomManager.Instance.AssignSelf(target, random.Data);
                    target.RpcStartMeeting(random.Data);
                    DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(target);
                }
                else
                {
                    target.RpcStartMeeting(random.Data);
                }
            }
            catch { }
        }
        else
        {
            try
            {
                target.CmdReportDeadBody(random.Data);
            }
            catch { }
        }
    }

    private static bool TryInvokePlayerRpc(PlayerControl player, string methodName, params object[] args)
    {
        if (player == null) return false;

        try
        {
            var method = player.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null) return false;
            method.Invoke(player, args);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryShapeshift(PlayerControl player, PlayerControl target)
    {
        if (player == null || target == null) return false;

        string[] candidates = { "RpcShapeshift", "CmdCheckShapeshift", "Shapeshift" };
        foreach (var method in candidates)
        {
            if (TryInvokePlayerRpc(player, method, target, !CheatToggles.noShapeshiftAnim))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryUnshapeshift(PlayerControl player)
    {
        if (player == null) return false;

        string[] candidates = { "RpcCheckRevertShapeshift", "CmdCheckRevertShapeshift", "RpcRevertShapeshift", "RevertShapeshift", "UnShapeshift" };
        foreach (var method in candidates)
        {
            if (TryInvokePlayerRpc(player, method))
            {
                return true;
            }
        }

        return false;
    }

    public static void FrameShapeshift(PlayerControl player)
    {
        if (player == null) return;

        PlayerControl random = CheatHelpers.GetRandomPlayer(player);
        if (random == null) return;

        TryShapeshift(player, random);
    }

    public static void FrameUnshapeshift(PlayerControl player)
    {
        if (player == null) return;
        TryUnshapeshift(player);
    }

    public static void ShapeshiftEveryoneTo(PlayerControl target)
    {
        if (target == null) return;

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null) continue;
            TryShapeshift(player, target);
        }
    }

    public static void UnshapeshiftEveryone()
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null) continue;
            TryUnshapeshift(player);
        }
    }

    public static void UpdateLobbyDisco()
    {
        if (!lobbyDiscoMode || !Utils.isHost) return;

        if (Time.time - _lastLobbyColorUpdate < 0.5f) return;

        int maxColors = Palette.PlayerColors.Length;
        if (maxColors <= 0) return;

        SetLobbyColorForEveryone(UnityEngine.Random.Range(0, maxColors));
        _lastLobbyColorUpdate = Time.time;
    }

    private static bool TryInvokePlayerColorRpc(PlayerControl player, int colorId)
    {
        if (player == null) return false;

        string[] candidates = new[]
        {
            "RpcSetColor",
            "RpcSetColorId",
            "RpcSetPlayerColor",
            "CmdSetColor",
            "RpcSetOutfit",
            "CmdSetOutfit"
        };

        foreach (var name in candidates)
        {
            try
            {
                var method = player.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null) continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 1) continue;

                var parameterType = parameters[0].ParameterType;
                object arg = parameterType == typeof(byte)
                    ? (object)(byte)colorId
                    : Convert.ChangeType(colorId, parameterType);

                method.Invoke(player, new[] { arg });
                return true;
            }
            catch { }
        }

        return false;
    }

    public static void ApplyZeroCdSetting()
    {
        try
        {
            if (!Utils.isHost) return;
            CheatToggles.noOptionsLimits = true;

            var normalOptions = GameOptionsManager.Instance?.currentNormalGameOptions;
            var currentOptions = GameOptionsManager.Instance?.CurrentGameOptions;
            if (normalOptions == null && currentOptions == null) return;

            ApplyZeroCdToOptions(normalOptions);
            if (!ReferenceEquals(normalOptions, currentOptions))
            {
                ApplyZeroCdToOptions(currentOptions);
            }
        }
        catch { }
    }

    private static void ApplyZeroCdToOptions(object options)
    {
        if (options == null) return;

        SetOption(options, new[] { "KillCooldown", "KillCooldownSeconds", "KillCd", "KillTimer" }, 0.000001f);
        SetOption(options, new[] { "ImpostorVision", "ImpostorVisionMod", "ImpostorVisionRange", "ImpostorVisionMultiplier" }, 9999f);
        SetOption(options, new[] { "CrewVision", "CrewMateVision", "CrewVisionMod", "CrewVisionRange", "CrewVisionMultiplier" }, 9999f);
        SetOption(options, new[] { "KillDistance", "KillRange", "KillDistanceSetting" }, "Medium");
        SetOption(options, new[] { "PlayerSpeedMod", "PlayerSpeedMultiplier", "PlayerSpeed" }, 2.9f);
        SetOption(options, new[] { "EmergencyMeetings", "EmergencyMeetingCount", "NumEmergencyMeetings" }, 9999);
        SetOption(options, new[] { "EmergencyCooldown", "EmergencyCooldownSeconds", "EmergencyCooldownTime" }, 0f);
        SetOption(options, new[] { "DiscussionTime", "DiscussionTimeSeconds", "DiscussionTimeMinutes" }, 0f);
        SetOption(options, new[] { "VotingTime", "VotingTimeSeconds", "VotingTimeMinutes" }, 33f);
        SetOption(options, new[] { "CommonTasks", "CommonTaskCount", "NumCommonTasks" }, 0);
        SetOption(options, new[] { "LongTasks", "LongTaskCount", "NumLongTasks" }, 0);
        SetOption(options, new[] { "ShortTasks", "ShortTaskCount", "NumShortTasks" }, 1);
        SetOption(options, new[] { "Scientist", "ScientistCount", "NumScientists" }, 0);
        SetOption(options, new[] { "GuardianAngel", "GuardianAngelCount", "NumGuardianAngels", "GuardianAngels" }, 15);
        SetOption(options, new[] { "Engineer", "EngineerCount", "NumEngineers" }, 15);
        SetOption(options, new[] { "Noisemaker", "NoisemakerCount", "NumNoisemakers" }, 1);
        SetOption(options, new[] { "Detective", "DetectiveCount", "NumDetectives" }, 1);
        SetOption(options, new[] { "Tracker", "TrackerCount", "NumTrackers" }, 0);
        SetOption(options, new[] { "Shapeshifter", "ShapeshifterCount", "NumShapeshifters" }, 0);
        SetOption(options, new[] { "Phantom", "PhantomCount", "NumPhantoms" }, 0);
        SetOption(options, new[] { "Viper", "ViperCount", "NumVipers" }, 3);
        SetOption(options, new[] { "GuardianAngelProtectionCooldown", "GuardianAngelCooldown", "GuardianProtectionCooldown" }, 0.0000000001f);
        SetOption(options, new[] { "GuardianAngelProtectionDuration", "GuardianProtectionDuration", "GuardianAngelDuration" }, 200f);
        SetOption(options, new[] { "GuardianProtectVisibleToImpostor", "GuardianAngelProtectVisibleToImpostor", "ProtectVisibleToImpostor" }, true);
        SetOption(options, new[] { "VentCooldown", "VentUseCooldown", "VentCooldownSeconds" }, 0.001f);
        SetOption(options, new[] { "MaxVentTime", "VentMaxTime", "VentDuration", "VentTime" }, 20f);
    }

    private static void SetOption(object target, string[] names, object value)
    {
        if (target == null) return;

        var type = target.GetType();
        foreach (var name in names)
        {
            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanWrite) continue;

            try
            {
                if (property.PropertyType.IsEnum)
                {
                    object enumValue = value is string stringValue
                        ? Enum.Parse(property.PropertyType, stringValue, true)
                        : Convert.ChangeType(value, Enum.GetUnderlyingType(property.PropertyType));

                    property.SetValue(target, enumValue);
                }
                else
                {
                    object converted = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(target, converted);
                }

                return;
            }
            catch { }
        }

        foreach (var name in names)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) continue;

            try
            {
                if (field.FieldType.IsEnum)
                {
                    object enumValue = value is string stringValue
                        ? Enum.Parse(field.FieldType, stringValue, true)
                        : Convert.ChangeType(value, Enum.GetUnderlyingType(field.FieldType));

                    field.SetValue(target, enumValue);
                }
                else
                {
                    object converted = Convert.ChangeType(value, field.FieldType);
                    field.SetValue(target, converted);
                }

                return;
            }
            catch { }
        }
    }

    public static bool IsVoteLockEnabled()
    {
        return CheatToggles.voteLockEnabled;
    }

    public static void SpawnMeetingHud()
    {
        try
        {
            if (MeetingHud.Instance == null && DestroyableSingleton<HudManager>.Instance != null)
            {
                MeetingHud meetingHud = UnityEngine.Object.Instantiate<MeetingHud>(DestroyableSingleton<HudManager>.Instance.MeetingPrefab);
                AmongUsClient.Instance.Spawn(meetingHud, -2, 0);
            }
        }
        catch { }
    }

    public static void CloseMeeting()
    {
        try
        {
            if (MeetingHud.Instance != null)
            {
                try
                {
                    MeetingHud.Instance.Despawn();
                }
                catch
                {
                    // Fallback to Close if Despawn is unavailable for any reason
                    MeetingHud.Instance.Close();
                    MeetingHud.Instance.RpcClose();
                }
            }
        }
        catch { }
    }
}

[HarmonyPatch(typeof(PlayerControl), "ReportDeadBody")]
public static class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        try
        {
            if (CheatToggles.disableReportsAndMeetings)
            {
                return false;
            }
        }
        catch { }

        return true;
    }
}

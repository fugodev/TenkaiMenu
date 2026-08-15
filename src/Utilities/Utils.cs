using System;
using System.Text;
using UnityEngine;
using InnerNet;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using System.IO;
using Hazel;
using System.Reflection;
using AmongUs.GameOptions;
using BepInEx;
using HarmonyLib;
using UnityEngine.SceneManagement;
using Sentry.Internal.Extensions;
using System.Runtime.CompilerServices;
using AmongUs.InnerNet.GameDataMessages;
using Il2CppInterop.Runtime.Injection;

namespace TenkaiMenu;

public static class Utils
{
    public static bool isPastingInput;
    public static ReferenceDataManager ReferenceDataManager = DestroyableSingleton<ReferenceDataManager>.Instance; // Useful for getting full lists of all the Among Us cosmetics IDs
    public static SabotageSystemType SabotageSystem => ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
    public static bool isShip => ShipStatus.Instance;
    public static bool isClient => AmongUsClient.Instance;
    public static bool isLobby => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined && !isFreePlay;
    public static bool isOnlineGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    public static bool isLocalGame => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
    public static bool isFreePlay => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    public static bool isPlayer => PlayerControl.LocalPlayer;
    public static bool isHost => AmongUsClient.Instance && AmongUsClient.Instance.AmHost;
    public static bool isInGame => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && isPlayer;
    public static bool isMeeting => MeetingHud.Instance;
    public static bool isMeetingVoting => isMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
    public static bool isMeetingProceeding => isMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Proceeding;
    public static bool isExiling => ExileController.Instance && !(isAirshipMap && SpawnInMinigame.Instance.isActiveAndEnabled);
    public static bool isAnySabotageActive => ShipStatus.Instance && SabotageSystem.AnyActive;
    public static bool isNormalGame => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal;
    public static bool isHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;
    public static bool isSkeldMap => (MapNames)GetCurrentMapID() == MapNames.Skeld;
    public static bool isMiraHQMap => (MapNames)GetCurrentMapID() == MapNames.MiraHQ;
    public static bool isPolusMap => (MapNames)GetCurrentMapID() == MapNames.Polus;
    public static bool isDleksMap => (MapNames)GetCurrentMapID() == MapNames.Dleks;
    public static bool isAirshipMap => (MapNames)GetCurrentMapID() == MapNames.Airship;
    public static bool isFungleMap => (MapNames)GetCurrentMapID() == MapNames.Fungle;
    public const float DefaultSpeed = 2.5f;
    public const float DefaultGhostSpeed = 3f;

    // Checks if LocalPlayer's speed is at its default value
    public static bool IsSpeedDefault(bool forGhost = false)
    {
        return forGhost ? Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, DefaultGhostSpeed) :
            Mathf.Approximately(PlayerControl.LocalPlayer.MyPhysics.Speed, DefaultSpeed);
    }

    // Snaps LocalPlayer's speed to the default if within snapRange
    public static void SnapSpeedToDefault(float snapRange, bool forGhost = false)
    {
        if (forGhost)
        {
            PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed - DefaultGhostSpeed)
                                                             < snapRange ? DefaultGhostSpeed : PlayerControl.LocalPlayer.MyPhysics.GhostSpeed;
        }
        else
        {
            PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed - DefaultSpeed)
                                                        < snapRange ? DefaultSpeed : PlayerControl.LocalPlayer.MyPhysics.Speed;
        }
    }

    // Gets a player's real name, display name, and whether they are disguised or not
    public static (string realName, string displayName, bool isDisguised) GetPlayerIdentity(PlayerControl player)
    {
        if (player == null || player.Data == null) return ("", "", false);

        var realName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>";
        var displayName = $"<color=#{ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[player.CurrentOutfit.ColorId])}>{player.CurrentOutfit.PlayerName}</color>";
        var isDisguised = player.CurrentOutfit.PlayerName != player.Data.PlayerName;

        return (realName, displayName, isDisguised);
    }

    // Checks if player is currently vanished
    public static bool IsVanished(NetworkedPlayerInfo playerInfo)
    {
        PhantomRole phantomRole = playerInfo.Role as PhantomRole;

        if (phantomRole != null)
        {
            return phantomRole.fading || phantomRole.isInvisible;
        }

        return false;
    }

    // Checks whether a player is a valid target depending on whether killAnyone cheat is enabled or not
    public static bool IsValidTarget(NetworkedPlayerInfo target)
    {
        var killAnyoneRequirements = target && !target.Disconnected && target.Object.Visible && target.PlayerId != PlayerControl.LocalPlayer.PlayerId && target.Role && target.Object;

        var fullRequirements = killAnyoneRequirements && !target.IsDead && !target.Object.inVent && !target.Object.inMovingPlat && target.Role.CanBeKilled;

        return CheatToggles.killAnyone ? killAnyoneRequirements : fullRequirements;
    }

    public static List<NetworkedPlayerInfo> GetAllPlayerData()
    {
        var playerDataList = new List<NetworkedPlayerInfo>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.Data != null)
            {
                playerDataList.Add(player.Data);
            }
        }

        return playerDataList;
    }

    // Adjusts HUD resolution
    // Used to fix UI problems when zooming out
    public static void AdjustResolution()
    {
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
    }

    // Gets RoleBehaviour from a RoleType
    public static RoleBehaviour GetBehaviourByRoleType(RoleTypes roleType)
    {
        return RoleManager.Instance.AllRoles.ToArray().First(r => r.Role == roleType);
    }

    // Gets RoleBehaviour from a TeamType
    public static RoleBehaviour GetBehaviourByTeamType(RoleTeamTypes roleTeamType)
    {
        RoleTypes roleType = (RoleTypes)Enum.Parse(typeof(RoleTypes), roleTeamType.ToString(), true);
        RoleBehaviour role = GetBehaviourByRoleType(roleType);

        return role;
    }

    public static void ForceSetScanner(PlayerControl player, bool toggle)
    {
        var count = ++player.scannerCount;
        player.SetScanner(toggle, count);
        RpcSetScannerMessage rpcMessage = new(player.NetId, toggle, count);
        AmongUsClient.Instance.LateBroadcastReliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
    }

    public static void ForcePlayAnimation(byte animationType)
    {
        // PlayerControl.LocalPlayer.RpcPlayAnimation(1) wouldn't work if visual tasks are turned off
        // The below way makes sure it works regardless of visual task settings

        PlayerControl.LocalPlayer.PlayAnimation(animationType);
        RpcPlayAnimationMessage rpcMessage = new(PlayerControl.LocalPlayer.NetId, animationType);
        AmongUsClient.Instance.LateBroadcastUnreliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
    }

    // Coroutine to teleport the LocalPlayer to a position after a delay
    public static System.Collections.IEnumerator DelayedSnapTo(Vector2 position, float delay = 0.25f)
    {
        yield return new WaitForSeconds(delay);
        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
    }

    public static void NetworkSnapPlayer(PlayerControl player, Vector2 position)
    {
        if (player == null || player.NetTransform == null || AmongUsClient.Instance == null) return;

        ushort num = (ushort)(player.NetTransform.lastSequenceId + 8);
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetTransform.NetId, 21, SendOption.None, -1);
        NetHelpers.WriteVector2(position, writer);
        writer.Write(num);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    // Kills any player using RPC calls
    public static void MurderPlayer(PlayerControl target, MurderResultFlags result)
    {
        // Defensive checks
        if (target == null) return;
        if (PlayerControl.LocalPlayer == null) return;
        if (target.Data == null || target.Data.Disconnected) return;

        if (isFreePlay)
        {
            try
            {
                PlayerControl.LocalPlayer.MurderPlayer(target, MurderResultFlags.Succeeded);
            }
            catch { }
            return;
        }

        if (!isInGame && !isFreePlay)
        {
            // Do not attempt murder outside of a running game
            return;
        }

        try
        {
            // Use the game's Rpc call to perform a murder. This avoids constructing RPCs for every client
            // which could lead to invalid client IDs and lobby disconnects.
            bool didSucceed = result == MurderResultFlags.Succeeded;
            PlayerControl.LocalPlayer.RpcMurderPlayer(target, didSucceed);
        }
        catch
        {
            // Fallback: try direct MurderPlayer if Rpc fails for some reason
            try { PlayerControl.LocalPlayer.MurderPlayer(target, result); } catch { }
        }
    }

    public static ClientData getClientByPlayer(PlayerControl player)
    {
        try
        {
            return AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Character.PlayerId == player.PlayerId);
        }
        catch
        {
            return null;
        }
    }

    public static int getClientIdByPlayer(PlayerControl player)
    {
        if (player == null) return -1;
        var client = getClientByPlayer(player);
        return client == null ? -1 : client.Id;
    }

    // Vote to kick a player (casts LocalPlayer's vote for target client)
    public static void voteKick(NetworkedPlayerInfo playerData)
    {
        if (playerData == null) return;

        PlayerControl targetPlayer = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p && p.Data == playerData);
        if (targetPlayer == null) return;

        int clientId = getClientIdByPlayer(targetPlayer);
        if (clientId < 0) return;

        var voteBan = DestroyableSingleton<VoteBanSystem>.Instance;
        if (isHost)
        {
            AmongUsClient.Instance.KickPlayer(clientId, false);
            return;
        }

        if (voteBan == null) return;

        if (voteBan.HasMyVote(targetPlayer.OwnerId))
        {
            return;
        }

        try
        {
            voteBan.CmdAddVote(targetPlayer.OwnerId);
        }
        catch { }

        int voteCount = 1;
        if (voteBan.Votes.TryGetValue(targetPlayer.OwnerId, out var countArray))
        {
            voteCount = 0;
            foreach (int vote in countArray)
            {
                if (vote != 0)
                {
                    voteCount++;
                }
            }
        }

        if (HudManager.Instance != null && HudManager.Instance.Notifier != null)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage($"<color=#ffffff><b><color=#c40033>TenkaiMenu</color></b> <color=#ffff00>VoteKicked</color> {targetPlayer.Data.PlayerName} <color=#00ff00>({voteCount}/3)</color><color=#ff0000>!</color></color>");
        }
    }

    public static void SelfBanGlitch()
    {
        if (PlayerControl.LocalPlayer == null) return;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.AmOwner || player.Data == null) continue;

            try
            {
                PlayerControl.LocalPlayer.CmdReportDeadBody(player.Data);
            }
            catch { }
        }
    }

    public static void CompleteTask(PlayerTask task)
    {
        if (task == null || task.IsComplete || PlayerControl.LocalPlayer == null) return;

        if (isFreePlay)
        {
            PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
            return;
        }

        var hostData = AmongUsClient.Instance.GetHost();
        if (hostData == null || hostData.Character.Data.Disconnected) return;

        foreach (var item in PlayerControl.AllPlayerControls)
        {
            var messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.CompleteTask, SendOption.Reliable, AmongUsClient.Instance.GetClientIdFromCharacter(item));
            messageWriter.WritePacked(task.Id);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }

    // Opens Chat UI
    public static void OpenChat()
    {
        if (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.chatScreen.SetActive(true);
            PlayerControl.LocalPlayer.NetTransform.Halt();
            DestroyableSingleton<HudManager>.Instance.Chat.StartCoroutine(DestroyableSingleton<HudManager>.Instance.Chat.CoOpen());
            if (DestroyableSingleton<FriendsListManager>.InstanceExists)
            {
                DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(true);
            }
            if (DestroyableSingleton<HudManager>.Instance.Chat.chatNotification.gameObject.activeSelf)
			{
				DestroyableSingleton<HudManager>.Instance.Chat.chatNotification.Close();
			}
        }

    }

    // Draws a tracer line between two GameObjects
    public static void DrawTracer(GameObject sourceObject, GameObject targetObject, Color color)
    {
        var lineRenderer = sourceObject.GetComponent<LineRenderer>();

        if (!lineRenderer)
        {
            lineRenderer = sourceObject.AddComponent<LineRenderer>();
        }

        lineRenderer.SetVertexCount(2);
        lineRenderer.SetWidth(0.02F, 0.02F);

        // I just picked an already existing material from the game
        Material material = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;

        lineRenderer.material = material;
        lineRenderer.SetColors(color, color);

        lineRenderer.SetPosition(0, sourceObject.transform.position);
        lineRenderer.SetPosition(1, targetObject.transform.position);
    }

    // Returns whether the ChatUI should be active or not
    public static bool IsChatUiActive()
    {
        try
        {
            return CheatToggles.enableChat || MeetingHud.Instance || !ShipStatus.Instance || PlayerControl.LocalPlayer.Data.IsDead;
        }
        catch
        {
            return false;
        }
    }

    // Returns the max number of nested RPCs that can be in a GameData message
    // without getting kicked by AC
    public static int GetMaxRpcPackingLimit()
    {
        int num = 0;

        if (isClient && AmongUsClient.Instance.AmHost)
        {
            num = GameManager.Instance.LogicOptions.MaxPlayers * 2;
        }

        return 10 + num;
    }

    // Overloads target with set strength using Pet RPCs that
    // repeatedly restart the hand-petting animation, preventing old petting coroutines
    // from resolving
    public static void Overload(int targetId, int strength)
    {
        if (strength < 1) return;

        int maxRpc = GetMaxRpcPackingLimit();

        uint netId = PlayerControl.LocalPlayer.MyPhysics.NetId;
        byte rpcCall = (byte)RpcCalls.Pet;

        if (strength <= maxRpc)
        {
            // SendOption.None has no flow control, allowing for flooding without limits

            var messageWriter = MessageWriter.Get(SendOption.None);

            if (targetId < 0) // -1 = Broadcast
            {
                messageWriter.StartMessage(Tags.GameData);
                messageWriter.Write(AmongUsClient.Instance.GameId);
            }
            else
            {
                messageWriter.StartMessage(Tags.GameDataTo);
                messageWriter.Write(AmongUsClient.Instance.GameId);
                messageWriter.WritePacked(targetId);
            }

            for (var msg = 0; msg < strength; msg++)
            {
                messageWriter.StartMessage((byte)GameDataTypes.RpcFlag);

                messageWriter.WritePacked(netId);

                messageWriter.Write(rpcCall);

                // Use LocalPlayer.GetTruePosition() as the petting position
                // to minimize WalkPlayerTo delay and start the hand-petting animation immediately

                NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), messageWriter);

                // Pet position is decoded as (-50, -50) on target clients
                // This keeps the hand-petting animation out of normal view

                messageWriter.Write((ushort)0);

                messageWriter.Write((ushort)0);

                messageWriter.EndMessage();
            }

            messageWriter.EndMessage();

            AmongUsClient.Instance.connection.Send(messageWriter);

            messageWriter.Recycle();
        }
        else
        {
            int strengthGroups = strength / maxRpc;
            int remainder = strength % maxRpc;

            for (int group = 0; group < strengthGroups; group++)
            {
                Overload(targetId, maxRpc);
            }

            Overload(targetId, remainder);
        }
    }

    // Closes Chat UI
    public static void CloseChat()
    {
        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
        {
            DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
        }
    }

    // Gets the distance between two players
    public static float GetDistanceBetween(PlayerControl source, PlayerControl target)
    {

        Vector2 vector = target.GetTruePosition() - source.GetTruePosition();
		float magnitude = vector.magnitude;

        return magnitude;

    }

    // Returns a list of all the players in the game ordered from closest to farthest (from LocalPlayer by default)
    public static System.Collections.Generic.List<PlayerControl> GetPlayersSortedByDistance(PlayerControl source = null)
    {

        if (source.IsNull())
        {
            source = PlayerControl.LocalPlayer;
        }

        System.Collections.Generic.List<PlayerControl> outputList = new System.Collections.Generic.List<PlayerControl>();

        outputList.Clear();

        var allPlayers = GameData.Instance.AllPlayers;
        foreach (var playerInfo in allPlayers)
        {
            var player = playerInfo.Object;
            if (player)
            {
                outputList.Add(player);
            }
        }

        outputList = outputList.OrderBy(target => GetDistanceBetween(source, target)).ToList();

        return outputList.Count <= 0 ? null : outputList;
    }

    // Returns current map ID if available
    public static byte GetCurrentMapID()
    {
        // Works for the tutorial
        if (isFreePlay)
        {
            return (byte)AmongUsClient.Instance.TutorialMapId;
        }

        // Works for local / online games
        if (GameOptionsManager.Instance?.currentGameOptions != null)
        {
            return GameOptionsManager.Instance.currentGameOptions.MapId;
        }

        // Defaults to byte.MaxValue if the current map ID is unavailable
        return byte.MaxValue;
    }

    // Gets SystemType of the room the player is currently in
    public static SystemTypes GetCurrentRoom()
    {
        return HudManager.Instance.roomTracker.LastRoom.RoomId;
    }

    // Gets the PlainShipRoom of room that overlaps specified position
    public static PlainShipRoom GetRoomFromPosition(Vector2 position)
    {
        return ShipStatus.Instance == null ? null : ShipStatus.Instance.AllRooms.FirstOrDefault(
            room => room != null && room.roomArea != null && room.roomArea.OverlapPoint(position));
    }

    // Returns colored ping text for PingTracker
    public static string GetColoredPingText(string pingText, int ping)
    {
        return ping switch
        {
            < 1 => $"<color=#b8b8b8>{pingText}</color>", // Grey for ping < 1
            < 100 => $"<color=#00ff00ff>{pingText}</color>", // Green for ping < 100
            < 400 => $"<color=#ffff00ff>{pingText}</color>", // Yellow for 100 < ping < 400
            _ => $"<color=#ff0000ff>{pingText}</color>" // Red for ping > 400
        };
    }

    // Returns a gradient-colored text string with a subtle glow effect
    public static string GetGradientText(string text, Color startColor, Color endColor, float animationOffset = 0f)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < text.Length; i++)
        {
            float t = text.Length <= 1 ? 0f : (float)i / (text.Length - 1);
            Color baseColor = Color.Lerp(startColor, endColor, t);
            float glow = 0.1f + Mathf.Sin((Time.time + animationOffset + i * 0.1f) * 4.5f) * 0.1f;
            Color glowingColor = Color.Lerp(baseColor, Color.white, glow);

            builder.Append($"<color=#{ColorUtility.ToHtmlStringRGB(glowingColor)}>{text[i]}</color>");
        }

        return builder.ToString();
    }

    // Returns the current approximate FPS
    public static int GetFps()
    {
        return (int)(1f / Time.unscaledDeltaTime);
    }

    // Gets a UnityEngine.KeyCode from a string
    public static KeyCode StringToKeycode(string keyCodeStr)
    {

        if(!string.IsNullOrEmpty(keyCodeStr)) // Empty strings are automatically invalid
        {
            try
            {
                // Case-insensitive parse of UnityEngine.KeyCode to check if string is valid
                KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeStr, true);

                return keyCode;

            }

            catch { }
        }

        return KeyCode.Delete; // If string is invalid, return Delete as the default key
    }

    // Gets a platform type from a string
    public static bool StringToPlatformType(string platformStr, out Platforms? platform)
    {
        if (!string.IsNullOrEmpty(platformStr)) // Empty strings are automatically invalid
        {
            try
            {
                // Case-insensitive parse of Platforms from string (if it valid)
                platform = (Platforms)Enum.Parse(typeof(Platforms), platformStr, true);

                return true; // If platform type is valid, return false
            }catch{}
        }

        platform = null;
        return false; // If platform type is invalid, return false
    }

    public static string PlatformTypeToString(Platforms platform)
    {
        return platform switch
        {
            Platforms.StandaloneEpicPC => "Epic Games",
            Platforms.StandaloneSteamPC => "Steam",
            Platforms.StandaloneMac => "Mac",
            Platforms.StandaloneWin10 => "Microsoft Store",
            Platforms.StandaloneItch => "Itch.io",
            Platforms.IPhone => "iPhone / iPad",
            Platforms.Android => "Android",
            Platforms.Switch => "Nintendo Switch",
            Platforms.Xbox => "Xbox",
            Platforms.Playstation => "PlayStation",
            (Platforms)112 => "Starlight",
            _ => "Unknown"
        };
    }

    // Gets the name for a specified player's role as a string
    // Strings are automatically translated
    public static string GetRoleName(NetworkedPlayerInfo playerData)
    {
        var translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(playerData.Role.StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        if (translatedRole != "STRMISS") return translatedRole;

        translatedRole = DestroyableSingleton<TranslationController>.Instance.GetString(GetBehaviourByTeamType(playerData.Role.TeamType).StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());
        return translatedRole;
    }

    // Gets the appropriate nametag for a player
    // Gets the appropriate nametag for a player
    public static string GetNameTag(NetworkedPlayerInfo playerInfo, string playerName, bool isChat = false)
    {
        var nameTag = playerName;

        if (playerInfo.Role.IsNull() || playerInfo.IsNull() || playerInfo.Disconnected ||
            playerInfo.Object.CurrentOutfit.IsNull()) return nameTag;

        var player = AmongUsClient.Instance.GetClientFromPlayerInfo(playerInfo);
        var host = AmongUsClient.Instance.GetHost();
        var level = playerInfo.PlayerLevel + 1;

        var platform = "Unknown";
        if (!isLocalGame) try { platform = PlatformTypeToString(player.PlatformData.Platform); } catch { }

        var roleColor = ColorUtility.ToHtmlStringRGB(playerInfo.Role.TeamColor);
        bool isImpostorTeam = false;

        if (playerInfo.Role != null)
        {
            string roleEnumStr = playerInfo.Role.Role.ToString().ToLower();
            string roleNameLower = GetRoleName(playerInfo).ToLower();
            string teamStr = "";
            try { teamStr = playerInfo.Role.TeamType.ToString().ToLower(); } catch { }

            // Assign standard role colors
            if (roleEnumStr.Contains("crewmate") || roleNameLower.Contains("crewmate")) roleColor = "3b92ff";
            else if (roleEnumStr.Contains("guardian") || roleNameLower.Contains("guardian") || roleNameLower.Contains("angel")) roleColor = "ffff00";
            else if (roleEnumStr.Contains("noise") || roleNameLower.Contains("noice")) roleColor = "ff9900";
            else if (roleEnumStr.Contains("tracker") || roleNameLower.Contains("tracker")) roleColor = "00ff00";
            else if (roleEnumStr.Contains("engineer") || roleNameLower.Contains("engineer")) roleColor = "00ffff";
            else if (roleEnumStr.Contains("detective") || roleNameLower.Contains("detective")) roleColor = "fc0dbc";
            else if (roleEnumStr.Contains("scientist") || roleNameLower.Contains("scientist")) roleColor = "cc66ff";

            if (teamStr.Contains("impostor") || roleEnumStr.Contains("impostor") || roleNameLower.Contains("impostor") ||
                roleEnumStr.Contains("shapeshifter") || roleNameLower.Contains("shapeshifter") ||
                roleEnumStr.Contains("phantom") || roleNameLower.Contains("phantom"))
            {
                isImpostorTeam = true;
            }
        }

        var hostString = player == host ? " - Host" : "";
        
        // FIX: If I am an Impostor, my teammates are always Red. 
        // If I am Crew, names are only Red if "See Roles" is active (as per your request).
        bool isLocalPlayerImpostor = PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Impostor;
        bool shouldBeRed = (isImpostorTeam && isLocalPlayerImpostor) || (CheatToggles.seeRoles && isImpostorTeam);
        
        var finalName = shouldBeRed ? $"<color=#ff0000>{playerName}</color>" : $"<color=#ffffff>{playerName}</color>";

        if (CheatToggles.seeRoles)
        {
            var coloredRole = $"<color=#{roleColor}>{GetRoleName(playerInfo)}</color>";
            nameTag = CheatToggles.seePlayerInfo ? 
                $"<size=70%><color=#ff00ff>{platform} - Lv:{level}{hostString}</color></size>\r\n<size=70%>{coloredRole}</size>\r\n{finalName}" :
                $"<size=70%>{coloredRole}</size>\r\n{finalName}";
        }
        else
        {
            nameTag = (CheatToggles.seePlayerInfo && !isChat) ? 
                $"<size=70%><color=#ff00ff>{platform} - Lv:{level}{hostString}</color></size>\r\n{finalName}" : finalName;
        }

        return isChat ? nameTag.Replace("\r\n", " ") : nameTag;
    }

    // Returns a player's NetworkedPlayerInfo from their client ID
    public static NetworkedPlayerInfo GetPlayerDataFromClientId(int clientId)
    {
        var players = PlayerControl.AllPlayerControls.ToArray();

        for (int i = 0; i < players.Count; i++)
		{   NetworkedPlayerInfo playerData = players[i].Data;

			if (playerData.ClientId == clientId)
			{
				return playerData;
			}
		}

        return null; // Returns null if no matching player is found
    }

    // Returns a random 1 - 12 characters long name
    public static string GetRandomName()
    {
        var length = UnityEngine.Random.Range(1, 13);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
    }

    // Returns current AmongUsClient ping in ms
    public static int GetPing()
    {
        if (isClient && AmongUsClient.Instance.AmClient)
        {
            return AmongUsClient.Instance.Ping;
        }
        else
        {
            return 0; // Returns 0 if not connected to a game
        }
    }

    // Shows a custom popup ingame
    // Found here: https://github.com/NuclearPowered/Reactor/blob/6eb0bf19c30733b78532dada41db068b2b247742/Reactor/Networking/Patches/HttpPatches.cs
    public static void ShowPopup(string text)
    {
        var popup = UnityEngine.Object.Instantiate(DiscordManager.Instance.discordPopup, Camera.main!.transform);

        var background = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        var size = background.size;
        size.x *= 2.5f;
        background.size = size;

        popup.TextAreaTMP.fontSizeMin = 2;
        popup.Show(text);
    }

    public static void ShowNewPopup(string text)
    {
        DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(text);
    }

    // Loads sprites from manifest resources
    // Found here: https://github.com/Loonie-Toons/TOHE-Restored/blob/TOHE/Modules/Utils.cs
    public static Dictionary<string, Sprite> CachedSprites = new();
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;

            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;

            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            TenkaiMenu.Log.LogError($"Failed to read Texture: {path}");
        }
        return null;
    }

    // Loads textures from manifest resources
    // Found here: https://github.com/Loonie-Toons/TOHE-Restored/blob/TOHE/Modules/Utils.cs
    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();

            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray(), false);
            return texture;
        }
        catch
        {
            TenkaiMenu.Log.LogError($"Failed to read Texture: {path}");
        }
        return null;
    }

    // Opens the config file in the default text editor
    public static void OpenConfigFile()
    {
        var configFilePath = TenkaiMenu.Plugin.Config.ConfigFilePath;
        var configEditor = TenkaiMenu.configEditor.Value;

        if (!string.IsNullOrWhiteSpace(configEditor))
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = configEditor,
                        Arguments = configFilePath,
                        UseShellExecute = true
                        //Verb = "edit"
                    });
                }
                catch (Exception ex)
                {
                    TenkaiMenu.Log.LogError(ex.Message);
                }
            }
            else
            {
                TenkaiMenu.Log.LogError("Configuration file does not exist");
            }
        }
        else
        {
            TenkaiMenu.Log.LogError("Configuration editor not specified");
        }
    }

    public class PanicCleaner : MonoBehaviour
    {
        // Creates a PanicCleaner to unpatch Harmony
        public static void Create()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PanicCleaner>();
            var go = new GameObject("TenkaiMenu_PanicCleaner");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<PanicCleaner>();
        }

        // Unpatching Harmony in handled in the next frame after creation
        // This allows some patches to run for a last time and finish properly
        private void LateUpdate()
        {
            try { Harmony.UnpatchID(TenkaiMenu.Id); } catch { }
            Destroy(gameObject);
        }
    }

    public static void Panic()
    {
        TenkaiMenu.isPanicked = true;

        CheatToggles.DisableAll();

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = false;

        Scene scene = SceneManager.GetActiveScene();

        if (scene.name == "MainMenu" || scene.name == "MatchMaking")
        {
            SceneManager.LoadScene(scene.name);
        }

        UnityEngine.Object.Destroy(TenkaiMenu.menuUI);

        UnityEngine.Object.Destroy(TenkaiMenu.consoleUI);
        UnityEngine.Object.Destroy(TenkaiMenu.doorsUI);
        UnityEngine.Object.Destroy(TenkaiMenu.tasksUI);
        UnityEngine.Object.Destroy(TenkaiMenu.protectUI);
        // UnityEngine.Object.Destroy(TenkaiMenu.rolesUI);

        UnityEngine.Object.Destroy(TenkaiMenu.keybindListener);

        PanicCleaner.Create();
    }
}
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace TenkaiMenu;

public static class TenkaiCheats
{
    private static bool _isScanAnimActive;
    private static bool _isCamsAnimActive;
    private static bool _isInvisibleActive;
    private static float _levelFarmTimer;
    private static float _autoKillTimer;
    private static Vector2 _invisibilityOriginalPosition;

    private static readonly Dictionary<byte, ushort> _ventSeqIds = new Dictionary<byte, ushort>();
    private static float _spamTpImpsTimer = -999f;
    private static float _spamTpAllTimer = -999f;
    private static float _destroyInGameTimer = -999f;
    private static bool _doorHallucinationSent;


    public static void CloseMeetingCheat()
    {
        if (!CheatToggles.closeMeeting) return;

        if (Utils.isMeeting) // Closes MeetingHud window if it's open
        {
            // Destroy MeetingHud window gameobject
            MeetingHud.Instance.DespawnOnDestroy = false;
            UnityEngine.Object.Destroy(MeetingHud.Instance.gameObject);

            // Gameplay must be reenabled
            DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false));
            PlayerControl.LocalPlayer.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown());
            ShipStatus.Instance.EmergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
            DestroyableSingleton<HudManager>.Instance.SetMapButtonEnabled(true);
            DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
            ControllerManager.Instance.CloseAndResetAll();
        }
        else if (ExileController.Instance) // Ends exile cutscene if it's playing
        {
            ExileController.Instance.ReEnableGameplay();
            ExileController.Instance.WrapUp();
        }

        CheatToggles.closeMeeting = false;
    }

    public static void SkipMeetingCheat()
    {
        if (!CheatToggles.skipMeeting) return;

        if (Utils.isMeeting)
        {
            MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), null, true);
        }

        CheatToggles.skipMeeting = false;
    }

    public static void CallMeetingCheat()
    {
        if (!CheatToggles.callMeeting) return;

        if (Utils.isHost)
        {
            MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
            PlayerControl.LocalPlayer.RpcStartMeeting(null);
        }
        else
        {
            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
        }

        CheatToggles.callMeeting = false;
    }

    public static void ForceStartGameCheat()
    {
        if (!CheatToggles.forceStartGame) return;

        if (Utils.isHost && Utils.isLobby)
        {
            AmongUsClient.Instance.SendStartGame();
        }

        CheatToggles.forceStartGame = false;
    }

    public static void CompleteMyTasksCheat()
    {
        if (CheatToggles.completeMyTasks)
        {
            foreach (var task in PlayerControl.LocalPlayer.myTasks)
            {
                Utils.CompleteTask(task);
            }

            CheatToggles.completeMyTasks = false;
        }
    }

    public static void OpenSabotageMapCheat()
    {
        if (!CheatToggles.sabotageMap) return;

        DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
        {
            Mode = MapOptions.Modes.Sabotage
        });

        CheatToggles.sabotageMap = false;
    }

    public static void HandleEngineerCheats(EngineerRole engineerRole)
    {
        if (CheatToggles.endlessVentTime)
        {
            engineerRole.inVentTimeRemaining = float.MaxValue;
        }
        else if (engineerRole.inVentTimeRemaining > engineerRole.GetCooldown())
        {
            engineerRole.inVentTimeRemaining = engineerRole.GetCooldown();
        }

        if (CheatToggles.noVentCooldown)
        {
            if (engineerRole.cooldownSecondsRemaining > 0f)
            {
                engineerRole.cooldownSecondsRemaining = 0f;

                DestroyableSingleton<HudManager>.Instance.AbilityButton.ResetCoolDown();
                DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCooldownFill(0f);
            }
        }
    }

    public static void HandleShapeshifterCheats(ShapeshifterRole shapeshifterRole)
    {
        if (CheatToggles.endlessSsDuration)
        {
            shapeshifterRole.durationSecondsRemaining = float.MaxValue;
        }
        else if (shapeshifterRole.durationSecondsRemaining > GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.ShapeshifterDuration))
        {
            shapeshifterRole.durationSecondsRemaining = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.ShapeshifterDuration);
        }
    }

    public static void HandleScientistCheats(ScientistRole scientistRole)
    {
        if (CheatToggles.noVitalsCooldown)
        {
            scientistRole.currentCooldown = 0f;
        }

        if (CheatToggles.endlessBattery)
        {
            scientistRole.currentCharge = float.MaxValue;
        }
        else if (scientistRole.currentCharge > scientistRole.RoleCooldownValue)
        {
            scientistRole.currentCharge = scientistRole.RoleCooldownValue;
        }
    }

    public static void HandleTrackerCheats(TrackerRole trackerRole)
    {
        if (CheatToggles.noTrackingCooldown)
        {
            trackerRole.cooldownSecondsRemaining = 0f;
            trackerRole.delaySecondsRemaining = 0f;

            DestroyableSingleton<HudManager>.Instance.AbilityButton.ResetCoolDown();
            DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCooldownFill(0f);
        }

        if (CheatToggles.noTrackingDelay && MapBehaviour.Instance != null)
        {
            MapBehaviour.Instance.trackedPointDelayTime = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.TrackerDelay);
        }

        if (CheatToggles.endlessTracking)
        {
            trackerRole.durationSecondsRemaining = float.MaxValue;
        }
        else if (trackerRole.durationSecondsRemaining > GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.TrackerDuration))
        {
            trackerRole.durationSecondsRemaining = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.TrackerDuration);
        }
    }

    public static void UseVentCheat(HudManager hudManager)
    {
        try
        {
            if (!PlayerControl.LocalPlayer.Data.Role.CanVent && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                hudManager.ImpostorVentButton.gameObject.SetActive(CheatToggles.unlockVents);
            }
        } catch { }
    }

    public static void WalkInVentCheat()
    {
        try
        {
            if (!CheatToggles.walkInVents) return;

            PlayerControl.LocalPlayer.inVent = false;
            PlayerControl.LocalPlayer.moveable = true;
        } catch { }
    }

    public static void KickVentsCheat()
    {
        if (!CheatToggles.kickVents) return;

        foreach (var vent in ShipStatus.Instance.AllVents)
        {
            VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, vent.Id);
        }

        CheatToggles.kickVents = false;
    }

    public static void DoorHallucinationAllCheat()
    {
        if (!CheatToggles.doorHallucinationAll)
        {
            _doorHallucinationSent = false;
            return;
        }

        if (_doorHallucinationSent) return;
        _doorHallucinationSent = true;

        if (ShipStatus.Instance == null)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Door Hallucination failed: The game must be started.");
            return;
        }

        foreach (PlayerControl target in PlayerControl.AllPlayerControls)
        {
            if (target == null || target.AmOwner || target.Data == null || target.Data.Disconnected) continue;
            DoorHallucination(target);
        }
    }

    // --- VENT TELEPORT FEATURES ---

    public static void TeleportAllToVent()
    {
        if (ShipStatus.Instance == null || Utils.isLobby) return;

        var vents = ShipStatus.Instance.AllVents;
        if (vents == null || vents.Count == 0) return;

        Vent targetVent = null;
        foreach (var vent in vents)
        {
            if (vent != null)
            {
                targetVent = vent;
                break;
            }
        }

        if (targetVent == null) return;

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.AmOwner || player.Data == null || player.Data.Disconnected || player.Data.IsDead) continue;
            VentTP(player, targetVent.Id);
        }
    }

    public static void SpamTpImpsCheat()
    {
        if (!CheatToggles.spamTpImps) return;
        if (ShipStatus.Instance == null || Utils.isLobby) return;
        if (Time.realtimeSinceStartup - _spamTpImpsTimer < 1f) return;

        _spamTpImpsTimer = Time.realtimeSinceStartup;

        var vents = ShipStatus.Instance.AllVents;
        if (vents == null || vents.Count == 0) return;

        List<PlayerControl> impostors = new List<PlayerControl>();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.AmOwner || player.Data == null || player.Data.Disconnected || player.Data.IsDead) continue;
            if (player.Data.Role != null && player.Data.Role.IsImpostor)
            {
                impostors.Add(player);
            }
        }

        if (impostors.Count == 0) return;

        List<Vent> shuffledVents = new List<Vent>(vents);
        for (int i = shuffledVents.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Vent temp = shuffledVents[i];
            shuffledVents[i] = shuffledVents[j];
            shuffledVents[j] = temp;
        }

        int count = Mathf.Min(impostors.Count, shuffledVents.Count);
        for (int i = 0; i < count; i++)
        {
            Vent targetVent = shuffledVents[i];
            if (targetVent == null) continue;
            VentTP(impostors[i], targetVent.Id);
        }
    }

    public static void SpamTpAllCheat()
    {
        if (!CheatToggles.spamTpAll) return;
        if (ShipStatus.Instance == null || Utils.isLobby) return;
        if (Time.realtimeSinceStartup - _spamTpAllTimer < 2f) return;

        _spamTpAllTimer = Time.realtimeSinceStartup;

        var vents = ShipStatus.Instance.AllVents;
        if (vents == null || vents.Count == 0) return;

        List<PlayerControl> players = new List<PlayerControl>();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.AmOwner || player.Data == null || player.Data.Disconnected || player.Data.IsDead) continue;
            players.Add(player);
        }

        if (players.Count == 0) return;

        List<Vent> shuffledVents = new List<Vent>(vents);
        for (int i = shuffledVents.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Vent temp = shuffledVents[i];
            shuffledVents[i] = shuffledVents[j];
            shuffledVents[j] = temp;
        }

        int count = Mathf.Min(players.Count, shuffledVents.Count);
        for (int i = 0; i < count; i++)
        {
            Vent targetVent = shuffledVents[i];
            if (targetVent == null) continue;
            VentTP(players[i], targetVent.Id);
        }
    }

    public static void DestroyInGameCheat()
    {
        if (!CheatToggles.destroyInGame || CheatToggles.destroyInGamePlayerId < 0) return;
        if (ShipStatus.Instance == null || Utils.isLobby) return;
        if (Time.realtimeSinceStartup - _destroyInGameTimer < 0.5f) return;

        _destroyInGameTimer = Time.realtimeSinceStartup;

        var vents = ShipStatus.Instance.AllVents;
        if (vents == null || vents.Count == 0) return;

        PlayerControl target = null;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.AmOwner || player.Data == null || player.Data.Disconnected || player.Data.IsDead) continue;
            if (player.PlayerId == CheatToggles.destroyInGamePlayerId)
            {
                target = player;
                break;
            }
        }

        if (target == null)
        {
            CheatToggles.destroyInGame = false;
            CheatToggles.destroyInGamePlayerId = -1;
            return;
        }

        List<Vent> shuffledVents = new List<Vent>(vents);
        for (int i = shuffledVents.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Vent temp = shuffledVents[i];
            shuffledVents[i] = shuffledVents[j];
            shuffledVents[j] = temp;
        }

        Vent randomVent = shuffledVents[0];
        if (randomVent != null)
        {
            VentTP(target, randomVent.Id);
        }
    }

    private static void SendVentPair(AmongUsClient client, PlayerControl target, int ventId, int toClientId, ushort seqId)
    {
        byte seqLo = (byte)(seqId & 255);
        byte seqHi = (byte)(seqId >> 8);
        ushort num = (ushort)(seqId + 1);
        byte seq2Lo = (byte)(num & 255);
        byte seq2Hi = (byte)(num >> 8);
        SendUpdateSystemToClient(client, (SystemTypes)37, target.NetId, new byte[]
        {
            seqLo,
            seqHi,
            2,
            (byte)(ventId & 255)
        }, toClientId);
        SendUpdateSystemToClient(client, (SystemTypes)37, target.NetId, new byte[]
        {
            seq2Lo,
            seq2Hi,
            5,
            (byte)(ventId & 255)
        }, toClientId);
    }

    private static void SendUpdateSystemToClient(AmongUsClient client, SystemTypes systemType, uint senderNetId, byte[] extraBytes, int targetClientId)
    {
        if (client == null || ShipStatus.Instance == null) return;

        MessageWriter writer = client.StartRpcImmediately(ShipStatus.Instance.NetId, 35, SendOption.Reliable, targetClientId);
        if (writer == null) return;

        writer.Write((byte)systemType);
        writer.WritePacked(senderNetId);
        foreach (byte b in extraBytes)
        {
            writer.Write(b);
        }

        client.FinishRpcImmediately(writer);
    }

    public static void VentTP(PlayerControl target, int ventId)
    {
        if (target == null || target.AmOwner || target.Data == null)
        {
            return;
        }

        AmongUsClient client = AmongUsClient.Instance;
        if (client == null || !client.AmConnected || ShipStatus.Instance == null)
        {
            return;
        }

        if (client.AmHost)
        {
            try
            {
                PlayerPhysics myPhysics = target.MyPhysics;
                myPhysics?.RpcBootFromVent(ventId);
            }
            catch
            {
            }
            return;
        }

        byte pid = target.Data.PlayerId;
        ushort seqId;
        if (!_ventSeqIds.TryGetValue(pid, out seqId))
        {
            seqId = 1000;
        }

        int hostId = client.HostId;
        SendVentPair(client, target, ventId, hostId, seqId);
        _ventSeqIds[pid] = (ushort)(seqId + 2);
    }

    // --- OTHER CHEATS ---

    public static void UpdateFollowSelected()
    {
        if (!CheatToggles.followSelectedPlayer)
        {
            return;
        }

        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.MyPhysics?.body == null)
        {
            CheatToggles.followSelectedPlayer = false;
            CheatToggles.followSelectedPlayerId = -1;
            return;
        }

        PlayerControl target = null;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null) continue;
            if (player.PlayerId != CheatToggles.followSelectedPlayerId) continue;
            if (player.AmOwner || player.Data.IsDead) continue;
            target = player;
            break;
        }

        if (target == null)
        {
            CheatToggles.followSelectedPlayer = false;
            CheatToggles.followSelectedPlayerId = -1;
            return;
        }

        try
        {
            if (Camera.main != null)
            {
                var fc = Camera.main.gameObject.GetComponent<FollowerCamera>();
                if (fc != null)
                {
                    fc.enabled = true;
                    fc.SetTarget(PlayerControl.LocalPlayer);
                }
            }

            MoveTowardPlayer(target.transform.position);
        }
        catch
        {
            CheatToggles.followSelectedPlayer = false;
            CheatToggles.followSelectedPlayerId = -1;
        }
    }

    private static void MoveTowardPlayer(Vector2 targetPos)
    {
        var body = PlayerControl.LocalPlayer.MyPhysics.body;
        Vector2 myPos = body.position;
        if (Vector2.Distance(myPos, targetPos) > 0.8f)
        {
            Vector2 dir = (targetPos - myPos).normalized;
            float speed = PlayerControl.LocalPlayer.MyPhysics.TrueSpeed;

            body.position = Vector2.MoveTowards(myPos, targetPos, speed * Time.deltaTime);
            body.velocity = dir * speed;
            PlayerControl.LocalPlayer.MyPhysics.FlipX = dir.x < 0f;
            return;
        }

        body.velocity = Vector2.zero;
    }

    public static void DoorHallucination(PlayerControl target)
    {
        if (target == null)
        {
            return;
        }

        try
        {
            if (ShipStatus.Instance == null)
            {
                HudManager.Instance.Notifier.AddDisconnectMessage("Door Hallucination failed: Game has not started.");
            }
            else
            {
                AmongUsClient client = AmongUsClient.Instance;
                if (client == null || !client.AmConnected)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Door Hallucination failed: Not connected.");
                }
                else
                {
                    int targetClientId = Utils.getClientIdByPlayer(target);
                    if (targetClientId < 0)
                    {
                        HudManager.Instance.Notifier.AddDisconnectMessage("Door Hallucination failed: Could not resolve target client.");
                    }
                    else
                    {
                        HashSet<SystemTypes> rooms = new HashSet<SystemTypes>();
                        Il2CppReferenceArray<OpenableDoor> allDoors = ShipStatus.Instance.AllDoors;
                        for (int i = 0; i < allDoors.Length; i++)
                        {
                            if (allDoors[i] != null)
                            {
                                rooms.Add(allDoors[i].Room);
                            }
                        }

                        if (rooms.Count == 0)
                        {
                            HudManager.Instance.Notifier.AddDisconnectMessage("Door Hallucination failed: No doors found on this map.");
                        }
                        else
                        {
                            foreach (SystemTypes room in rooms)
                            {
                                MessageWriter w = client.StartRpcImmediately(ShipStatus.Instance.NetId, 27, SendOption.Reliable, targetClientId);
                                w.Write((byte)room);
                                client.FinishRpcImmediately(w);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Door Hallucination failed: exception.");
        }
    }

    public static void KillAllCheat()
    {
        if (!CheatToggles.killAll) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
        }
        else
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
            }
        }

        CheatToggles.killAll = false;
    }

    public static void KillAllCrewCheat()
    {
        if (!CheatToggles.killAllCrew) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
        }
        else
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.TeamType == RoleTeamTypes.Crewmate)
                {
                    Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
                }
            }
        }

        CheatToggles.killAllCrew = false;
    }

    public static void KillAllImpsCheat()
    {
        if (!CheatToggles.killAllImps) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
        }
        else
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.TeamType == RoleTeamTypes.Impostor)
                {
                    Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
                }
            }
        }

        CheatToggles.killAllImps = false;
    }

    public static void LevelFarmCheat()
    {
        if (!CheatToggles.levelFarm || Utils.isLobby) return;

        _levelFarmTimer -= Time.deltaTime;
        if (_levelFarmTimer <= 0f)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
            }
            _levelFarmTimer = 1f;
        }
    }

    public static void ProtectCheat()
    {
        if (!Utils.isHost || Utils.isLobby) return;

        foreach (var player in ProtectUI.playersToProtect)
        {
            if (player.protectedByGuardianId == -1)
            {
                PlayerControl.LocalPlayer.RpcProtectPlayer(player, PlayerControl.LocalPlayer.cosmetics.ColorId);
            }
        }
    }

    public static void TeleportCursorCheat()
    {
        if (PlayerControl.LocalPlayer?.NetTransform == null || Camera.main == null) return;
        if (!CheatToggles.teleportCursor) return;

        if (Input.GetMouseButtonDown(1))
        {
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    public static void SpeedHackCheat()
    {
        if (CheatToggles.speedHackEnabled)
        {
            Time.timeScale = Mathf.Clamp(CheatToggles.gameSpeed, 0.1f, 3f);
        }
        else if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
        }
    }

    private static readonly string[] ConfuseHats =
    {
        "hat_pk05_Fedora",
        "hat_pk03_Headphones",
        "hat_screamghostface",
        "hat_pk08_Cowboy",
        "hat_pk07_Bandana",
        "hat_pk01_Beanie",
        "hat_pk02_PirateHat"
    };

    private static readonly string[] ConfuseVisors =
    {
        "visor_animesunglassesVisor",
        "visor_D2CGoggles",
        "visor_pk01_PaperMaskVisor",
        "visor_Scar",
        "visor_eliksni"
    };

    private static readonly string[] ConfuseSkins =
    {
        "skin_Hazmat-Greenskin",
        "skin_Science",
        "skin_Mech",
        "skin_SuitW",
        "skin_rhm",
        "skin_screamghostface"
    };

    public static void ConfuseNowCheat()
    {
        if (!Utils.isPlayer || PlayerControl.LocalPlayer == null) return;

        var local = PlayerControl.LocalPlayer;

        local.RpcSetHat(ConfuseHats[UnityEngine.Random.Range(0, ConfuseHats.Length)]);
        local.RpcSetVisor(ConfuseVisors[UnityEngine.Random.Range(0, ConfuseVisors.Length)]);
        local.RpcSetSkin(ConfuseSkins[UnityEngine.Random.Range(0, ConfuseSkins.Length)]);
    }

    public static void AutoKillCheat()
    {
        if (!CheatToggles.autoKill || !Utils.isPlayer) return;
        if (PlayerControl.LocalPlayer.Data.Role.TeamType != RoleTeamTypes.Impostor) return;

        _autoKillTimer -= Time.deltaTime;
        if (_autoKillTimer > 0f) return;
        _autoKillTimer = 0.25f;

        PlayerControl closest = null;
        float minDistance = float.MaxValue;
        Vector2 localPosition = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player == PlayerControl.LocalPlayer || player.Data == null || player.Data.IsDead) continue;

            float distance = Vector2.Distance(localPosition, player.GetTruePosition());
            if (distance >= minDistance) continue;

            closest = player;
            minDistance = distance;
        }

        if (closest != null)
        {
            Utils.MurderPlayer(closest, MurderResultFlags.Succeeded);
        }
    }

    public static void NoClipCheat()
    {
        try
        {
            PlayerControl.LocalPlayer.Collider.enabled = !(CheatToggles.noClip || PlayerControl.LocalPlayer.onLadder);
        } catch { }
    }

    public static void PlayScannerCheat()
    {
        if (CheatToggles.animMedScan && !_isScanAnimActive)
        {
            Utils.ForceSetScanner(PlayerControl.LocalPlayer, true);
            _isScanAnimActive = true;
        }
        else if (!CheatToggles.animMedScan && _isScanAnimActive)
        {
            Utils.ForceSetScanner(PlayerControl.LocalPlayer, false);
            _isScanAnimActive = false;
        }
    }

    public static void PlayAnimationCheat()
    {
        if (CheatToggles.animPet && Utils.isPlayer && PlayerControl.LocalPlayer.cosmetics != null && PlayerControl.LocalPlayer.cosmetics.CurrentPet != null)
        {
            RpcPetMessage rpcMessage = new(PlayerControl.LocalPlayer.MyPhysics.NetId,
                PlayerControl.LocalPlayer.cosmetics.CurrentPet.PettingPlayerPosition,
                PlayerControl.LocalPlayer.cosmetics.CurrentPet.transform.position);
            AmongUsClient.Instance.LateBroadcastReliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
        }

        byte mapId = Utils.GetCurrentMapID();

        if (mapId == byte.MaxValue) return;

        var map = (MapNames)mapId;

        if (CheatToggles.animShields)
        {
            if (map is MapNames.Skeld or MapNames.Dleks)
            {
                Utils.ForcePlayAnimation((byte)TaskTypes.PrimeShields);
            }
            CheatToggles.animShields = false;
        }

        if (CheatToggles.animAsteroids)
        {
            if (map is MapNames.Skeld or MapNames.Dleks or MapNames.Polus)
            {
                Utils.ForcePlayAnimation((byte)TaskTypes.ClearAsteroids);
            }
            else
            {
                CheatToggles.animAsteroids = false;
            }
        }

        if (CheatToggles.animEmptyGarbage)
        {
            if (map is MapNames.Skeld or MapNames.Dleks)
            {
                Utils.ForcePlayAnimation((byte)TaskTypes.EmptyGarbage);
            }

            CheatToggles.animEmptyGarbage = false;
        }

        if (map is not (MapNames.MiraHQ or MapNames.Fungle))
        {
            if (CheatToggles.animCamsInUse && !_isCamsAnimActive)
            {
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 1);
                _isCamsAnimActive = true;
            }
            else if (!CheatToggles.animCamsInUse && _isCamsAnimActive)
            {
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 0);
                _isCamsAnimActive = false;
            }
        }
        else
        {
            CheatToggles.animCamsInUse = false;
        }
    }

    public static void StopShipAnimCheats()
    {
        CheatToggles.animShields = false;
        CheatToggles.animAsteroids = false;
        CheatToggles.animEmptyGarbage = false;
        CheatToggles.animMedScan = false;
        CheatToggles.animCamsInUse = false;

        _isCamsAnimActive = false;
        _isScanAnimActive = false;
    }

    public static void InvisibilityCheat()
    {
        if (!Utils.isPlayer || PlayerControl.LocalPlayer == null) return;

        try
        {
            if (CheatToggles.invisibility)
            {
                if (!_isInvisibleActive)
                {
                    _invisibilityOriginalPosition = PlayerControl.LocalPlayer.GetTruePosition();
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(999f, 999f));
                    
                    List<PlayerControl> alivePlayersExceptLocal = new();
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (!player.AmOwner && !player.Data.IsDead)
                        {
                            alivePlayersExceptLocal.Add(player);
                        }
                    }

                    if (alivePlayersExceptLocal.Count > 0)
                    {
                        PlayerControl randomPlayer = alivePlayersExceptLocal[UnityEngine.Random.Range(0, alivePlayersExceptLocal.Count)];
                        Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(randomPlayer);
                        PlayerControl.LocalPlayer.moveable = false;
                    }

                    _isInvisibleActive = true;
                }
            }
            else if (_isInvisibleActive)
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(_invisibilityOriginalPosition);
                Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.moveable = true;
                
                _isInvisibleActive = false;
            }
        }
        catch { }
    }
}

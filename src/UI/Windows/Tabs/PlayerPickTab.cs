using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using UnityEngine;

namespace TenkaiMenu;

public class PlayerPickTab : ITab
{
    public string name => "Player Pick";

    private Vector2 _playerScroll;
    private Vector2 _actionScroll;
    private static PlayerControl _target;
    private string _drawErrorMessage = string.Empty;
    private bool _drawError;

    public void Draw()
    {
        if (PlayerControl.AllPlayerControls == null || AmongUsClient.Instance == null)
        {
            GUILayout.Label("No players available.", GUIStylePreset.TabSubtitle);
            return;
        }

        float panelHeight = MenuUI.windowHeight - 120f;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.22f));
        _playerScroll = GUILayout.BeginScrollView(_playerScroll, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUILayout.Height(panelHeight));

        if (PlayerControl.AllPlayerControls.Count == 0)
        {
            GUILayout.Label("No players were found.", GUIStylePreset.TabSubtitle);
        }
        else
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null) continue;

                // Save old style color context
                Color oldColor = GUI.backgroundColor;

                // Dark Crimson if selected target, Vibrant Red if unselected target list item
                GUI.backgroundColor = (_target == player) ? new Color(0.45f, 0.05f, 0.08f, 1f) : new Color(0.85f, 0.1f, 0.15f, 1f);
                
                if (GUILayout.Button(player.Data.PlayerName, GUIStylePreset.NormalButton, GUILayout.Height(30f)))
                {
                    _target = player;
                    _drawError = false;
                    _drawErrorMessage = string.Empty;
                }
                
                // Immediately restore context
                GUI.backgroundColor = oldColor;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(10f);

        GUILayout.BeginVertical();
        _actionScroll = GUILayout.BeginScrollView(_actionScroll, GUILayout.Height(panelHeight));

        if (_target == null || PlayerControl.AllPlayerControls == null || !PlayerControl.AllPlayerControls.Contains(_target) || _target.Data == null)
        {
            _target = null;
            GUILayout.Label("Select a player to view actions", GUIStylePreset.TabSubtitle);
        }
        else if (_drawError)
        {
            GUILayout.Label("Player view failed:", GUIStylePreset.TabSubtitle);
            GUILayout.Label(_drawErrorMessage);
            
            if (DrawRedButton("Clear selection", null, GUILayout.Height(30f)))
            {
                _target = null;
                _drawError = false;
                _drawErrorMessage = string.Empty;
            }
        }
        else
        {
            try
            {
                DrawSelectedPlayer(_target);
            }
            catch (Exception ex)
            {
                _drawError = true;
                _drawErrorMessage = ex.Message;
                _target = null;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawSelectedPlayer(PlayerControl player)
    {
        if (player == null || player.Data == null)
        {
            _target = null;
            return;
        }

        GUILayout.Label("Player Info:", GUIStylePreset.TabSubtitle);

        ClientData clientData = Utils.getClientByPlayer(player);
        bool isHost = AmongUsClient.Instance != null && player.OwnerId == AmongUsClient.Instance.HostId;
        string roleColor = player.Data.RoleType == RoleTypes.Impostor ? "red" : "#8afcfc";
        string hostLine = isHost ? "\n<color=#00ffcc>Host: True</color>" : string.Empty;
        string info = $"Name: {player.Data.PlayerName} {player.Data.ColorName}\n<color={roleColor}>Role: {player.Data.RoleType}</color>\nState: {(player.Data.IsDead ? "Dead" : "Alive")}\nLevel: {player.Data.PlayerLevel}";

        if (clientData != null)
        {
            info += $"\nDevice: {clientData.PlatformData.Platform}";
        }

        info += $"\nFriendCode: {player.Data.FriendCode}\nPUID: {player.Data.Puid}" + hostLine;
        GUILayout.Label(info, GUIStylePreset.TabSubtitle);

        GUILayout.Space(10f);
        GUILayout.Label("Actions", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        if (DrawRedButton("VoteKick", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
        {
            Utils.voteKick(player.Data);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (DrawRedButton("Copy Outfit", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
        {
            CopyOutfit(player);
        }
        if (DrawRedButton("Copy Level", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
        {
            CopyLevel(player);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (DrawRedButton("Teleport To", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
        {
            if (PlayerControl.LocalPlayer != null && player != null)
            {
                try
                {
                    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(player.transform.position);
                }
                catch { }
            }
        }
        if (DrawRedButton("Report Player", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
        {
            if (player.Data.IsDead)
            {
                try
                {
                    PlayerControl.LocalPlayer?.CmdReportDeadBody(player.Data);
                }
                catch { }
            }
            else
            {
                if (HudManager.Instance != null && HudManager.Instance.Notifier != null)
                {
                    HudManager.Instance.Notifier.AddDisconnectMessage("Selected player isn't dead. Can't report body.");
                }
            }
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        string whisperButtonText = ChatMimic_RpcSendChat_Patch.whisperTarget == player ? "Stop Whisper" : "Whisper";
        if (DrawRedButton(whisperButtonText, GUIStylePreset.NormalButton, GUILayout.Height(30f)))
        {
            if (ChatMimic_RpcSendChat_Patch.whisperTarget == player)
            {
                ChatMimic_RpcSendChat_Patch.whisperTarget = null;
            }
            else
            {
                ChatMimic_RpcSendChat_Patch.whisperTarget = player;
            }
        }
        string followButtonText = CheatToggles.followSelectedPlayer && CheatToggles.followSelectedPlayerId == player.PlayerId ? "Stop Follow" : "Follow Selected";
        if (DrawRedButton(followButtonText, GUIStylePreset.NormalButton, GUILayout.Height(30f)))
        {
            if (CheatToggles.followSelectedPlayer && CheatToggles.followSelectedPlayerId == player.PlayerId)
            {
                CheatToggles.followSelectedPlayer = false;
                CheatToggles.followSelectedPlayerId = -1;
            }
            else
            {
                CheatToggles.followSelectedPlayer = true;
                CheatToggles.followSelectedPlayerId = player.PlayerId;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        string destroyButtonText = CheatToggles.destroyInGame && CheatToggles.destroyInGamePlayerId == player.PlayerId ? "Stop Destroy [In-game]" : "Destroy [In-game]";
        if (DrawRedButton(destroyButtonText, GUIStylePreset.NormalButton, GUILayout.Height(30f)))
        {
            if (CheatToggles.destroyInGame && CheatToggles.destroyInGamePlayerId == player.PlayerId)
            {
                CheatToggles.destroyInGame = false;
                CheatToggles.destroyInGamePlayerId = -1;
            }
            else
            {
                CheatToggles.destroyInGame = true;
                CheatToggles.destroyInGamePlayerId = player.PlayerId;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);

        if (Utils.isHost)
        {
            GUILayout.Label("Host Actions", GUIStylePreset.TabSubtitle);

            GUILayout.BeginHorizontal();
            if (DrawRedButton("Kick", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
            {
                HostCheats.KickOrBan(player, false);
            }
            if (DrawRedButton("Ban", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
            {
                HostCheats.KickOrBan(player, true);
            }
            if (DrawRedButton("Eject", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
            {
                HostCheats.EjectPlayer(player);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (DrawRedButton("Force Meeting", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
            {
                HostCheats.ForceMeeting(player);
            }
            if (DrawRedButton("Self Report", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
            {
                HostCheats.SelfReport(player);
            }
            if (DrawRedButton("Kill Player", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
            {
                try
                {
                    Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
                }
                catch { }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (DrawRedButton("Copy Outfit", GUIStylePreset.NormalButton, GUILayout.Height(30f)))
            {
                CopyOutfit(player);
            }
            GUILayout.EndHorizontal();
        }
    }

    private void CopyOutfit(PlayerControl player)
    {
        if (player == null || player.Data == null) return;

        var outfit = player.Data.Outfits[PlayerOutfitType.Default];
        if (outfit == null) return;

        var local = PlayerControl.LocalPlayer;
        if (local == null) return;

        try
        {
            local.RpcSetHat(outfit.HatId ?? string.Empty);
            local.RpcSetSkin(outfit.SkinId ?? string.Empty);
            local.RpcSetVisor(outfit.VisorId ?? string.Empty);
            local.RpcSetPet(outfit.PetId ?? string.Empty);
            local.RpcSetNamePlate(outfit.NamePlateId ?? string.Empty);
        }
        catch { }
    }

    private void CopyLevel(PlayerControl player)
    {
        if (player == null || player.Data == null) return;

        uint newLevel = player.Data.PlayerLevel + 1;
        TenkaiMenu.spoofLevel.Value = newLevel.ToString();
        TenkaiMenu.spoofLevel.ConfigFile.Save();
        TenkaiSpoof.SpoofLevel();
    }

    // Specialized Custom Helper to Inject Red Action Theme Designs
    private bool DrawRedButton(string text, GUIStyle style = null, params GUILayoutOption[] options)
    {
        Color oldColor = GUI.backgroundColor;
        
        // Premium bright action red configuration
        GUI.backgroundColor = new Color(0.85f, 0.1f, 0.15f, 1f);
        
        bool isClicked;
        if (style != null)
        {
            isClicked = GUILayout.Button(text, style, options);
        }
        else
        {
            isClicked = GUILayout.Button(text, options);
        }
        
        GUI.backgroundColor = oldColor;
        return isClicked;
    }
}

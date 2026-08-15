using UnityEngine;
using AmongUs.GameOptions;

namespace TenkaiMenu;

public class HostOnlyTab : ITab
{
    private const float HostControlButtonWidth = 140f;

    public string name => "Host-Only";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        // Left Side Column (Width: 42.5% of window)
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawMurder();

        GUILayout.Space(15);

        DrawGameState();

        GUILayout.Space(15);

        DrawMeetings();

        GUILayout.Space(15);
        // Lobby Controls & Colors moved to left column for easier access
        DrawLobbyControlsLeft();

        GUILayout.Space(15);
        GUILayout.EndVertical();

        // 30px horizontal safety margin to keep things perfectly centered
        GUILayout.Space(30);

        // Right Side Column (Width: 50% of window)
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.50f));

        // Right column intentionally left for other controls

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    // New left-column layout for Lobby Controls (with +/- style controls)
    private void DrawLobbyControlsLeft()
    {
        GUILayout.Label("Lobby Controls", GUIStylePreset.TabSubtitle);

        // Impostor controls with +/- and apply
        var normalOpt = GameOptionsManager.Instance?.currentNormalGameOptions;
        int curImps = normalOpt != null ? normalOpt.NumImpostors : 0;

        GUILayout.BeginHorizontal();
        Color old = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(20)))
        {
            HostCheats.DecreaseImpostors(1);
        }
        GUI.backgroundColor = old;

        GUILayout.Label($"Impostors: {curImps}", GUILayout.Width(110));

        GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(20)))
        {
            HostCheats.IncreaseImpostors(1);
        }
        GUI.backgroundColor = old;

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Apply", GUILayout.Width(80), GUILayout.Height(24)))
        {
            CheatToggles.noOptionsLimits = true;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Speed controls with +/- and apply
        float curSpeed = normalOpt != null ? normalOpt.PlayerSpeedMod : 1f;

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(20)))
        {
            HostCheats.DecreasePlayerSpeed(0.25f);
        }
        GUI.backgroundColor = old;

        GUILayout.Label($"Speed: {curSpeed:0.00}", GUILayout.Width(110));

        GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(20)))
        {
            HostCheats.IncreasePlayerSpeed(0.25f);
        }
        GUI.backgroundColor = old;

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Apply", GUILayout.Width(80), GUILayout.Height(24)))
        {
            CheatToggles.noOptionsLimits = true;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Custom seeker count controls
        int curSeekers = CheatToggles.seekersCount;
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(20)))
        {
            CheatToggles.seekersCount = Mathf.Max(1, CheatToggles.seekersCount - 1);
        }
        GUI.backgroundColor = old;

        GUILayout.Label($"Seekers: {curSeekers}", GUILayout.Width(110));

        GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(20)))
        {
            CheatToggles.seekersCount = Mathf.Min(15, CheatToggles.seekersCount + 1);
        }
        GUI.backgroundColor = old;

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Apply", GUILayout.Width(80), GUILayout.Height(24)))
        {
            HostCheats.ApplyCustomSeekers();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (DrawGreenButton("Apply 0cd Setting"))
        {
            HostCheats.ApplyZeroCdSetting();
        }

        GUILayout.Space(10);

        GUILayout.Label("Lobby Colors", GUIStylePreset.TabSubtitle);

        // Match the same size as other green buttons by using default width
        if (DrawGreenButton("Every1 Same Color"))
        {
            HostCheats.SetEveryoneSameColor();
        }

        if (DrawGreenButton("Change Color"))
        {
            HostCheats.CycleLobbyColor();
        }

        string discoText = CheatToggles.lobbyDiscoMode ? "Disco Mode: ON" : "Disco Mode: OFF";
        if (DrawGreenButton(discoText))
        {
            CheatToggles.lobbyDiscoMode = !CheatToggles.lobbyDiscoMode;
        }

        GUILayout.Space(10);
    }

    private void DrawGeneral()
    {
        CheatToggles.killVanished = DrawPillToggle(CheatToggles.killVanished, "Kill While Vanished");

        CheatToggles.killAnyone = DrawPillToggle(CheatToggles.killAnyone, "Kill Anyone");

        bool reversedSkeld = DrawPillToggle(CheatToggles.reversedSkeld, "Reversed Skeld");
        if (reversedSkeld != CheatToggles.reversedSkeld)
        {
            CheatToggles.reversedSkeld = reversedSkeld;
            HostCheats.SetReversedSkeld(reversedSkeld);
        }

        CheatToggles.noKillCd = DrawPillToggle(CheatToggles.noKillCd, "No Kill Cooldown");

        CheatToggles.showProtectMenu = DrawPillToggle(CheatToggles.showProtectMenu, "Show Protect Menu");

        bool showAssignRoleMenu = DrawPillToggle(CheatToggles.showAssignRoleMenu, "Open Assign Role Menu");
        if (showAssignRoleMenu != CheatToggles.showAssignRoleMenu)
        {
            CheatToggles.showAssignRoleMenu = showAssignRoleMenu;
        }

        bool alwaysImpostor = DrawPillToggle(CheatToggles.alwaysImpostor, "Always Imposter");
        if (alwaysImpostor != CheatToggles.alwaysImpostor)
        {
            CheatToggles.alwaysImpostor = alwaysImpostor;
            HostCheats.SetAlwaysImpostor(alwaysImpostor);
        }
    }

    private void DrawMurder()
    {
        GUILayout.Label("Murder", GUIStylePreset.TabSubtitle);

        CheatToggles.killPlayer = DrawPillToggle(CheatToggles.killPlayer, "Kill Player");

        CheatToggles.telekillPlayer = DrawPillToggle(CheatToggles.telekillPlayer, "Telekill Player");

         if (DrawGreenButton("Kill All Crewmates"))
        {
            CheatToggles.killAllCrew = true;
        }

        if (DrawGreenButton("Kill All Impostors"))
        {
            CheatToggles.killAllImps = true;
        }

        if (DrawGreenButton("Kill Everyone"))
        {
            CheatToggles.killAll = true;
        }
    }

    private void DrawGameState()
    {
        GUILayout.Label("Game State", GUIStylePreset.TabSubtitle);

        CheatToggles.noOptionsLimits = DrawPillToggle(CheatToggles.noOptionsLimits, "No Options Limits");

        if (DrawGreenButton("Force Start Game"))
        {
            CheatToggles.forceStartGame = true;
        }

        CheatToggles.noGameEnd = DrawPillToggle(CheatToggles.noGameEnd, "No Game End");

        if (DrawGreenButton("End Game Immediately"))
        {
            HostCheats.EndGameImmediately();
        }

        if (DrawGreenButton("Destroy Lobby"))
        {
            HostCheats.DestroyLobby();
        }

        if (DrawGreenButton("Recreate Lobby"))
        {
            HostCheats.RecreateLobby();
        }

        CheatToggles.levelFarm = DrawPillToggle(CheatToggles.levelFarm, "Level Farm");
    }

    private void DrawHostControls()
    {
        GUILayout.Label("Lobby Controls", GUIStylePreset.TabSubtitle);

        if (DrawGreenButton("Increase Impostors", HostControlButtonWidth))
        {
            HostCheats.IncreaseImpostors();
        }

        if (DrawGreenButton("Decrease Impostors", HostControlButtonWidth))
        {
            HostCheats.DecreaseImpostors();
        }

        GUILayout.Space(5);

        if (DrawGreenButton("Increase Speed", HostControlButtonWidth))
        {
            HostCheats.IncreasePlayerSpeed();
        }

        if (DrawGreenButton("Decrease Speed", HostControlButtonWidth))
        {
            HostCheats.DecreasePlayerSpeed();
        }

        if (DrawGreenButton("Apply 0cd Setting", HostControlButtonWidth))
        {
            HostCheats.ApplyZeroCdSetting();
        }

        GUILayout.Space(15);
        GUILayout.Label("Lobby Colors", GUIStylePreset.TabSubtitle);

        if (DrawGreenButton("Every1 Same Color", HostControlButtonWidth))
        {
            HostCheats.SetEveryoneSameColor();
        }

        if (DrawGreenButton("Change Color", HostControlButtonWidth))
        {
            HostCheats.CycleLobbyColor();
        }

        // Space-saving Button Toggle System for Disco Mode
        string discoText = CheatToggles.lobbyDiscoMode ? "Disco Mode: ON" : "Disco Mode: OFF";
        if (DrawGreenButton(discoText, HostControlButtonWidth))
        {
            CheatToggles.lobbyDiscoMode = !CheatToggles.lobbyDiscoMode;
        }

        GUILayout.Space(10);
    }

    private void DrawMeetings()
    {
        GUILayout.Label("Meetings", GUIStylePreset.TabSubtitle);
        
        if (DrawGreenButton("Skip Meeting"))
        {
            CheatToggles.skipMeeting = true;
        }

        CheatToggles.voteImmune = DrawPillToggle(CheatToggles.voteImmune, "Vote Immune");

        CheatToggles.ejectPlayer = DrawPillToggle(CheatToggles.ejectPlayer, "Eject Player");

        CheatToggles.voteLockEnabled = DrawPillToggle(CheatToggles.voteLockEnabled, "Enable VoteLock");
        
        CheatToggles.disableReportsAndMeetings = DrawPillToggle(CheatToggles.disableReportsAndMeetings, "Report/Meetings Off");

        if (Utils.isHost)
        {
            CheatToggles.spawnMeetingHud = DrawPillToggle(CheatToggles.spawnMeetingHud, "Spawn Meeting HUD");
        }
    }

    // Upgraded Custom Helper to support optional sizing constraints
    private bool DrawGreenButton(string text, float width = 0f)
    {
        Color oldBtnColor = GUI.backgroundColor;
        
        // Premium green color config injection
        GUI.backgroundColor = new Color(0.1f, 0.75f, 0.2f, 1f); 
        
        bool isClicked;
        if (width > 0f)
        {
            isClicked = GUILayout.Button(text, GUILayout.Width(width));
        }
        else
        {
            isClicked = GUILayout.Button(text);
        }
        
        // Immediately restore old color config
        GUI.backgroundColor = oldBtnColor;
        
        return isClicked;
    }

    // Bulletproof Dashboard Pill Toggle
    private bool DrawPillToggle(bool value, string text)
    {
        GUILayout.BeginHorizontal();
        
        GUILayout.Label(text);
        
        GUILayout.FlexibleSpace();
        
        Color oldColor = GUI.backgroundColor;
        
        GUI.backgroundColor = value ? new Color(1f, 0f, 0.5f, 1f) : new Color(0f, 0.45f, 0.9f, 1f);
        
        if (GUILayout.Button(value ? "ON" : "OFF", GUILayout.Width(55), GUILayout.Height(20)))
        {
            value = !value;
        }
        
        GUI.backgroundColor = oldColor;
        
        GUILayout.EndHorizontal();
        return value;
    }
}

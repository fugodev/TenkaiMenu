using UnityEngine;
using System;

namespace TenkaiMenu;

public class MovementTab : ITab
{
    public string name => "Player";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawPlayer();

        GUILayout.Space(15);

        DrawGeneral();

        GUILayout.Space(15);

        DrawAnimations();

        GUILayout.Space(15);

        DrawTeleport();

        GUILayout.EndVertical();
    }

    private void DrawPlayer()
    {
        GUILayout.Label("Player", GUIStylePreset.TabSubtitle);

        // Converted to Green Action Button
        if (DrawGreenButton(" Self Ban Glitch"))
        {
            Utils.SelfBanGlitch();
        }

        // Converted to Pill Toggles
        CheatToggles.movePlayerNonHost = DrawPillToggle(CheatToggles.movePlayerNonHost, "Move Me [Left-Click Drag]");
        CheatToggles.movePlayerMethod2 = DrawPillToggle(CheatToggles.movePlayerMethod2, "Move Me [Method 2]");
        CheatToggles.copyOutfit = DrawPillToggle(CheatToggles.copyOutfit, "Copy Outfit");
        CheatToggles.copyLevel = DrawPillToggle(CheatToggles.copyLevel, "Copy Level");
        CheatToggles.invisibility = DrawPillToggle(CheatToggles.invisibility, "Invisibility");

        bool immortality = DrawPillToggle(CheatToggles.immortality, "Immortality");
        if (immortality != CheatToggles.immortality)
        {
            CheatToggles.immortality = immortality;
            ImmortalityPatches.Enabled = immortality;
        }
    }

    private void DrawAnimations()
    {
        GUILayout.Label("Animations", GUIStylePreset.TabSubtitle);

        CheatToggles.animShields = DrawPillToggle(CheatToggles.animShields, "Shields");
        CheatToggles.animAsteroids = DrawPillToggle(CheatToggles.animAsteroids, "Asteroids");
        CheatToggles.animEmptyGarbage = DrawPillToggle(CheatToggles.animEmptyGarbage, "Empty Garbage");
        CheatToggles.animMedScan = DrawPillToggle(CheatToggles.animMedScan, "Medbay Scan");
        CheatToggles.animCamsInUse = DrawPillToggle(CheatToggles.animCamsInUse, "Cams In Use");
        CheatToggles.moonWalk = DrawPillToggle(CheatToggles.moonWalk, "Moonwalk");

        bool sonicModeEnabled = DrawPillToggle(CheatToggles.gameSpeed == 3f && CheatToggles.speedHackEnabled, "Sonic Mode");
        if (sonicModeEnabled != (CheatToggles.gameSpeed == 3f && CheatToggles.speedHackEnabled))
        {
            CheatToggles.speedHackEnabled = true;
            CheatToggles.gameSpeed = sonicModeEnabled ? 3f : 1f;
            if (!sonicModeEnabled)
            {
                Time.timeScale = 1f;
            }
        }
    }

    private void DrawGeneral()
    {
        // Converted to Pill Toggles
        CheatToggles.noClip = DrawPillToggle(CheatToggles.noClip, "No Clips");
        CheatToggles.invertControls = DrawPillToggle(CheatToggles.invertControls, "Invert Controls");

        // Speed Boost Pill Toggle with speed scaling logic hook intact
        bool speedHackEnabled = DrawPillToggle(CheatToggles.speedHackEnabled, "Speed Boost");
        if (speedHackEnabled != CheatToggles.speedHackEnabled)
        {
            CheatToggles.speedHackEnabled = speedHackEnabled;
            if (!speedHackEnabled)
            {
                Time.timeScale = 1f;
            }
        }

        if (CheatToggles.speedHackEnabled)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Game Speed:", GUILayout.Width(100f));
            CheatToggles.gameSpeed = GUILayout.HorizontalSlider(CheatToggles.gameSpeed, 0.1f, 3f);
            GUILayout.Label($"{CheatToggles.gameSpeed:F2}x", GUILayout.Width(40f));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        GUILayout.Space(5);

        try
        {
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                GUILayout.Label($"Ghost Speed: {PlayerControl.LocalPlayer?.MyPhysics.GhostSpeed:F2}");
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = GUILayout.HorizontalSlider(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, 0f, 20f, GUILayout.Width(250f));
                Utils.SnapSpeedToDefault(0.05f, true);
                if (Utils.IsSpeedDefault(true))
                    GUILayout.Label("(Default Speed)");
            }
            else
            {
                GUILayout.Label($"Player Speed: {PlayerControl.LocalPlayer?.MyPhysics.Speed:F2}");
                PlayerControl.LocalPlayer.MyPhysics.Speed = GUILayout.HorizontalSlider(PlayerControl.LocalPlayer.MyPhysics.Speed, 0f, 20f, GUILayout.Width(250f));
                Utils.SnapSpeedToDefault(0.05f);
                if (Utils.IsSpeedDefault())
                    GUILayout.Label("(Default Speed)");
            }
        } catch (NullReferenceException) {}

        GUILayout.Space(10);

        // Converted to Green Action Button
        if (DrawGreenButton(" Randomize Outfit"))
        {
            TenkaiCheats.ConfuseNowCheat();
        }

        // Converted to Pill Toggles
        CheatToggles.autoKill = DrawPillToggle(CheatToggles.autoKill, "Auto Killing");
        CheatToggles.autoReport = DrawPillToggle(CheatToggles.autoReport, "Auto Report");

        if (CheatToggles.autoReport)
        {
            GUILayout.Space(5);
            GUILayout.Label("Report range:", GUILayout.Width(100f));
            CheatToggles.autoReportRange = GUILayout.HorizontalSlider(CheatToggles.autoReportRange, CheatToggles.autoReportDefaultRange, CheatToggles.autoReportNormalMaxRange + 1f, GUILayout.Width(230f));

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Default: {CheatToggles.autoReportDefaultRange:F1}", GUILayout.Width(120f));
            GUILayout.FlexibleSpace();
            GUILayout.Label(CheatToggles.autoReportRange > CheatToggles.autoReportNormalMaxRange ? "Current: Anywhere" : $"Current: {CheatToggles.autoReportRange:F1}", GUILayout.Width(90f));
            GUILayout.EndHorizontal();
        }
    }

    private void DrawTeleport()
    {
        GUILayout.Label("Teleport", GUIStylePreset.TabSubtitle);

        // Converted to Pill Toggles
        CheatToggles.teleportCursor = DrawPillToggle(CheatToggles.teleportCursor, "to cursor");
        CheatToggles.teleportPlayer = DrawPillToggle(CheatToggles.teleportPlayer, "to player");
    }

    // Custom Helper for Reusable Green Action Buttons
    private bool DrawGreenButton(string text)
    {
        Color oldBtnColor = GUI.backgroundColor;
        
        // Apply premium green background style tint
        GUI.backgroundColor = new Color(0.1f, 0.75f, 0.2f, 1f); 
        
        bool isClicked = GUILayout.Button(text);
        
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

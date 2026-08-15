using UnityEngine;

namespace TenkaiMenu;

public class RolesTab : ITab
{
    public string name => "Roles";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        // Consolidated layout: All submenus are now stacked inside this single left-aligned column
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawImpostor();

        GUILayout.Space(15);

        DrawShapeshifter();

        GUILayout.Space(15);

        DrawCrewmate();

        GUILayout.Space(15);

        DrawTracker();

        GUILayout.Space(15);

        DrawEngineer();

        GUILayout.Space(15);

        DrawScientist();

        GUILayout.Space(15);

        DrawDetective();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.setFakeRole = DrawPillToggle(CheatToggles.setFakeRole, "Set Fake Role");

        CheatToggles.setFakeAlive = DrawPillToggle(CheatToggles.setFakeAlive, "Set Fake Alive");
    }

    private void DrawImpostor()
    {
        GUILayout.Label("Impostor", GUIStylePreset.TabSubtitle);

        CheatToggles.killReach = DrawPillToggle(CheatToggles.killReach, "Kill Reach");

        CheatToggles.killOtherImpostors = DrawPillToggle(CheatToggles.killOtherImpostors, "Kill Other Impostors");
    }

    private void DrawShapeshifter()
    {
        GUILayout.Label("Shapeshifter", GUIStylePreset.TabSubtitle);

        CheatToggles.noShapeshiftAnim = DrawPillToggle(CheatToggles.noShapeshiftAnim, "No Ss Animation");

        CheatToggles.endlessSsDuration = DrawPillToggle(CheatToggles.endlessSsDuration, "Endless Ss Duration");
    }

    private void DrawCrewmate()
    {
        GUILayout.Label("Crewmate", GUIStylePreset.TabSubtitle);

        CheatToggles.showTasksMenu = DrawPillToggle(CheatToggles.showTasksMenu, "Show Tasks Menu");
    }

    private void DrawTracker()
    {
        GUILayout.Label("Tracker", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessTracking = DrawPillToggle(CheatToggles.endlessTracking, "Endless Tracking");

        CheatToggles.noTrackingDelay = DrawPillToggle(CheatToggles.noTrackingDelay, "No Track Delay");

        CheatToggles.noTrackingCooldown = DrawPillToggle(CheatToggles.noTrackingCooldown, "No Track Cooldown");

        CheatToggles.trackReach = DrawPillToggle(CheatToggles.trackReach, "Track Reach");
    }

    private void DrawEngineer()
    {
        GUILayout.Label("Engineer", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessVentTime = DrawPillToggle(CheatToggles.endlessVentTime, "Endless Vent Time");

        CheatToggles.noVentCooldown = DrawPillToggle(CheatToggles.noVentCooldown, "No Vent Cooldown");
    }

    private void DrawScientist()
    {
        GUILayout.Label("Scientist", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessBattery = DrawPillToggle(CheatToggles.endlessBattery, "Endless Battery");

        CheatToggles.noVitalsCooldown = DrawPillToggle(CheatToggles.noVitalsCooldown, "No Vitals Cooldown");
    }

    private void DrawDetective()
    {
        GUILayout.Label("Detective", GUIStylePreset.TabSubtitle);

        CheatToggles.interrogateReach = DrawPillToggle(CheatToggles.interrogateReach, "Interrogate Reach");
    }

    // Bulletproof Dashboard Pill Toggle
    private bool DrawPillToggle(bool value, string text)
    {
        GUILayout.BeginHorizontal();
        
        // 1. Render the feature name on the left side
        GUILayout.Label(text);
        
        // 2. Automatically push the button all the way to the right edge of the column
        GUILayout.FlexibleSpace();
        
        // Save old background style tint
        Color oldColor = GUI.backgroundColor;
        
        // 3. Set background color (Hot Pink/Red if ON, Sleek Blue if OFF)
        GUI.backgroundColor = value ? new Color(1f, 0f, 0.5f, 1f) : new Color(0f, 0.45f, 0.9f, 1f);
        
        // 4. Create a clean pill button that flips the value instantly when clicked
        if (GUILayout.Button(value ? "ON" : "OFF", GUILayout.Width(55), GUILayout.Height(20)))
        {
            value = !value;
        }
        
        // Restore color configurations for subsequent components
        GUI.backgroundColor = oldColor;
        
        GUILayout.EndHorizontal();
        return value;
    }
}

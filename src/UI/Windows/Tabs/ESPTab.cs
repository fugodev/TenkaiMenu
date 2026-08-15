using UnityEngine;

namespace TenkaiMenu;

public class ESPTab : ITab
{
    public string name => "ESP";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        // Consolidated layout: everything now stacks inside this single column block
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawCamera();

        GUILayout.Space(15);

        DrawTracers();

        GUILayout.Space(15);

        DrawMinimap();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.seePlayerInfo = DrawPillToggle(CheatToggles.seePlayerInfo, "See Player Info");

        CheatToggles.seeRoles = DrawPillToggle(CheatToggles.seeRoles, "See Roles");

        CheatToggles.seeGhosts = DrawPillToggle(CheatToggles.seeGhosts, "See Ghosts");

        CheatToggles.noShadows = DrawPillToggle(CheatToggles.noShadows, "No Shadows");

        CheatToggles.taskArrows = DrawPillToggle(CheatToggles.taskArrows, "Task Arrows");

        CheatToggles.revealVotes = DrawPillToggle(CheatToggles.revealVotes, "Reveal Votes");

        CheatToggles.seeLobbyInfo = DrawPillToggle(CheatToggles.seeLobbyInfo, "See Lobby Info");
    }

    private void DrawCamera()
    {
        GUILayout.Label("Camera", GUIStylePreset.TabSubtitle);

        CheatToggles.zoomOut = DrawPillToggle(CheatToggles.zoomOut, "Zoom Out");

        CheatToggles.spectate = DrawPillToggle(CheatToggles.spectate, "Spectate");

        CheatToggles.freecam = DrawPillToggle(CheatToggles.freecam, "Freecam");
    }

    private void DrawTracers()
    {
        GUILayout.Label("Tracers", GUIStylePreset.TabSubtitle);

        CheatToggles.tracersCrew = DrawPillToggle(CheatToggles.tracersCrew, "Crewmates");

        CheatToggles.tracersImps = DrawPillToggle(CheatToggles.tracersImps, "Impostors");

        CheatToggles.tracersGhosts = DrawPillToggle(CheatToggles.tracersGhosts, "Ghosts");

        CheatToggles.tracersBodies = DrawPillToggle(CheatToggles.tracersBodies, "Dead Bodies");

        CheatToggles.colorBasedTracers = DrawPillToggle(CheatToggles.colorBasedTracers, "Color-based");

        CheatToggles.distanceBasedTracers = DrawPillToggle(CheatToggles.distanceBasedTracers, "Distance-based");
    }

    private void DrawMinimap()
    {
        GUILayout.Label("Minimap", GUIStylePreset.TabSubtitle);

        CheatToggles.mapCrew = DrawPillToggle(CheatToggles.mapCrew, "Crewmates");

        CheatToggles.mapImps = DrawPillToggle(CheatToggles.mapImps, "Impostors");

        CheatToggles.mapGhosts = DrawPillToggle(CheatToggles.mapGhosts, "Ghosts");

        CheatToggles.colorBasedMap = DrawPillToggle(CheatToggles.colorBasedMap, "Color-based");
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

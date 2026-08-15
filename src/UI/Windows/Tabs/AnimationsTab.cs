using UnityEngine;

namespace TenkaiMenu;

public class AnimationsTab : ITab
{
    public string name => "Animations";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawClientSided();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        // Converted all visual effect triggers into persistent Pill Toggles
        CheatToggles.animShields = DrawPillToggle(CheatToggles.animShields, "Shields");

        CheatToggles.animAsteroids = DrawPillToggle(CheatToggles.animAsteroids, "Asteroids");

        CheatToggles.animEmptyGarbage = DrawPillToggle(CheatToggles.animEmptyGarbage, "Empty Garbage");

        CheatToggles.animMedScan = DrawPillToggle(CheatToggles.animMedScan, "Medbay Scan");

        CheatToggles.animCamsInUse = DrawPillToggle(CheatToggles.animCamsInUse, "Cams In Use");
    }

    private void DrawClientSided()
    {
        GUILayout.Label("Client-Sided", GUIStylePreset.TabSubtitle);

        // Moonwalk remains a persistent Pill Toggle
        CheatToggles.moonWalk = DrawPillToggle(CheatToggles.moonWalk, "Moonwalk");
    }

    // Bulletproof Dashboard Pill Toggle
    private bool DrawPillToggle(bool value, string text)
    {
        GUILayout.BeginHorizontal();
        
        // Render the feature name on the left side
        GUILayout.Label(text);
        
        // Automatically push the button all the way to the right edge of the column
        GUILayout.FlexibleSpace();
        
        // Save old background style tint
        Color oldColor = GUI.backgroundColor;
        
        // Set background color (Hot Pink/Red if ON, Sleek Blue if OFF)
        GUI.backgroundColor = value ? new Color(1f, 0f, 0.5f, 1f) : new Color(0f, 0.45f, 0.9f, 1f);
        
        // Create a clean pill button that flips the value instantly when clicked
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

using UnityEngine;

namespace TenkaiMenu;

public class ConsoleTab : ITab
{
    public string name => "Console";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        // Converted logging and visibility settings to modern Pill Toggles
        CheatToggles.showConsole = DrawPillToggle(CheatToggles.showConsole, "Show Console");

        CheatToggles.logDeaths = DrawPillToggle(CheatToggles.logDeaths, "Log Deaths");

        CheatToggles.logShapeshifts = DrawPillToggle(CheatToggles.logShapeshifts, "Log Shapeshifts");

        CheatToggles.logVents = DrawPillToggle(CheatToggles.logVents, "Log Vents");
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

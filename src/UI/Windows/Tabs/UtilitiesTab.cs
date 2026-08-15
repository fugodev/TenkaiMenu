using UnityEngine;

namespace TenkaiMenu;

public class UtilitiesTab : ITab
{
    public string name => "Utilities";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawConfig();

        GUILayout.Space(15);

        DrawModes();

        GUILayout.Space(15);

        DrawChatSubmenu();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        // Remaining features converted to Pill Toggles
        CheatToggles.freeCosmetics = DrawPillToggle(CheatToggles.freeCosmetics, "Free Cosmetics");

        CheatToggles.avoidPenalties = DrawPillToggle(CheatToggles.avoidPenalties, "Avoid Penalties");

        CheatToggles.unlockFeatures = DrawPillToggle(CheatToggles.unlockFeatures, "Unlock Extra Features");

        CheatToggles.copyLobbyCodeOnDisconnect = DrawPillToggle(CheatToggles.copyLobbyCodeOnDisconnect, "Copy Lobby Code on Disconnect");
        CheatToggles.showLobbyTimer = DrawPillToggle(CheatToggles.showLobbyTimer, "Show Lobby Timer");

        CheatToggles.spoofAprilFoolsDate = DrawPillToggle(CheatToggles.spoofAprilFoolsDate, "Spoof Date to April 1st");
    }

    private void DrawConfig()
    {
        GUILayout.Label("Config", GUIStylePreset.TabSubtitle);

        // Entire Config submenu converted to Green Action Buttons
        if (DrawGreenButton("Open Config"))
        {
            CheatToggles.openConfig = true;
        }

        if (DrawGreenButton("Reload Config"))
        {
            CheatToggles.reloadConfig = true;
        }

        if (DrawGreenButton("Save to Profile"))
        {
            CheatToggles.saveProfile = true;
        }

        if (DrawGreenButton("Load from Profile"))
        {
            CheatToggles.loadProfile = true;
        }
    }

    private void DrawModes()
    {
        GUILayout.Label("Modes", GUIStylePreset.TabSubtitle);

        // RGB Mode remains a Pill Toggle
        CheatToggles.rgbMode = DrawPillToggle(CheatToggles.rgbMode, "RGB Mode");

        // Panic Mode converted to Green Action Button
        if (DrawGreenButton("Panic Mode"))
        {
            CheatToggles.panicMode = true;
        }
    }

    private void DrawChatSubmenu()
    {
        GUILayout.Label("Chats", GUIStylePreset.TabSubtitle);

        CheatToggles.enableChat = DrawPillToggle(CheatToggles.enableChat, "Enable Chat");
        CheatToggles.bypassUrlBlock = DrawPillToggle(CheatToggles.bypassUrlBlock, "Bypass URL Block");
        CheatToggles.lowerRateLimits = DrawPillToggle(CheatToggles.lowerRateLimits, "Lower Rate Limits");

        GUILayout.Space(10);

        GUILayout.Label("Textbox", GUIStylePreset.TabSubtitle);
        CheatToggles.unlockCharacters = DrawPillToggle(CheatToggles.unlockCharacters, "Unlock Extra Characters");
        CheatToggles.longerMessages = DrawPillToggle(CheatToggles.longerMessages, "Allow Longer Messages");
        CheatToggles.unlockClipboard = DrawPillToggle(CheatToggles.unlockClipboard, "Unlock Clipboard");
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
using UnityEngine;

namespace TenkaiMenu;

public class ShipTab : ITab
{
    public string name => "Ship";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        // Stacking all features sequentially inside the unified column block
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawSabotage();

        GUILayout.Space(15);

        DrawVents();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.unfixableLights = DrawPillToggle(CheatToggles.unfixableLights, "Unfixable Lights");

        bool glitchLobbyEngine = DrawPillToggle(CheatToggles.glitchLobbyEngine, "Glitch Lobby Engine");
        if (glitchLobbyEngine != CheatToggles.glitchLobbyEngine)
        {
            CheatToggles.glitchLobbyEngine = glitchLobbyEngine;
            HostCheats.SetLobbyEngineGlitch(glitchLobbyEngine);
        }

        if (DrawGreenButton(" Force Meeting [In-Game]"))
        {
            try
            {
                PlayerControl.LocalPlayer?.CmdReportDeadBody(null);
            }
            catch { }
        }

        if (DrawGreenButton(" Call Meeting"))
        {
            CheatToggles.callMeeting = true;
        }

        if (DrawGreenButton(" Destroy Lobby"))
        {
            HostCheats.DestroyLobby();
        }

        if (DrawGreenButton(" Close Meeting"))
        {
            CheatToggles.closeMeeting = true;
        }

        CheatToggles.autoOpenDoorsOnUse = DrawPillToggle(CheatToggles.autoOpenDoorsOnUse, "Auto-Open Doors On Use");
    }

    private void DrawSabotage()
    {
        GUILayout.Label("Sabotage", GUIStylePreset.TabSubtitle);

        //Note: The following buttons are designed to toggle sabotage features on and off. First "Button Name" is off state, second is on state. The text remains the same for clarity.
        // Reactor
        if (DrawGreenButton(CheatToggles.reactorSab ? "Reactor" : "Reactor"))
        {
            CheatToggles.reactorSab = !CheatToggles.reactorSab;
        }

        // Oxygen
        if (DrawGreenButton(CheatToggles.oxygenSab ? "Oxygen" : "Oxygen"))
        {
            CheatToggles.oxygenSab = !CheatToggles.oxygenSab;
        }

        // Lights
        if (DrawGreenButton(CheatToggles.elecSab ? "Lights" : "Lights"))
        {
            CheatToggles.elecSab = !CheatToggles.elecSab;
        }

        // Comms
        if (DrawGreenButton(CheatToggles.commsSab ? "Comms" : "Comms"))
        {
            CheatToggles.commsSab = !CheatToggles.commsSab;
        }

            CheatToggles.showDoorsMenu = DrawPillToggle(CheatToggles.showDoorsMenu, "Show Doors Menu");

            // Close current room doors where the player is located
        if (DrawGreenButton(" Close Current Doors"))
        {
            CheatToggles.closeCurrentDoors = true;
        }

        // Mushroom Mixup
        if (DrawGreenButton(CheatToggles.mushSab ? "Mushroom Mixup" : "Mushroom Mixup"))
        {
            CheatToggles.mushSab = !CheatToggles.mushSab;
        }

        // Trigger Spores
        if (DrawGreenButton(CheatToggles.mushSpore ? "Trigger Spores" : "Trigger Spores"))
        {
            CheatToggles.mushSpore = !CheatToggles.mushSpore;
        }

            CheatToggles.doorHallucinationAll = DrawPillToggle(CheatToggles.doorHallucinationAll, "Door Hallucination All");

        bool blockSabotages = DrawPillToggle(CheatToggles.blockSabotages, "Block Sabotages");
        if (blockSabotages != CheatToggles.blockSabotages)
        {
            CheatToggles.blockSabotages = blockSabotages;
            ShipTrollPatches.BlockSabotages.Enabled = blockSabotages;
        }

        if (DrawGreenButton(" Open Sabotage Map"))
        {
            CheatToggles.sabotageMap = true;
        }  
    }

    private void DrawVents()
    {
        GUILayout.Label("Vents", GUIStylePreset.TabSubtitle);

        CheatToggles.unlockVents = DrawPillToggle(CheatToggles.unlockVents, "Unlock Vents");
        bool disableVents = DrawPillToggle(CheatToggles.disableVents, "Disable Vents");
        if (disableVents != CheatToggles.disableVents)
        {
            CheatToggles.disableVents = disableVents;
            ShipTrollPatches.DisableVents.Enabled = disableVents;
        }

        if (DrawGreenButton(" Kick All From Vents"))
        {
            CheatToggles.kickVents = true;
        }

        CheatToggles.walkInVents = DrawPillToggle(CheatToggles.walkInVents, "Walk In Vents");

        if (DrawGreenButton(" TP Everyone to Vent"))
        {
            TenkaiCheats.TeleportAllToVent();
        }

        CheatToggles.spamTpAll = DrawPillToggle(CheatToggles.spamTpAll, "Spam TP All");
    }
    
    // Custom Helper for Reusable Green Action Buttons
    private bool DrawGreenButton(string text)
    {
        Color oldBtnColor = GUI.backgroundColor;
        
        // Apply the premium green background style tint
        GUI.backgroundColor = new Color(0.1f, 0.75f, 0.2f, 1f); 
        
        bool isClicked = GUILayout.Button(text);
        
        // Immediately restore the old color config
        GUI.backgroundColor = oldBtnColor;
        
        return isClicked;
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
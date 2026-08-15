using UnityEngine;

namespace TenkaiMenu;

public class AnticheatTab : ITab
{
    public string name => "Anticheat";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        GUILayout.Label("Anticheat", GUIStylePreset.TabSubtitle);

        CheatToggles.anticheatEnabled = DrawPillToggle(CheatToggles.anticheatEnabled, "Enable Anticheat");
        CheatToggles.anticheatDetectPlayerLevels = DrawPillToggle(CheatToggles.anticheatDetectPlayerLevels, "Detected player levels");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Detected level above: {CheatToggles.anticheatDetectPlayerLevelAbove}", GUILayout.Width(220));
        float detectedLevelAboveSlider = GUILayout.HorizontalSlider(CheatToggles.anticheatDetectPlayerLevelAbove, 100f, 10000f, GUILayout.Width(140));
        CheatToggles.anticheatDetectPlayerLevelAbove = Mathf.Clamp(Mathf.RoundToInt(detectedLevelAboveSlider), 100, 10000);
        GUILayout.EndHorizontal();
        CheatToggles.anticheatKickPlayerLevels = DrawPillToggle(CheatToggles.anticheatKickPlayerLevels, "Kick player levels");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Kick player level below: Lv{CheatToggles.anticheatKickPlayerLevelBelow}", GUILayout.Width(220));
        float kickLevelBelowSlider = GUILayout.HorizontalSlider(CheatToggles.anticheatKickPlayerLevelBelow, 1f, 100f, GUILayout.Width(140));
        CheatToggles.anticheatKickPlayerLevelBelow = Mathf.Clamp(Mathf.RoundToInt(kickLevelBelowSlider), 1, 100);
        GUILayout.EndHorizontal();

        CheatToggles.flagSpoofedPlatformData = DrawPillToggle(CheatToggles.flagSpoofedPlatformData, "Detect spoofed platform");

        GUILayout.Space(10);
        GUILayout.Label("Actions");
        CheatToggles.anticheatSendNotification = DrawPillToggle(CheatToggles.anticheatSendNotification, "Send Notification");
        CheatToggles.anticheatDiscardRpc = DrawPillToggle(CheatToggles.anticheatDiscardRpc, "Block RPC");

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Punishment");
        GUILayout.FlexibleSpace();

        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.3f, 0.2f, 0.75f, 1f);

        if (GUILayout.Button(((AnticheatGuard.PenaltyMode)CheatToggles.anticheatPunishment).ToString(), GUILayout.Width(120), GUILayout.Height(20)))
        {
            CheatToggles.anticheatPunishment = (CheatToggles.anticheatPunishment + 1) % 4;
        }

        GUI.backgroundColor = oldColor;
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

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

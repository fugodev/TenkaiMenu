using System.Collections.Generic;
using UnityEngine;

namespace TenkaiMenu;

public class SettingsTab : ITab
{
    public string name => "Settings";

    private string _menuKeybindInput = "";
    private string _menuColorInput = "";
    private string _menuWidthInput = "";
    private string _menuHeightInput = "";
    private string _spoofLevelInput = "";
    private string _spoofPlatformInput = "";
    private bool _initialized;
    private float _cursorBlinkTime = 0.5f;

    private readonly Dictionary<string, bool> _focusedFields = new();
    private readonly Dictionary<string, float> _lastBlinkTime = new();
    private readonly Dictionary<string, bool> _cursorVisible = new();
    private readonly Dictionary<string, Rect> _fieldRects = new();

    public void Draw()
    {
        if (!_initialized)
        {
            InitializeInputFields();
            _initialized = true;
        }

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGUISettings();

        GUILayout.Space(15);

        DrawSpoofingSettings();

        GUILayout.Space(15);

        DrawPrivacySettings();

        GUILayout.EndVertical();
    }

    private void InitializeInputFields()
    {
        _menuKeybindInput = TenkaiMenu.menuKeybind.Value ?? "";
        _menuColorInput = TenkaiMenu.menuHtmlColor.Value ?? "";
        _menuWidthInput = TenkaiMenu.menuWidth.Value.ToString();
        _menuHeightInput = TenkaiMenu.menuHeight.Value.ToString();
        _spoofLevelInput = TenkaiMenu.spoofLevel.Value ?? "";
        _spoofPlatformInput = TenkaiMenu.spoofPlatform.Value ?? "";

        _focusedFields["menuKeybind"] = false;
        _focusedFields["menuColor"] = false;
        _focusedFields["menuWidth"] = false;
        _focusedFields["menuHeight"] = false;
        _focusedFields["spoofLevel"] = false;
        _focusedFields["spoofPlatform"] = false;

        _cursorVisible["menuKeybind"] = true;
        _cursorVisible["menuColor"] = true;
        _cursorVisible["menuWidth"] = true;
        _cursorVisible["menuHeight"] = true;
        _cursorVisible["spoofLevel"] = true;
        _cursorVisible["spoofPlatform"] = true;

        _lastBlinkTime["menuKeybind"] = 0f;
        _lastBlinkTime["menuColor"] = 0f;
        _lastBlinkTime["menuWidth"] = 0f;
        _lastBlinkTime["menuHeight"] = 0f;
        _lastBlinkTime["spoofLevel"] = 0f;
        _lastBlinkTime["spoofPlatform"] = 0f;
    }

    // Green button drawing helper
    private bool DrawGreenButton(string text, float width, float height = 20f)
    {
        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.18f, 0.8f, 0.44f); // Green tint
        bool clicked = GUILayout.Button(text, GUILayout.Width(width), GUILayout.Height(height));
        GUI.backgroundColor = oldColor;
        return clicked;
    }

    private void HandleCustomTextField(ref string content, string fieldKey, float width = 120f, float height = 20f)
    {
        GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(height));

        if (Event.current.type == EventType.Repaint)
        {
            _fieldRects[fieldKey] = GUILayoutUtility.GetLastRect();
        }

        if (!_focusedFields.ContainsKey(fieldKey))
        {
            _focusedFields[fieldKey] = false;
        }

        if (Event.current.type == EventType.MouseDown && _fieldRects.ContainsKey(fieldKey))
        {
            if (_fieldRects[fieldKey].Contains(Event.current.mousePosition))
            {
                _focusedFields[fieldKey] = true;
                _lastBlinkTime[fieldKey] = Time.time;
                _cursorVisible[fieldKey] = true;
                Event.current.Use();
            }
            else
            {
                _focusedFields[fieldKey] = false;
            }
        }

        if (_focusedFields[fieldKey] && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Backspace)
            {
                if (content.Length > 0)
                {
                    content = content.Substring(0, content.Length - 1);
                    Event.current.Use();
                }
            }
            else if (Event.current.character != '\0' && !char.IsControl(Event.current.character))
            {
                content += Event.current.character.ToString();
                Event.current.Use();
            }
        }

        if (_fieldRects.ContainsKey(fieldKey))
        {
            GUI.Label(new Rect(_fieldRects[fieldKey].x + 5f, _fieldRects[fieldKey].y + 1f, _fieldRects[fieldKey].width - 10f, _fieldRects[fieldKey].height), content);
            if (_focusedFields[fieldKey])
            {
                if (!_lastBlinkTime.ContainsKey(fieldKey))
                {
                    _lastBlinkTime[fieldKey] = Time.time;
                }

                if (Time.time - _lastBlinkTime[fieldKey] > _cursorBlinkTime)
                {
                    _cursorVisible[fieldKey] = !_cursorVisible[fieldKey];
                    _lastBlinkTime[fieldKey] = Time.time;
                }

                if (_cursorVisible[fieldKey])
                {
                    Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(content));
                    GUI.Label(new Rect(_fieldRects[fieldKey].x + textSize.x + 7f, _fieldRects[fieldKey].y + 1f, 10f, _fieldRects[fieldKey].height - 4f), "|");
                }
            }
        }
    }

    // Pill toggle with the button first (leading) for Settings tab
    private bool DrawPillToggleLeading(bool value, string text)
    {
        GUILayout.BeginHorizontal();

        // Save old background color
        Color oldColor = GUI.backgroundColor;

        // Set toggle tint based on state
        GUI.backgroundColor = value ? new Color(1f, 0f, 0.5f, 1f) : new Color(0f, 0.45f, 0.9f, 1f);

        // Draw pill-style button first
        if (GUILayout.Button(value ? "ON" : "OFF", GUILayout.Width(55), GUILayout.Height(20)))
        {
            value = !value;
        }

        // Restore color for label
        GUI.backgroundColor = oldColor;

        GUILayout.Space(8f);
        GUILayout.Label(text);

        GUILayout.EndHorizontal();
        return value;
    }

    private void DrawGUISettings()
    {
        GUILayout.Label("GUI Settings", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Menu Keybind:", GUILayout.Width(140f), GUILayout.Height(20f));
        HandleCustomTextField(ref _menuKeybindInput, "menuKeybind", 120f, 20f);
        if (DrawGreenButton("Apply", 70f, 20f))
        {
            TenkaiMenu.menuKeybind.Value = _menuKeybindInput;
            TenkaiMenu.menuKeybind.ConfigFile.Save();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Custom Menu Color:", GUILayout.Width(140f), GUILayout.Height(20f));
        HandleCustomTextField(ref _menuColorInput, "menuColor", 120f, 20f);
        if (DrawGreenButton("Apply", 70f, 20f))
        {
            TenkaiMenu.menuHtmlColor.Value = _menuColorInput;
            TenkaiMenu.menuHtmlColor.ConfigFile.Save();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.Label("Menu Size", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Menu Width:", GUILayout.Width(140f), GUILayout.Height(20f));
        HandleCustomTextField(ref _menuWidthInput, "menuWidth", 120f, 20f);
        if (DrawGreenButton("Apply", 70f, 20f))
        {
            if (int.TryParse(_menuWidthInput, out int width) && width >= 300)
            {
                TenkaiMenu.menuWidth.Value = width;
                MenuUI.ApplyMenuSize(width, TenkaiMenu.menuHeight.Value);
                TenkaiMenu.menuWidth.ConfigFile.Save();
            }
            else
            {
                _menuWidthInput = TenkaiMenu.menuWidth.Value.ToString();
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Menu Height:", GUILayout.Width(140f), GUILayout.Height(20f));
        HandleCustomTextField(ref _menuHeightInput, "menuHeight", 120f, 20f);
        if (DrawGreenButton("Apply", 70f, 20f))
        {
            if (int.TryParse(_menuHeightInput, out int height) && height >= 300)
            {
                TenkaiMenu.menuHeight.Value = height;
                MenuUI.ApplyMenuSize(TenkaiMenu.menuWidth.Value, height);
                TenkaiMenu.menuHeight.ConfigFile.Save();
            }
            else
            {
                _menuHeightInput = TenkaiMenu.menuHeight.Value.ToString();
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Menu Font Size:", GUILayout.Width(140f), GUILayout.Height(20f));
        float newFontSize = GUILayout.HorizontalSlider(TenkaiMenu.menuFontSize.Value, 8f, 24f, GUILayout.Width(120f));
        GUILayout.Label($"{TenkaiMenu.menuFontSize.Value:F0}px", GUILayout.Width(50f));
        GUILayout.EndHorizontal();
        if (Mathf.Abs(newFontSize - TenkaiMenu.menuFontSize.Value) > 0.01f)
        {
            TenkaiMenu.menuFontSize.Value = newFontSize;
            MenuUI.ApplyMenuFontSize();
            TenkaiMenu.menuFontSize.ConfigFile.Save();
        }

        GUILayout.Space(5);

        TenkaiMenu.menuOpenOnMouse.Value = DrawPillToggleLeading(TenkaiMenu.menuOpenOnMouse.Value, "Open Menu on Mouse Position");

        GUILayout.Space(5);

        TenkaiMenu.autoLoadProfile.Value = DrawPillToggleLeading(TenkaiMenu.autoLoadProfile.Value, "Auto-Load Profile on Startup");
    }

    private void DrawSpoofingSettings()
    {
        GUILayout.Label("Spoofing Settings", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spoof Level (1-100001):", GUILayout.Width(140f), GUILayout.Height(20f));
        HandleCustomTextField(ref _spoofLevelInput, "spoofLevel", 120f, 20f);
        if (DrawGreenButton("Apply", 70f, 20f))
        {
            if (int.TryParse(_spoofLevelInput, out int level) && level >= 1 && level <= 100001)
            {
                TenkaiMenu.spoofLevel.Value = _spoofLevelInput;
                TenkaiMenu.spoofLevel.ConfigFile.Save();
            }
            else
            {
                _spoofLevelInput = TenkaiMenu.spoofLevel.Value;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spoof Platform:", GUILayout.Width(140f), GUILayout.Height(20f));
        HandleCustomTextField(ref _spoofPlatformInput, "spoofPlatform", 120f, 20f);
        if (DrawGreenButton("Apply", 70f, 20f))
        {
            TenkaiMenu.spoofPlatform.Value = _spoofPlatformInput;
            TenkaiMenu.spoofPlatform.ConfigFile.Save();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.Label("Supported Platforms: Epicgames, Steam, Mac, StandaloneWin10, etc.");
    }

    private void DrawPrivacySettings()
    {
        GUILayout.Label("Privacy Settings", GUIStylePreset.TabSubtitle);

        TenkaiMenu.spoofDeviceId.Value = DrawPillToggleLeading(TenkaiMenu.spoofDeviceId.Value, "Hide Device ID");

        GUILayout.Space(5);

        TenkaiMenu.noTelemetry.Value = DrawPillToggleLeading(TenkaiMenu.noTelemetry.Value, "Disable Telemetry");

        GUILayout.Space(10);

        if (DrawGreenButton("Open Config File", 200f, 24f))
        {
            Utils.OpenConfigFile();
        }

        GUILayout.Space(5);
        GUILayout.Label("For more advanced configuration options, click 'Open Config File'", GUIStylePreset.TabSubtitle);
    }
}
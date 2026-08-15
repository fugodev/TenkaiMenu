using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace TenkaiMenu;

public class MenuUI : MonoBehaviour
{
    public static int windowHeight = 584;
    public static int windowWidth = 600;
    private Rect _windowRect;
    public static Rect currentWindowRect;

    public static bool isGUIActive = false;
    public static bool isMouseOverMenu = false;
    private List<ITab> _tabs = new();
    private List<Vector2> _tabScrollPositions = new();
    private int _selectedTab;
    private bool _lastSpawnMeetingHudState;
    public static float hue; // For RGB mode

    // 1. Declare the purple background texture variable
    private static Texture2D solidPurpleTex;

    private void Start()
    {
        windowWidth = Mathf.Max(300, TenkaiMenu.menuWidth.Value);
        windowHeight = Mathf.Max(300, TenkaiMenu.menuHeight.Value);

        // Add all tabs on start
        _tabs.Add(new MovementTab());
        _tabs.Add(new ESPTab());
        _tabs.Add(new RolesTab());
        _tabs.Add(new ShipTab());
        _tabs.Add(new ExploitsTab());
        _tabs.Add(new AnticheatTab());
        _tabs.Add(new ConsoleTab());
        _tabs.Add(new HostOnlyTab());
        _tabs.Add(new UtilitiesTab());
        _tabs.Add(new PlayerPickTab());
        _tabs.Add(new SettingsTab());

        for (int i = 0; i < _tabs.Count; i++)
        {
            _tabScrollPositions.Add(Vector2.zero);
        }

        // Instantiate 2D area of MenuUI at the upper-left corner by default
        _windowRect = new(
            Mathf.Clamp(20f, 0f, Screen.width - windowWidth),
            Mathf.Clamp(40f, 0f, Screen.height - windowHeight),
            windowWidth,
            windowHeight
        );
    }

    public static void ApplyMenuSize(int width, int height)
    {
        windowWidth = Mathf.Max(300, width);
        windowHeight = Mathf.Max(300, height);

        if (TenkaiMenu.menuUI != null)
        {
            TenkaiMenu.menuUI.ResizeWindow();
        }
    }

    public static void ApplyMenuFontSize()
    {
        if (TenkaiMenu.menuUI != null)
        {
            TenkaiMenu.menuUI.InitStyles();
        }

        GUIStylePreset.RefreshStyles();
    }

    private void ResizeWindow()
    {
        _windowRect.width = windowWidth;
        _windowRect.height = windowHeight;
        _windowRect.x = Mathf.Clamp(_windowRect.x, 0f, Screen.width - _windowRect.width);
        _windowRect.y = Mathf.Clamp(_windowRect.y, 0f, Screen.height - _windowRect.height);
    }

    public void InitStyles()
    {
        int fontSize = Mathf.Max(8, Mathf.RoundToInt(TenkaiMenu.menuFontSize.Value));
        GUI.skin.toggle.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = fontSize;
        GUI.skin.textField.fontSize = fontSize;
        GUI.skin.box.fontSize = fontSize;
        GUI.skin.window.fontSize = fontSize;
        GUI.skin.scrollView.fontSize = fontSize;
        GUI.skin.verticalScrollbar.fontSize = fontSize;
        GUI.skin.horizontalScrollbar.fontSize = fontSize;
        GUIStylePreset.RefreshStyles();
    }

    private void Update()
    {
        if (isGUIActive)
        {
            currentWindowRect = _windowRect;
            Vector2 guiMousePosition = new(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            isMouseOverMenu = currentWindowRect.Contains(guiMousePosition);
        }
        else
        {
            isMouseOverMenu = false;
        }

        if (Input.GetKeyDown(Utils.StringToKeycode(TenkaiMenu.menuKeybind.Value)))
        {
            isGUIActive = !isGUIActive;
            if (TenkaiMenu.menuOpenOnMouse.Value)
            {
                Vector2 mousePosition = Input.mousePosition;
                _windowRect.position = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            }
        }

        if (CheatToggles.rgbMode)
        {
            hue += Time.deltaTime * 0.3f;
            // Adjust speed of color change, higher multiplier = faster
            if (hue > 1f) hue -= 1f;
            // Loop hue back to 0 when it exceeds 1
        }

        if (CheatToggles.panicMode) Utils.Panic();
        HostCheats.lobbyDiscoMode = CheatToggles.lobbyDiscoMode;
        HostCheats.UpdateLobbyDisco();

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = !(TenkaiMenu.inStealthMode || TenkaiMenu.isPanicked);
        if (CheatToggles.openConfig)
        {
            Utils.OpenConfigFile();
            CheatToggles.openConfig = false;
        }

        if (CheatToggles.reloadConfig)
        {
            TenkaiMenu.Plugin.Config.Reload();
            CheatToggles.reloadConfig = false;
        }

        if (CheatToggles.saveProfile)
        {
            CheatToggles.saveProfile = false;
            CheatToggles.SaveTogglesToProfile();
        }

        if (CheatToggles.loadProfile)
        {
            CheatToggles.LoadTogglesFromProfile();
            CheatToggles.loadProfile = false;
        }

        if (CheatToggles.closeCurrentDoors)
        {
            try
            {
                if (PlayerControl.LocalPlayer != null && Utils.isShip)
                {
                    DoorsHandler.CloseDoorsInRoom(Utils.GetCurrentRoom());
                }
            }
            catch { }

            CheatToggles.closeCurrentDoors = false;
        }

        // Sync Anticheat values from menu toggles to runtime logic
        AnticheatGuard.IsEnabled = CheatToggles.anticheatEnabled;
        AnticheatGuard.CheckPlatformSpoofing = CheatToggles.flagSpoofedPlatformData;
        AnticheatGuard.SendVisualAlerts = CheatToggles.anticheatSendNotification;
        AnticheatGuard.DropMaliciousPackets = CheatToggles.anticheatDiscardRpc;
        AnticheatGuard.CurrentPenalty = (AnticheatGuard.PenaltyMode)Mathf.Clamp(CheatToggles.anticheatPunishment, 0, 3);

        if (CheatToggles.spawnMeetingHud != _lastSpawnMeetingHudState)
        {
            if (CheatToggles.spawnMeetingHud)
            {
                HostCheats.SpawnMeetingHud();
            }
            else
            {
                HostCheats.CloseMeeting();
            }

            _lastSpawnMeetingHudState = CheatToggles.spawnMeetingHud;
        }

        if (CheatToggles.spawnMeetingHud && MeetingHud.Instance == null && _lastSpawnMeetingHudState)
        {
            CheatToggles.spawnMeetingHud = false;
            _lastSpawnMeetingHudState = false;
        }

        // Some cheats only work if the LocalPlayer exists, so they are turned off if it does not
        if(!Utils.isPlayer)
        {
            CheatToggles.setFakeRole = false;
            CheatToggles.setFakeAlive = false;
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.teleportPlayer = false;
            CheatToggles.spectate = false;
            CheatToggles.freecam = false;
            CheatToggles.killPlayer = false;
            CheatToggles.callMeeting = false;
            CheatToggles.copyOutfit = false;
            CheatToggles.copyLevel = false;
            CheatToggles.voteKick = false;
            CheatToggles.levelFarm = false;
            
        }

        // Some cheats only work if the ship exists, so they are turned off if it does not
        if(!Utils.isShip)
        {
            CheatToggles.sabotageMap = false;
            CheatToggles.unfixableLights = false;
            CheatToggles.completeMyTasks = false;
            CheatToggles.kickVents = false;
            CheatToggles.reportBody = false;
            CheatToggles.closeMeeting = false;
            CheatToggles.reactorSab = false;
            CheatToggles.oxygenSab = false;
            CheatToggles.commsSab = false;
            CheatToggles.elecSab = false;
            CheatToggles.mushSab = false;
            CheatToggles.closeAllDoors = false;
            CheatToggles.openAllDoors = false;
            CheatToggles.spamCloseAllDoors = false;
            CheatToggles.spamOpenAllDoors = false;
            CheatToggles.mushSpore = false;

            TenkaiCheats.StopShipAnimCheats();
        }

        if(!Utils.isHost && !Utils.isFreePlay)
        {
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.killPlayer = false;
            CheatToggles.ejectPlayer = false;
            CheatToggles.noKillCd = false;
            CheatToggles.killAnyone = false;
            CheatToggles.killVanished = false;
            CheatToggles.forceStartGame = false;
            CheatToggles.skipMeeting = false;
            CheatToggles.voteImmune = false;
            CheatToggles.noGameEnd = false;
            CheatToggles.showProtectMenu = false;
            CheatToggles.noOptionsLimits = false;
            CheatToggles.voteLockEnabled = false;
            CheatToggles.disableReportsAndMeetings = false;
            CheatToggles.endGame = false;
            CheatToggles.destroyLobby = false;
            CheatToggles.recreateLobby = false;
        }

        // Some cheats only work if in a meeting, so they are turned off if it does not
        if (!Utils.isMeeting)
        {
            CheatToggles.skipMeeting = false;
            CheatToggles.ejectPlayer = false;
        }
    }

    public void OnGUI()
    {
        if (!isGUIActive || TenkaiMenu.isPanicked) return;
        InitStyles();

        UIHelpers.ApplyUIColor();

        // 2. Generate and apply Midnight Purple with 70% opacity (0.7f Alpha)
        if (solidPurpleTex == null)
        {
            solidPurpleTex = new Texture2D(1, 1);
            // 0.7f Alpha gives it a clean transparent glass vibe while holding the deep purple hue
            solidPurpleTex.SetPixel(0, 0, new Color(0.06f, 0.03f, 0.08f, 0.7f)); 
            solidPurpleTex.Apply();
        }
        GUI.skin.window.normal.background = solidPurpleTex;
        GUI.skin.window.onNormal.background = solidPurpleTex;

        _windowRect = GUI.Window((int)WindowId.MenuUI, _windowRect, (GUI.WindowFunction)WindowFunction, "TenkaiMenu v" + TenkaiMenu.TenkaiVersion);
        currentWindowRect = _windowRect;
    }

    public void WindowFunction(int windowID)
    {
        GUILayout.BeginHorizontal();
        // Left tab selector (15% width)
        GUILayout.BeginVertical(GUILayout.Width(windowWidth * 0.15f));
        for (var i = 0; i < _tabs.Count; i++)
        {
            var style = (_selectedTab == i) ?
            GUIStylePreset.TabButtonSelected : GUIStylePreset.TabButton;
            if (GUILayout.Button(_tabs[i].name, style, GUILayout.Height(24))) // Reduced height
                _selectedTab = i;
        }
        GUILayout.EndVertical();

        // Vertical separator line + invisible space to create gap between the tab selector and the content
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
        GUILayout.Space(10f);

        // Right tab content and controls (85% width)
        GUILayout.BeginVertical(GUILayout.Width(windowWidth * 0.85f));
        // Tab-specific content
        if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
        {
            GUILayout.Label(_tabs[_selectedTab].name, GUIStylePreset.TabTitle);
            // Use a plain vertical layout for right content, no box
            GUILayout.BeginVertical(GUILayout.Width(windowWidth * 0.85f), GUILayout.Height(windowHeight - 90f));
            _tabScrollPositions[_selectedTab] = GUILayout.BeginScrollView(
                _tabScrollPositions[_selectedTab],
                false,
                true,
                GUI.skin.horizontalScrollbar,
                GUI.skin.verticalScrollbar,
                GUILayout.Width(windowWidth * 0.85f - 30f),
                GUILayout.Height(windowHeight - 110f),
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            );
            try
            {
                _tabs[_selectedTab].Draw();
            }
            catch (Exception ex)
            {
                GUILayout.Label($"Tab error: {ex.Message}", GUIStylePreset.TabSubtitle);
            }

            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        // Make the window draggable
        GUI.DragWindow();
    }
}
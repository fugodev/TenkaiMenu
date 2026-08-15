using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmongUs.GameOptions;

namespace TenkaiMenu;

public class AssignRoleUI : MonoBehaviour
{
    public static int windowHeight = 500;
    public static int windowWidth = 700;

    private Rect _windowRect;
    private Vector2 _scrollPosition = Vector2.zero;
    private readonly Dictionary<byte, int> _selectedRoleIndex = new();

    private void Start()
    {
        _windowRect = new Rect(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showAssignRoleMenu || !(MenuUI.isGUIActive || TenkaiMenu.menuKeepSubwindowsOpen.Value) || TenkaiMenu.isPanicked)
        {
            return;
        }

        UIHelpers.ApplyUIColor();
        _windowRect = GUI.Window((int)WindowId.AssignRoleUI, _windowRect, (GUI.WindowFunction)AssignRoleWindow, "Assign Roles");
    }

    private void AssignRoleWindow(int windowID)
    {
        var availableRoles = HostCheats.GetAssignableRoles().ToList();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Player", GUILayout.Width(110f));
        GUILayout.Label("Current Role", GUILayout.Width(110f));
        GUILayout.Label("Assigned", GUILayout.Width(110f));
        GUILayout.Label("Role Choice", GUILayout.Width(170f));
        GUILayout.Label("Actions", GUILayout.Width(120f));
        GUILayout.EndHorizontal();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.Role == null || string.IsNullOrEmpty(player.Data.PlayerName))
            {
                continue;
            }

            byte playerId = player.PlayerId;
            int selectedIndex = _selectedRoleIndex.TryGetValue(playerId, out var currentIndex) ? currentIndex : 0;

            if (HostCheats.RoleAssignments.TryGetValue(playerId, out RoleTypes assignedRole))
            {
                int assignedIndex = availableRoles.IndexOf(assignedRole);
                if (assignedIndex >= 0)
                {
                    selectedIndex = assignedIndex;
                }
            }

            GUILayout.BeginHorizontal(GUI.skin.box);

            GUILayout.Label($"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>", GUILayout.Width(110f));
            GUILayout.Label(player.Data.Role.Role.ToString(), GUILayout.Width(110f));
            GUILayout.Label(HostCheats.RoleAssignments.TryGetValue(playerId, out var role) ? role.ToString() : "None", GUILayout.Width(110f));

            if (availableRoles.Count > 0)
            {
                selectedIndex = Mathf.Clamp(selectedIndex, 0, availableRoles.Count - 1);
                _selectedRoleIndex[playerId] = selectedIndex;

                GUILayout.BeginHorizontal(GUILayout.Width(170f));
                if (GUILayout.Button("<", GUILayout.Width(24f), GUILayout.Height(20f)))
                {
                    selectedIndex = (selectedIndex - 1 + availableRoles.Count) % availableRoles.Count;
                    _selectedRoleIndex[playerId] = selectedIndex;
                }

                GUILayout.Label(availableRoles[selectedIndex].ToString(), GUILayout.Width(122f), GUILayout.Height(20f));

                if (GUILayout.Button(">", GUILayout.Width(24f), GUILayout.Height(20f)))
                {
                    selectedIndex = (selectedIndex + 1) % availableRoles.Count;
                    _selectedRoleIndex[playerId] = selectedIndex;
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("No roles available", GUILayout.Width(170f));
            }

            GUILayout.BeginVertical(GUILayout.Width(120f));
            if (GUILayout.Button("Assign", GUILayout.Height(20f)))
            {
                if (Utils.isHost && availableRoles.Count > 0)
                {
                    HostCheats.SetRoleAssignment(playerId, availableRoles[_selectedRoleIndex[playerId]]);
                }
            }

            if (GUILayout.Button("Clear", GUILayout.Height(20f)))
            {
                if (Utils.isHost)
                {
                    HostCheats.RemoveRoleAssignment(playerId);
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All", GUILayout.Width(180f)))
        {
            HostCheats.ClearRoleAssignments();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Close", GUILayout.Width(80f)))
        {
            CheatToggles.showAssignRoleMenu = false;
        }
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }
}

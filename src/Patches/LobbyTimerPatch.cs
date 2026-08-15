using HarmonyLib;
using UnityEngine;
using InnerNet;

namespace TenkaiMenu;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class LobbyTimerPatch
{
    private static float _lobbyTimer = 600f;
    private static int _lastGameId;

    [HarmonyPostfix]
    public static void Postfix(GameStartManager __instance)
    {
        try
        {
            if (AmongUsClient.Instance == null) return;

            if (AmongUsClient.Instance.GameId != _lastGameId)
            {
                _lobbyTimer = 600f;
                _lastGameId = AmongUsClient.Instance.GameId;
            }

            if (LobbyBehaviour.Instance != null)
            {
                _lobbyTimer -= Time.deltaTime;
            }

            if (!CheatToggles.showLobbyTimer)
            {
                if (__instance.GameRoomNameCode != null)
                {
                    __instance.GameRoomNameCode.text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                }
            }
            else
            {
                int lobbyTime = Mathf.Max(0, (int)_lobbyTimer);
                int minutes = lobbyTime / 60;
                int seconds = lobbyTime % 60;
                string timeColor = (lobbyTime > 180) ? "#ffffff" : ((lobbyTime <= 60) ? "#f00" : "#ff0");
                string timeDisplay = $" <{timeColor}>{minutes}:{(seconds < 10 ? "0" : "")}{seconds}</color>";
                string code = GameCode.IntToGameName(AmongUsClient.Instance.GameId);

                if (__instance.GameRoomNameCode != null)
                {
                    __instance.GameRoomNameCode.text = code + timeDisplay;
                }
            }
        }
        catch
        {
        }
    }
}

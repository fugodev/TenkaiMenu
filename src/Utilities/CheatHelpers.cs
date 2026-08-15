using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using InnerNet;
using AmongUs.GameOptions;
using UnityEngine;

namespace TenkaiMenu;

public static class CheatHelpers
{
    public static IGameOptions CreateCloneOptions(IGameOptions options)
    {
        if (GameManager.Instance == null || GameManager.Instance.LogicOptions == null) return options;

        byte[] bytes = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, false);
        return GameManager.Instance.LogicOptions.gameOptionsFactory.FromBytes(bytes);
    }

    public static void SendGameOptionsToClient(IGameOptions options, int targetClientId)
    {
        if (GameManager.Instance == null || AmongUsClient.Instance == null) return;

        bool isLocalTarget = AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame && PlayerControl.LocalPlayer != null && targetClientId == PlayerControl.LocalPlayer.OwnerId;
        if (isLocalTarget)
        {
            GameManager.Instance.LogicOptions.SetGameOptions(options);
            return;
        }

        byte[] bytes = GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, false);
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage((byte)FindLogicOptionsIndex());
        writer.WriteBytesAndSize(bytes);
        writer.EndMessage();
        SendDataFlag(GameManager.Instance.NetId, writer, targetClientId);
    }

    private static int FindLogicOptionsIndex()
    {
        if (GameManager.Instance == null) return -1;

        for (int i = 0; i < GameManager.Instance.LogicComponents.Count; i++)
        {
            if (GameManager.Instance.LogicComponents[i].GetType() == typeof(LogicOptions))
            {
                return i;
            }
        }

        return -1;
    }

    public static void SendDataFlag(uint netId, MessageWriter msg, int targetClientId = -1)
    {
        if (AmongUsClient.Instance == null) return;

        MessageWriter writer = MessageWriter.Get(SendOption.None);
        if (targetClientId == -1)
        {
            writer.StartMessage(Tags.GameData);
            writer.Write(AmongUsClient.Instance.GameId);
        }
        else
        {
            writer.StartMessage(Tags.GameDataTo);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.WritePacked(targetClientId);
        }

        writer.StartMessage((byte)1);
        writer.WritePacked(netId);
        writer.Write(msg, false);
        writer.EndMessage();
        writer.EndMessage();
        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }

    public static PlayerControl GetRandomPlayer(PlayerControl exclude = null)
    {
        if (PlayerControl.AllPlayerControls == null || PlayerControl.AllPlayerControls.Count == 0) return null;

        var list = new List<PlayerControl>();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null) continue;
            if (exclude != null && player == exclude) continue;
            list.Add(player);
        }

        if (list.Count == 0) return null;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}

using HarmonyLib;
using Hazel;
using TMPro;

namespace TenkaiMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
public static class ChatMimic_RpcSendChat_Patch
{
    public static PlayerControl whisperTarget;
    public static PlayerControl pendingWhisperDisplayTarget;

    public static bool Prefix(ref string chatText, PlayerControl __instance)
    {
        if (whisperTarget != null && __instance.AmOwner)
        {
            if (string.IsNullOrWhiteSpace(chatText))
            {
                return false;
            }

            int targetClientId = -1;
            try
            {
                targetClientId = AmongUsClient.Instance.GetClientIdFromCharacter(whisperTarget);
            }
            catch
            {
            }

            if (targetClientId >= 0)
            {
                if (AmongUsClient.Instance.AmClient && HudManager.Instance != null)
                {
                    pendingWhisperDisplayTarget = whisperTarget;
                    HudManager.Instance.Chat.AddChat(__instance, chatText, false);
                }

                MessageWriter mw = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 13, SendOption.Reliable, targetClientId);
                mw.Write(chatText);
                AmongUsClient.Instance.FinishRpcImmediately(mw);
                ChatMimic_RpcSendChat_Patch.whisperTarget = null;
                return false;
            }
        }

        if (CheatToggles.chatJailbreak && AmongUsClient.Instance != null && !AmongUsClient.Instance.AmHost)
        {
            chatText = chatText.Replace("<", string.Empty).Replace(">", string.Empty);
        }

        return !string.IsNullOrWhiteSpace(chatText);
    }
}

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
public static class ChatBubble_SetName_Whisper
{
    public static void Postfix(ChatBubble __instance)
    {
        if (ChatMimic_RpcSendChat_Patch.pendingWhisperDisplayTarget != null && __instance.playerInfo != null && __instance.playerInfo.Object == PlayerControl.LocalPlayer)
        {
            PlayerControl target = ChatMimic_RpcSendChat_Patch.pendingWhisperDisplayTarget;
            ChatMimic_RpcSendChat_Patch.pendingWhisperDisplayTarget = null;

            string theirName = target?.Data?.PlayerName ?? "???";
            __instance.NameText.text = $"[You] -> [{theirName}]";
            __instance.NameText.ForceMeshUpdate(true, true);
            return;
        }
    }
}

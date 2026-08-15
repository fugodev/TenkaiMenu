using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace TenkaiMenu
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class ForceMultipleSeekersPatch
    {
        public static void Postfix()
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
            if (!Utils.isHideNSeek) return;

            HostCheats.ApplyCustomSeekers();
        }
    }
}

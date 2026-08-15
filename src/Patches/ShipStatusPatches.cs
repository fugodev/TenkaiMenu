using HarmonyLib;

namespace TenkaiMenu;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class ShipStatus_FixedUpdate
{
    public static void Postfix(ShipStatus __instance)
    {
        TenkaiSabotageCheats.Process(__instance);
        TenkaiCheats.OpenSabotageMapCheat();

        TenkaiCheats.CloseMeetingCheat();
        TenkaiCheats.SkipMeetingCheat();
        TenkaiCheats.CallMeetingCheat();
        TenkaiCheats.WalkInVentCheat();
        TenkaiCheats.KickVentsCheat();

        TenkaiPPMCheats.ReportBodyPPM();
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class FungleShipStatus_FixedUpdate
{
    public static void Postfix(FungleShipStatus __instance)
    {
        TenkaiSabotageCheats.ProcessFungle(__instance);
    }
}

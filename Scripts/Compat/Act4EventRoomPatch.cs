using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MoeNegiMod.Aglaea.Acts;
using MoeNegiMod.Aglaea.Events;

namespace MoeNegiMod.Aglaea.Compat;

/// <summary>
/// 第四章的起点节点是 <see cref="MegaCrit.Sts2.Core.Map.MapPointType.Unknown"/>，
/// 原版问号房间只有约 85% 概率是事件。这里让第四章的问号**必定**是事件房间。
/// </summary>
[HarmonyPatch(typeof(UnknownMapPointOdds), nameof(UnknownMapPointOdds.Roll))]
internal static class Act4UnknownPointAlwaysEventPatch
{
    [HarmonyPrefix]
    private static bool Prefix(IRunState runState, ref RoomType __result)
    {
        if (runState.Act is not AglaeaZandarAct)
        {
            return true;
        }

        __result = RoomType.Event;
        return false;
    }
}

/// <summary>
/// 第四章只有「往昔的涟漪」这一个事件节点；把该章节的事件抽取固定为它，
/// 避免和原版共享事件池混在一起随机。
/// </summary>
[HarmonyPatch(typeof(ActModel), nameof(ActModel.PullNextEvent))]
internal static class Act4FixedRippleEventPatch
{
    [HarmonyPrefix]
    private static bool Prefix(ActModel __instance, RunState runState, ref EventModel __result)
    {
        if (__instance is not AglaeaZandarAct)
        {
            return true;
        }

        EventModel evt = ModelDb.Event<AglaeaRippleEvent>();
        runState.AddVisitedEvent(evt);
        __result = evt;
        return false;
    }
}

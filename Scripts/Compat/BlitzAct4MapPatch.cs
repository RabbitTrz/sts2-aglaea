using System;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MoeNegiMod.Aglaea.Acts;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Compat;

/// <summary>
/// 极速模式（Blitz）下的第四章地图。
///
/// 原版极速模式对每一章都强制使用 <see cref="BlitzActMap"/>：
/// 起点 Ancient → 怪物 → 怪物 → 商店 → 精英 → 宝箱 → 休息点 → Boss。
/// 但第四章没有 Ancient、普通怪和精英（只有赞达尔 Boss），
/// 因此进入第四章时会在起点（PullAncient 返回 null）或第一个怪物节点
/// （NextNormalEncounter 对空列表取模）直接崩掉 —— 表现为“极速模式无法加载第四章”。
///
/// 这里在构造完成后把线路改短：起点（问号事件）→ 商店 → Boss，
/// 与标准模式第四章的设计一致；后续商店/Boss 的推进仍由 BlitzActMap 的直线逻辑处理。
/// </summary>
internal static class BlitzAct4MapRewire
{
    internal static void Apply(BlitzActMap map, IRunState state)
    {
        if (state.Act is not AglaeaZandarAct)
        {
            return;
        }

        try
        {
            MapPoint start = map.StartingMapPoint;
            MapPoint boss = map.BossMapPoint;
            MapPoint? shop = start.Children.FirstOrDefault();
            if (shop == null)
            {
                MainFile.Logger.Warn("[Aglaea] 极速模式第四章地图重接失败：起点没有子节点。");
                return;
            }

            // 起点改为问号（MapPointType 没有 Event，问号由
            // Act4UnknownPointAlwaysEventPatch 固定解析为「往昔的涟漪」事件）。
            start.PointType = MapPointType.Unknown;
            foreach (MapPoint child in start.Children.ToList())
            {
                if (child != shop)
                {
                    start.RemoveChildPoint(child);
                }
            }

            // 只保留一个商店节点，并直接连到 Boss；其余中间节点一并断开。
            shop.PointType = MapPointType.Shop;
            foreach (MapPoint child in shop.Children.ToList())
            {
                shop.RemoveChildPoint(child);
            }
            foreach (MapPoint parent in boss.parents.ToList())
            {
                if (parent != shop)
                {
                    parent.RemoveChildPoint(boss);
                }
            }
            shop.AddChildPoint(boss);

            MainFile.Logger.Info(
                $"[Aglaea] 极速模式第四章地图已重接为 事件 → 商店 → Boss（{map.GetMapPoints().Count} 个节点）。");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Aglaea] 极速模式第四章地图重接失败：{ex.Message}");
        }
    }
}

/// <summary>新生成极速地图时重接第四章。</summary>
[HarmonyPatch(typeof(BlitzActMap), MethodType.Constructor, new[] { typeof(IRunState) })]
internal static class BlitzAct4MapPatch
{
    [HarmonyPostfix]
    private static void Postfix(BlitzActMap __instance, IRunState state)
        => BlitzAct4MapRewire.Apply(__instance, state);
}

/// <summary>从存档恢复极速地图时也重接（兼容修复前就卡在第四章的存档）。</summary>
[HarmonyPatch(typeof(BlitzActMap), MethodType.Constructor, new[] { typeof(SerializableActMap), typeof(IRunState) })]
internal static class BlitzAct4MapSavedPatch
{
    [HarmonyPostfix]
    private static void Postfix(BlitzActMap __instance, IRunState state)
        => BlitzAct4MapRewire.Apply(__instance, state);
}

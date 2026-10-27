using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MoeNegiMod.Aglaea.Acts;
using System.Collections.Generic;
using System.Reflection.Emit;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Compat;

/// <summary>
/// 第四章地图的纵向长度压缩。
///
/// 原版地图界面把所有章节都铺在固定的纵向跨度上：
/// <c>_distY = 2325 / (行数 - 1)</c>，Boss 节点固定放在 <c>-1980</c>。
/// 我们的第四章只有「休息点 → 商店 → Boss」三个点，于是唯一一行房间被拉到 2325px，
/// 整张地图显得特别长、特别空。
///
/// 这里只对 <see cref="AglaeaZandarAct"/> 生效：把 2325 和 -1980 两个常量乘 0.5，
/// 节点和它们之间的连线（DrawPaths 用的是压缩后的坐标）一起收拢；
/// 其它章节原样返回。
/// </summary>
internal static class Act4MapLayout
{
    private const float Compression = 0.5f;

    /// <summary>当前正在绘制的地图（由 SetMap 的 prefix 写入）。</summary>
    internal static ActMap? CurrentMap;

    public static float Compress(float value)
        => CurrentMap is AglaeaZandarActMap
            ? value * Compression
            : value;
}

[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
internal static class Act4MapLengthPatch
{
    [HarmonyPrefix]
    private static void Prefix(ActMap map)
    {
        Act4MapLayout.CurrentMap = map;
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        var compress = AccessTools.Method(typeof(Act4MapLayout), nameof(Act4MapLayout.Compress));
        int replaced = 0;

        // _distY = 2325f / (rowCount - 1) * num;
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldc_R4, 2325f));
        if (!matcher.IsInvalid)
        {
            matcher.InsertAfter(new CodeInstruction(OpCodes.Call, compress));
            replaced++;
        }

        // _bossPointNode.Position = new Vector2(-200f, -1980f * num);
        matcher.Start().MatchStartForward(new CodeMatch(OpCodes.Ldc_R4, -1980f));
        if (!matcher.IsInvalid)
        {
            matcher.InsertAfter(new CodeInstruction(OpCodes.Call, compress));
            replaced++;
        }

        if (replaced < 2)
        {
            MainFile.Logger.Warn(
                $"[Aglaea] 第四章地图长度补丁未完全生效（命中 {replaced}/2 个常量）。");
        }
        else
        {
            MainFile.Logger.Info("[Aglaea] 已补丁 NMapScreen.SetMap：第四章地图纵向压缩到 50%。");
        }

        return matcher.InstructionEnumeration();
    }
}

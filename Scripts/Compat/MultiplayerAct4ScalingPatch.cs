using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Compat;

/// <summary>
/// 多人模式下第四章的怪物缩放。
///
/// 原版 <c>MultiplayerScalingModel.GetMultiplayerScaling</c> 只处理 actIndex 0/1/2，
/// 其它值直接 <c>throw ArgumentOutOfRangeException("Invalid act index for HP scaling")</c>。
/// 而 <c>Creature.ScaleMonsterHpForMultiplayer</c> 会在**每个敌人**创建时调用它
/// （包括 Boss、DEFRAG/公理中途召唤的爪牙），所以多人进第四章 Boss 战会直接崩。
///
/// 这里只接管 actIndex >= 3：按第三幕 Boss 的 1.3 倍处理。0~2 走原版逻辑。
/// </summary>
[HarmonyPatch(typeof(MultiplayerScalingModel), nameof(MultiplayerScalingModel.GetMultiplayerScaling))]
internal static class MultiplayerAct4ScalingPatch
{
    /// <summary>第四章（以及未来章节）的多人血量倍率，对齐第三幕 Boss。</summary>
    private const decimal Act4Multiplier = 1.3m;

    [HarmonyPrefix]
    private static bool Prefix(int __1, ref decimal __result)
    {
        // __1 = actIndex
        if (__1 < 3)
        {
            return true;
        }

        __result = Act4Multiplier;
        MainFile.Logger.Info(
            $"[Aglaea] 多人第四章怪物缩放：actIndex={__1}，倍率 {Act4Multiplier}（原版会抛异常）。");
        return false;
    }
}

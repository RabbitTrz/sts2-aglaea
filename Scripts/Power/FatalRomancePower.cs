using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 致命浪漫（玩家身上的 Buff）：自己的衣匠对「处于临时力量下降效果」的敌人
/// 造成额外伤害。每 1 层使伤害倍率 +1（1 层 = 2 倍，2 层 = 3 倍，依此类推）。
///
/// 叠层写法对齐原版猎手的「追踪」（<see cref="TrackingPower"/>）：
/// 同样是 Counter 叠层，且 Amount 真正参与倍率计算，而不是只当标记。
/// 实际倍率在 <see cref="Commands.TailorCmd.AttackWith"/> 的伤害结算里读取。
/// </summary>
public sealed class FatalRomancePower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 按层数计算对符合条件的敌人的伤害倍率。
    /// Amount = 1 → ×2，Amount = 2 → ×3，依此类推（1 + 层数）。
    /// </summary>
    public static decimal GetMultiplier(int amount)
    {
        return 1m + System.Math.Max(0, amount);
    }
}

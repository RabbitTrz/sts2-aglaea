using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 衣匠额外最大生命（玩家身上的隐藏 Buff）：Amount 为本次战斗内，衣匠额外获得的最大生命值。
/// 由「诛线」「金星行列」等牌叠加，召唤新衣匠时读取此值加到其最大生命上。
/// 随战斗结束自动清理。
/// </summary>
public sealed class TailorMaxHpBonusPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

}

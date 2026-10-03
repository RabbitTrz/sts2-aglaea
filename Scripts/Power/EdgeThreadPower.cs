using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 锐丝如刃（玩家身上的 Buff）：使衣匠造成的攻击伤害提升固定数值。
/// 数值由「锐丝如刃」能力牌应用（2 点，升级 4 点）。
/// 伤害加成在 TailorCmd.GetTailorDamage 中读取 Amount。
/// </summary>
public sealed class EdgeThreadPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}

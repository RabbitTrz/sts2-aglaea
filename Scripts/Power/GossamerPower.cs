using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 脆若游丝（玩家身上的 Buff）：间隙织线的力量降低效果改为力量提升。
/// 效果在 WeaveLinePower 中读取（存在该 power 时改为施加正面力量）。
/// </summary>
public sealed class GossamerPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeaveLinePower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}

using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 衣匠上限加成（能力）：Amount 为衣匠最大数量额外增加的值。
/// 由「缠丝聚金」挂载，供给 TailorCmd 计算动态衣匠上限。
/// </summary>
public sealed class TailorLimitPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}

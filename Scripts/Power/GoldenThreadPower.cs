using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 金丝（玩家身上的 Buff）：每层使衣匠造成的攻击伤害提高 1 点。
/// 层数由「至高之姿」触发（打出攻击牌时获得1层）、以及「捻丝/触丝/缠丝聚金」等牌获得，
/// 可被「绞丝」等牌移除。
/// 该 power 是纯数值叠加层（Counter），伤害加成由 TailorCmd 计算衣匠伤害时读取。
/// </summary>
public sealed class GoldenThreadPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}

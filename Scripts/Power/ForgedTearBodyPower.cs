using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 锻泪匠躯（玩家身上的 Buff）：衣匠抵挡伤害时改为由所有衣匠均摊（差值最多 1）。
/// 仅作为标记存在，实际均摊逻辑在 TailorSwarmPower.ModifyHpLostBeforeOsty 中根据此 power 分流。
/// </summary>
public sealed class ForgedTearBodyPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}

using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 广织成网（玩家身上的 Buff）：衣匠的攻击会对所有敌人造成伤害。
/// 效果在 TailorCmd.AttackWith 中读取（存在该 power 时改为 AOE）。
/// </summary>
public sealed class WideNetPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}

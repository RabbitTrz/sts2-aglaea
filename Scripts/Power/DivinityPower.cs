using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 神性（玩家身上的 Buff，Counter 叠层）：由「半神形态」在角色攻击 N 次后获得。
/// 衣匠攻击时，消耗所有神性，每消耗一层该衣匠额外攻击一次。
/// 仅在战斗内累积，战斗结束清空。
/// </summary>
public sealed class DivinityPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}

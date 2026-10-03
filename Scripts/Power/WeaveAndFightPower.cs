using MegaCrit.Sts2.Core.Entities.Powers;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 即织即战（玩家身上的 Buff）：每当你召唤衣匠时，被召唤的衣匠自动发动 N 次攻击（N = 层数，可叠加）。
/// 该 power 仅作为层数标记存在，实际攻击逻辑在 TailorCmd.Summon 中触发
/// （衣匠通过 AddPet 添加，不走 Summon 关键字，因此 AfterSummon 不会触发）。
/// </summary>
public sealed class WeaveAndFightPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MoeNegiMod.Aglaea.Cards;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// CALL _毁灭论证法（赞达尔在论证形态期间持有的 Buff，Counter = 已累计的行动次数）：
/// 论证形态中每行动一次 +1 层；施放【求解：第一因之果】时，
/// 每层使其获得 1 点[gold]力量[/gold]，并给予所有玩家 1 层【纠缠】<see cref="EntanglePower"/>，随后移除。
///
/// 层数从 0 开始：Apply 传入的 amount 只为让 power 存在，随后在 AfterApplied 里清 0
/// （与间隙织线同一套写法）。
/// </summary>
public sealed class CallDestructionPower : AglaeaPower
{
    /// <summary>求解时每层给予的力量（2026-10-05 用户下调：1）。</summary>
    public const int StrengthPerStack = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<EntanglePower>()
    ];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SetAmount(0);
        return Task.CompletedTask;
    }
}

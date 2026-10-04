using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 金血浇铸：每回合开始时，失去等同于本能力层数的生命，获得 2 倍层数的至高之姿。
///
/// 每层 = 每回合 1 点生命 → 2 层至高之姿（即 1:2 的兑换比例）。
/// Amount 即层数，也是能力图标上显示的数字：
///   1 层 → 每回合失 1 点生命，得 2 层至高之姿
///   2 层 → 每回合失 2 点生命，得 4 层至高之姿
/// 卡牌每次打出为能力累积 1 层（见 AglaeaGiltBloodForge）。
/// </summary>
public sealed class GiltBloodForgePower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    /// <summary>每层能力对应的至高之姿层数。</summary>
    private const decimal _supremePosePerStack = 3m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner))
        {
            return;
        }

        Flash();

        // 【失去生命】而非【受到伤害】——这两者在 STS2 里是两个不同的概念：
        //   · 失去生命：Unblockable + Unpowered，不传 dealer，
        //     不吃格挡 / 力量 / 虚弱 / 易伤，也不会触发「本回合是否受到过伤害」类判定
        //     （如静息蓄势），因此不会污染「无伤回合」。
        //   · 受到伤害：由某个 dealer 造成，走完整的增伤 / 减伤 / 格挡管线。
        // 此处对齐能力型 buff 的失血写法（如 DemisePower）而非卡牌的失血写法：
        // 失血来源不是卡，故不带 ValueProp.Move、也不传 this/cardPlay。
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(), Owner, Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);

        // 1 层 = 2 层至高之姿。
        await PowerCmd.Apply<SupremePosePower>(
            new ThrowingPlayerChoiceContext(), Owner, Amount * _supremePosePerStack, Owner, null);
    }
}

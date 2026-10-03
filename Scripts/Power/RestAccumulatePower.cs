using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 静息蓄势：若一回合没有受到伤害，则在下回合开始时，获得 1 层至高之姿。
/// Amount 记录每回合开始时获得的至高之姿层数（升级后为 2）。
/// </summary>
public sealed class RestAccumulatePower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private sealed class Data
    {
        public bool TookDamageThisTurn;
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

    // 玩家实际掉血（未被[gold]格挡[/gold]）时标记本回合已受伤。
    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && result.UnblockedDamage > 0)
        {
            GetInternalData<Data>().TookDamageThisTurn = true;
        }
        return Task.CompletedTask;
    }

    // 玩家回合开始时：若上一回合（敌方回合）未受伤，则获得至高之姿。
    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner))
        {
            return;
        }

        var data = GetInternalData<Data>();
        if (!data.TookDamageThisTurn)
        {
            await PowerCmd.Apply<SupremePosePower>(
                new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
        }

        // 重置受伤标记，等待本回合（直到下个敌方回合）再次检测。
        data.TookDamageThisTurn = false;
    }
}

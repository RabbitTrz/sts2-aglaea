using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 三织：接下来 N 张攻击牌会打出 3 次（N = 层数，可叠加）。
/// 每打出一张攻击牌消耗 1 层，耗尽后移除；回合结束时也会移除（本回合限定）。
/// </summary>
public sealed class TripleNextAttackPower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (Amount <= 0 || card.Owner?.Creature != Owner || card.Type != CardType.Attack)
        {
            return playCount;
        }
        return playCount + 2;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        await PowerCmd.Decrement(this);
    }

    // 三织为「本回合」限定：玩家回合结束时移除。
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
        {
            return;
        }
        await PowerCmd.Remove(this);
    }
}

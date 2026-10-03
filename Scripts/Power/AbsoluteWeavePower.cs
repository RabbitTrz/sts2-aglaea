using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 决织（玩家身上的 Buff，本回合限定）：本回合不能再获得费用，所有攻击牌免费。
/// 回合结束时移除。
/// </summary>
public sealed class AbsoluteWeavePower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    // 禁止获得费用。
    public override decimal ModifyEnergyGain(Player player, decimal amount)
    {
        if (player.Creature != Owner)
        {
            return amount;
        }
        return 0m;
    }

    // 所有攻击牌免费。
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner?.Creature != Owner || card.Type != CardType.Attack)
        {
            return false;
        }
        modifiedCost = 0m;
        return true;
    }

    // 本回合限定：玩家回合结束时移除。
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 用 participants 而不是 side 判断：多人「额外回合」时 playersEndingTurn
        // 只包含部分玩家，用 side 会把还没结束回合的队友的「本回合」效果一并清掉。
        // 与 ArrestPower 的写法保持一致。
        if (!participants.Contains(Owner))
        {
            return;
        }
        await PowerCmd.Remove(this);
    }
}

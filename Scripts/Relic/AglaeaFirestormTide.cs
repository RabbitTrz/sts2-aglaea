using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 火力风暴潮（专属遗物，不进池）：每打出一张攻击牌，本回合内自身的攻击伤害 +5%（可叠）。
/// 由「火种」累计满 12 层时发放。
///
/// 计数只在本回合内累计，玩家回合开始时清零。
/// v0.1.3 之前的效果是「每打出 5 张攻击牌获得 1 层至高之姿」，已替换。
/// </summary>
public class AglaeaFirestormTide : AglaeaExclusiveRelic
{
    /// <summary>每张攻击牌提供的伤害加成（加法百分比，作用在乘区上）。</summary>
    private const decimal PerCardBonus = 0.05m;

    /// <summary>本回合已打出的攻击牌数。</summary>
    private int _cardsThisTurn;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只统计本次行动玩家自己的攻击牌（多人对局中避免统计他人）。
        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Player == Owner)
        {
            _cardsThisTurn++;
        }
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            _cardsThisTurn = 0;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 乘法修正，返回值是【倍率】而不是伤害值：没有加成时须返回 1。
    /// </summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (_cardsThisTurn <= 0 || dealer != Owner.Creature)
        {
            return 1m;
        }

        // 只加攻击伤害（与卡面「攻击伤害」措辞一致）。
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }

        return 1m + PerCardBonus * _cardsThisTurn;
    }
}

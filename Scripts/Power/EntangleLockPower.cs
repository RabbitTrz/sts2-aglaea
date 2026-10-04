using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 纠缠的锁（玩家身上的 Debuff，本回合限定）：本回合不能再打出手牌。
///
/// 效果与【凡庸 Normality】一致 —— 都靠覆写 <see cref="ShouldPlay"/> 让手牌打不出去。
/// 由「纠缠」被抽出第二张时挂上，玩家回合结束时移除。
/// </summary>
public sealed class EntangleLockPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Normality>()
    ];

    public override PowerType Type => PowerType.Debuff;

    // 本回合限定，不叠层。
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 与凡庸同一判定：只封【手牌】；不在手上（打牌中/自动打出）不拦。
    /// </summary>
    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (card.Owner != Owner.Player)
        {
            return true;
        }

        CardPile? pile = card.Pile;
        if (pile == null || pile.Type != PileType.Hand)
        {
            return true;
        }

        return false;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 用 participants 判断归属：多人「额外回合」时只清真正结束回合的那些玩家。
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}

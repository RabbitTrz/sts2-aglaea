using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 纬线：将抽牌堆、弃牌堆与手牌中的连携打击全部打出，并将其消耗。消耗。
/// </summary>
public class AglaeaWeftLine() : AglaeaCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<AglaeaLinkedStrike>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Player;

        // 收集手牌、抽牌堆、弃牌堆中所有连携打击。
        var linked = new List<CardModel>();
        linked.AddRange(CardPile.GetCards(player, PileType.Hand).Where(c => c is AglaeaLinkedStrike));
        linked.AddRange(CardPile.GetCards(player, PileType.Draw).Where(c => c is AglaeaLinkedStrike));
        linked.AddRange(CardPile.GetCards(player, PileType.Discard).Where(c => c is AglaeaLinkedStrike));

        foreach (var card in linked)
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
            // 打出后消耗。
            await CardCmd.Exhaust(choiceContext, card);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

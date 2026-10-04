using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 触丝：若存在衣匠，使随机衣匠攻击 1 次；从弃牌堆选择一张攻击牌放入手牌。消耗。
/// 升级后去掉消耗。
/// </summary>
public class AglaeaTouchThread() : AglaeaCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>(),
        TipExtraAttack
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 若存在衣匠，使随机一名衣匠额外攻击一次（不消耗其生命）。
        await TailorCmd.AttackOnce(choiceContext, cardPlay.Player, 3, extraAttack: true);

        // 从弃牌堆选择一张攻击牌放入手牌。
        var card = (await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: PileType.Discard.GetPile(cardPlay.Player),
            player: cardPlay.Player,
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            filter: c => c.Type == CardType.Attack)).FirstOrDefault();

        if (card != null)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

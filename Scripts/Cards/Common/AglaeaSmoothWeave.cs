using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 顺织：抽 1 张牌。若抽到的是攻击牌，则将其打出。消耗。
/// </summary>
public class AglaeaSmoothWeave() : AglaeaCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawn = await CardPileCmd.Draw(choiceContext, cardPlay.Player);
        if (drawn != null && drawn.Type == CardType.Attack)
        {
            await CardCmd.AutoPlay(choiceContext, drawn, null);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后移除消耗。
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

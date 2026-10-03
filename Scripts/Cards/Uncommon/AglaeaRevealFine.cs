using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 纤毫毕现：给予间隙织线，去除其所有[gold]格挡[/gold]，给予 2 层易伤。消耗。升级后费用 0。
/// </summary>
public class AglaeaRevealFine() : AglaeaCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeaveLinePower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        // 给予间隙织线。
        await PowerCmd.Apply<WeaveLinePower>(
            choiceContext, target, 1, cardPlay.Player.Creature, this);

        // 去除所有[gold]格挡[/gold]。
        if (target.Block > 0)
        {
            await CreatureCmd.LoseBlock(choiceContext, target, target.Block, cardPlay.Player.Creature);
        }

        // 给予 2 层易伤。
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, target, 2, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

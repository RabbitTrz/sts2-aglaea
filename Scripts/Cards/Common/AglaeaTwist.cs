using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 捻丝：若处于「至高之姿」，则获得 3 层「金丝」。消耗。升级后获得 5 层。
/// </summary>
public class AglaeaTwist() : AglaeaCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature.GetPower<SupremePosePower>() == null)
        {
            return;
        }

        await PowerCmd.Apply<GoldenThreadPower>(
            choiceContext, cardPlay.Player.Creature, DynamicVars.Gold.IntValue, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Gold.UpgradeValueBy(2m);
    }
}

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 金丝成障：回合开始时，若处于至高之姿，则获得 6 点[gold]格挡[/gold]。升级：9 点[gold]格挡[/gold]。
/// </summary>
public class AglaeaGoldBarrier() : AglaeaCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<GoldBarrierPower>("Barrier", 6m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<GoldBarrierPower>(
            choiceContext, cardPlay.Player.Creature, DynamicVars["Barrier"].IntValue, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Barrier"].UpgradeValueBy(3m);
    }
}

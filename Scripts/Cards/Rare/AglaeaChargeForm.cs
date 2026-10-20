using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Character;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 蓄姿：获得 1 费，抽 1 张牌，获得 2 层「至高之姿」。消耗。升级后获得 2 费。
/// </summary>
public class AglaeaChargeForm() : AglaeaCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        new CardsVar(1),
        new PowerVar<SupremePosePower>("Pose", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 至高之姿的登场演出：仅在层数为 0 且本场战斗还没播过时触发（不阻塞结算）。
        AglaeaVfx.PlaySupremeIntroOnce(cardPlay.Player.Creature);

        var player = cardPlay.Player;

        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, player);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, player);
        await PowerCmd.Apply<SupremePosePower>(
            choiceContext, player.Creature, DynamicVars["Pose"].IntValue, player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}

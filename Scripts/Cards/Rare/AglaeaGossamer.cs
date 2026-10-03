using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 脆若游丝：获得 3 层无实体。间隙织线的力量降低效果改为力量提升。
/// 升级后 4 层无实体。
/// </summary>
public class AglaeaGossamer() : AglaeaCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<IntangiblePower>("Intangible", 3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<IntangiblePower>(),
        TipWeaveLine
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = cardPlay.Player.Creature;

        await PowerCmd.Apply<IntangiblePower>(
            choiceContext, creature, DynamicVars["Intangible"].IntValue, creature, this);

        await PowerCmd.Apply<GossamerPower>(choiceContext, creature, 1, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Intangible"].UpgradeValueBy(1m);
    }
}

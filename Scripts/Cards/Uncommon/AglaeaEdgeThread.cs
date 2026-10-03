using BaseLib.Utils;
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
/// 锐丝如刃：衣匠攻击的伤害提升 2。升级：提升 4。
/// </summary>
public class AglaeaEdgeThread() : AglaeaCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        TipTailor
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<EdgeThreadPower>("Edge", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<EdgeThreadPower>(
            choiceContext, cardPlay.Player.Creature, DynamicVars["Edge"].IntValue, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Edge"].UpgradeValueBy(2m);
    }
}

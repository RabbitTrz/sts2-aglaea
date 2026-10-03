using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 织运之竭：造成 4 点伤害。若处于至高之姿，则给予 1 层易伤。升级：5 点伤害、2 层易伤。
/// </summary>
public class AglaeaThreadworn() : AglaeaCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move),
        new PowerVar<VulnerablePower>("Vuln", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose,
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        if (cardPlay.Player.Creature.GetPower<SupremePosePower>() != null)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext, cardPlay.Target, DynamicVars["Vuln"].IntValue, cardPlay.Player.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars["Vuln"].UpgradeValueBy(1m);
    }
}

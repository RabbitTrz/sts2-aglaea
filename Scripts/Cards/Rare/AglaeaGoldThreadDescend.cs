using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 金线降神：造成 10 点伤害。若斩杀，则获得 2 层至高之姿。升级后 12 点、3 层。
/// </summary>
public class AglaeaGoldThreadDescend() : AglaeaCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move),
        new PowerVar<SupremePosePower>("Pose", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        // 若造成斩杀，则获得至高之姿。
        if (cardPlay.Target != null && cardPlay.Target.IsDead)
        {
            await PowerCmd.Apply<SupremePosePower>(
                choiceContext, cardPlay.Player.Creature,
                DynamicVars["Pose"].IntValue, cardPlay.Player.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Pose"].UpgradeValueBy(1m);
    }
}

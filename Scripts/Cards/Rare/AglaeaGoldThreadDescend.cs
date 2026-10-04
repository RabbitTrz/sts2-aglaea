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
/// 金线降神：若处于「至高之姿」，获得 1 层「至高之姿」。造成 10 点伤害。升级后获得 2 层。
/// （v0.1.3 改动：触发条件从「若斩杀」改为「若处于至高之姿」；伤害不再随升级提升。）
/// </summary>
public class AglaeaGoldThreadDescend() : AglaeaCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move),
        new PowerVar<SupremePosePower>("Pose", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = cardPlay.Player.Creature;

        // 先叠层再结算伤害（与卡面语序一致）。
        if (creature.GetPower<SupremePosePower>() != null)
        {
            await PowerCmd.Apply<SupremePosePower>(
                choiceContext, creature, DynamicVars["Pose"].IntValue, creature, this);
        }

        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Pose"].UpgradeValueBy(1m);
    }
}

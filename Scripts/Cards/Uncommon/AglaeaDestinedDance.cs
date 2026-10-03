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
/// 命定共舞：造成 10 点伤害。若处于至高之姿，则移除所有至高之姿，每移除 1 层使本次攻击伤害加 5。
/// 升级后 12 点基础伤害、每层加 6。
/// </summary>
public class AglaeaDestinedDance() : AglaeaCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move),
        new PowerVar<SupremePosePower>("BonusPerPose", 5m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        int damage = DynamicVars.Damage.IntValue;

        // 若处于至高之姿，移除所有层并增伤。
        var pose = cardPlay.Player.Creature.GetPower<SupremePosePower>();
        if (pose != null)
        {
            int removed = pose.Amount;
            await PowerCmd.Remove<SupremePosePower>(cardPlay.Player.Creature);
            damage += removed * DynamicVars["BonusPerPose"].IntValue;
        }

        await CreatureCmd.Damage(
            choiceContext, target, damage, ValueProp.Move, cardPlay.Player.Creature);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["BonusPerPose"].UpgradeValueBy(1m);
    }
}

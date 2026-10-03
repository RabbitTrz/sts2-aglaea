using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 裂帛：造成 10 点伤害，并使其他敌人受到该伤害值一半的伤害。升级后 14 点。
/// </summary>
public class AglaeaTearSilk() : AglaeaCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        // 对其他存活的敌人造成一半伤害。
        int splash = Mathf.Max(1, DynamicVars.Damage.IntValue / 2);
        var target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        foreach (var enemy in cardPlay.Player.Creature.CombatState.HittableEnemies)
        {
            if (enemy == target || !enemy.IsAlive)
            {
                continue;
            }
            await CreatureCmd.Damage(
                choiceContext, enemy, splash, ValueProp.Move, cardPlay.Player.Creature);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}

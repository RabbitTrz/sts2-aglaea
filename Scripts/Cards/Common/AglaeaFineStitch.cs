using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 细针密缕：若敌人意图为攻击，则造成等同于其攻击值的伤害。升级后打满额外伤害值的基础上再加（见描述）。
/// 基础版：无固定伤害，完全取决于敌人意图。实际实现：读取敌人意图伤害值，若无攻击意图则造成 0（理论不可打出）。
/// </summary>
public class AglaeaFineStitch() : AglaeaCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        // 读取敌人本次意图的攻击伤害值。
        int damage = GetIntentAttackDamage(target);

        if (damage > 0)
        {
            await CreatureCmd.Damage(
                choiceContext, target, damage, ValueProp.Move, cardPlay.Player.Creature);
        }
    }

    private int GetIntentAttackDamage(Creature target)
    {
        var attackIntent = target.Monster?.NextMove?.Intents?
            .OfType<AttackIntent>()
            .FirstOrDefault();
        if (attackIntent == null)
        {
            return 0;
        }

        var enemies = target.CombatState?.HittableEnemies
            ?? Enumerable.Empty<Creature>().ToList();
        return attackIntent.GetSingleDamage(enemies, target);
    }

    protected override void OnUpgrade()
    {
        // 升级后费用降低（保持机制，具体数值见设计文档）。
        EnergyCost.UpgradeBy(-1);
    }
}

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Monsters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 锻泪匠躯（玩家身上的 Buff，v0.1.3 重做）：**自己的衣匠**死亡时，对所有敌人造成 Amount 点伤害。
///
/// 该伤害【视为衣匠攻击】：
///   · 伤害值吃「金丝」加成（走 TailorCmd.GetTailorDamage）
///   · dealer 用死去的衣匠 → 「间隙织线」会按衣匠攻击计数
///
/// v0.1.3 之前它的效果是「衣匠代伤时由所有衣匠均摊」，已整体替换
/// （TailorSwarmPower 里对应的均摊分支同时移除）。
/// </summary>
public sealed class ForgedTearBodyPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>(),
        HoverTipFactory.FromPower<WeaveLinePower>()
    ];

    public override PowerType Type => PowerType.Buff;

    // 可疊：每打出一张，伤害叠高（卡面数值即 Amount）。
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented)
        {
            return;
        }

        // 只响应自己的衣匠死亡。
        if (creature.Monster is not AglaeaTailorMonster || creature.PetOwner != Owner.Player)
        {
            return;
        }

        Player? player = Owner.Player;
        var combat = Owner.CombatState;
        if (player == null || combat == null || Amount <= 0)
        {
            return;
        }

        // 视为衣匠攻击：吃金丝加成；dealer 用死去的衣匠，间隙织线据此计数。
        // 不走「攻击扣血」（衣匠已经死了）。
        int damage = TailorCmd.GetTailorDamage(player, Amount);
        foreach (var enemy in combat.Enemies.Where(e => e.IsAlive).ToList())
        {
            await CreatureCmd.Damage(
                choiceContext, enemy, damage, ValueProp.Move | ValueProp.Unpowered, creature);
        }
    }
}

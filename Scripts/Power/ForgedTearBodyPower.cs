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
/// 锻泪匠躯（玩家身上的 Buff，v0.1.3 重做）：
/// **自己的衣匠受到致命伤害时**（伤害尚未扣除，衣匠仍存活），对所有敌人造成 Amount 点伤害。
///
/// 该伤害【视为衣匠攻击】：
///   · 伤害值吃「金丝」加成（走 TailorCmd.GetTailorDamage）
///   · dealer 用这名即将死去的衣匠 → 「间隙织线」会按衣匠攻击计数
///
/// 触发点放在「扣血之前」而不是 AfterDeath：原版 <c>CreatureCmd.Damage</c> 在
/// <c>dealer.IsDead</c> 时会直接跳过伤害，死后触发就只能拿一个死掉的 dealer，
/// 伤害会被静默吞掉。
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

    /// <summary>
    /// 在衣匠即将受到致命伤害时调用（伤害尚未扣除，衣匠仍存活），dealer 用这名衣匠。
    /// </summary>
    public static async Task TriggerFromTailor(
        PlayerChoiceContext choiceContext, Player player, Creature tailor)
    {
        var power = player.Creature.GetPower<ForgedTearBodyPower>();
        var combat = player.Creature.CombatState;
        if (power == null || power.Amount <= 0 || combat == null)
        {
            return;
        }

        int damage = TailorCmd.GetTailorDamage(player, power.Amount);
        foreach (var enemy in combat.Enemies.Where(e => e.IsAlive).ToList())
        {
            await CreatureCmd.Damage(
                choiceContext, enemy, damage, ValueProp.Move | ValueProp.Unpowered, tailor, null, null);
        }
    }
}

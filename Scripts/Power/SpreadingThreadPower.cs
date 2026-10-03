using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 传线连绵（玩家身上的 Buff）：间隙织线状态下的敌人在死亡时，
/// 使随机一名存活敌人获得间隙织线，并使其受到 Amount 点伤害。
/// Amount 由「传线连绵」能力牌应用（10 点，升级 15 点）。
/// </summary>
public sealed class SpreadingThreadPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeaveLinePower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented)
        {
            return;
        }

        // 只处理带间隙织线的敌人死亡。
        if (creature.GetPower<WeaveLinePower>() == null)
        {
            return;
        }

        var combat = creature.CombatState;
        if (combat == null)
        {
            return;
        }

        var others = combat.Enemies
            .Where(e => e.IsAlive && e != creature)
            .ToList();
        if (others.Count == 0)
        {
            return;
        }

        // 用 run 级同步随机数流选目标。本地随机数（Random.Shared）在多人下会让
        // 各客户端选到不同敌人 —— 上织线和那份伤害都会落在不同人身上，直接状态分歧闪退。
        var player = Owner.Player ?? Owner.PetOwner;
        if (player == null)
        {
            return;
        }

        var target = player.RunState.Rng.CombatTargets.NextItem(others);
        if (target == null)
        {
            return;
        }

        // 随机敌人获得间隙织线。
        await PowerCmd.Apply<WeaveLinePower>(choiceContext, target, 1, Owner, null, false);

        // 并使其受到 Amount 点伤害。
        if (Amount > 0)
        {
            await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Move, Owner);
        }
    }
}

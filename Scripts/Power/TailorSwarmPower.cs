using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 衣匠集体（玩家身上的隐藏 Buff）：管理阿格莱雅召唤的所有衣匠。
/// - 攻击牌打出后，随机一名衣匠攻击（优先带间隙织线的敌人）造成 3 点伤害。
/// - 玩家受到未[gold]格挡[/gold]的伤害时，由若干衣匠代为承受。
/// 该 power 常驻于玩家身上，不随单个衣匠死亡而消失。
/// </summary>
public sealed class TailorSwarmPower : AglaeaPower
{
    private const int TailorAttackDamage = 3;

    public override PowerType Type => PowerType.Buff;

    // 衣匠集体是「单一常驻」效果，不叠层。
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>当前存活且属于本玩家的所有衣匠。</summary>
    private IEnumerable<Creature> Tailors()
    {
        return Owner.Pets.Where(c => c.IsAlive && c.Monster is AglaeaTailorMonster);
    }

    // 攻击牌打出后，随机一名衣匠攻击随机目标（优先间隙织线）。
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        // 只响应【自己】打出的攻击牌。
        // 这个钩子会被战斗里所有角色的能力收到——包括队友的。不判断归属的话，
        // 队友每打一张攻击牌，自己的衣匠也会跟着攻击一次（多人实测到的 bug）。
        if (cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        if (Owner.Player == null)
        {
            return;
        }

        if (Tailors().Count() == 0)
        {
            return;
        }

        await TailorCmd.AttackOnce(choiceContext, Owner.Player, TailorAttackDamage);
    }

    // 玩家受到未[gold]格挡[/gold]的攻击伤害时，由衣匠代为承受：
    // 优先让血量最低的衣匠抵挡，若其无法完全吸收，则下一个衣匠继续抵挡，
    // 直到所有衣匠耗尽；剩余伤害仍由玩家承受。
    //
    // （v0.1.3 之前这里还有一条「锻泪匠躯 → 均摊伤害」的分支，随该牌重做为
    //   「衣匠死亡时 AOE」而移除。）
    public override decimal ModifyHpLostBeforeOsty(
        Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
        {
            return amount;
        }

        // 只抵挡攻击牌的伤害（与 DieForYouPower 一致，避免误挡毒/荆棘等）。
        if (!props.IsPoweredAttack())
        {
            return amount;
        }

        var tailors = Tailors().ToList();
        if (tailors.Count == 0)
        {
            return amount;
        }

        return PlanDrain(tailors, amount, props);
    }

    /// <summary>本次受击的衣匠分摊计划（伤害在 AfterDamageReceived 里实际扣除）。</summary>
    private sealed class PendingDrain
    {
        public required Creature Tailor;

        public decimal Amount;

        public ValueProp Props;
    }

    private readonly List<PendingDrain> _pendingDrains = new();

    /// <summary>
    /// 只计算分摊、不扣血。真正的扣血延后到异步的 <see cref="AfterDamageReceived"/>：
    /// 这样衣匠受到致命伤害时它仍然存活，可以名正言顺地当锻泪匠躯的 dealer
    /// （原版 CreatureCmd.Damage 会跳过已死 dealer 的伤害）。
    /// </summary>
    private decimal PlanDrain(List<Creature> tailors, decimal amount, ValueProp props)
    {
        decimal remaining = amount;
        _pendingDrains.Clear();

        foreach (var tailor in tailors.OrderBy(c => c.CurrentHp))
        {
            if (remaining <= 0m)
            {
                break;
            }

            decimal absorbed = Math.Min(remaining, tailor.CurrentHp);
            if (absorbed <= 0m)
            {
                continue;
            }

            _pendingDrains.Add(new PendingDrain
            {
                Tailor = tailor,
                Amount = absorbed,
                Props = props,
            });
            remaining -= absorbed;
        }

        return remaining;
    }

    // 玩家受到伤害后：结算衣匠代伤（致命的那一下先触发锻泪匠躯，衣匠此时还活着），
    // 然后清理阵亡衣匠（正常播死亡动画并移出战斗），并为每个阵亡衣匠往抽牌堆放若虫。
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
        {
            return;
        }

        if (Owner.Player == null)
        {
            return;
        }

        var drains = new List<PendingDrain>(_pendingDrains);
        _pendingDrains.Clear();

        foreach (var drain in drains)
        {
            if (!drain.Tailor.IsAlive)
            {
                continue;
            }

            // 这一下会把它打死：在扣血之前触发锻泪匠躯（dealer 用这名衣匠）。
            if (drain.Amount >= drain.Tailor.CurrentHp)
            {
                await ForgedTearBodyPower.TriggerFromTailor(choiceContext, Owner.Player, drain.Tailor);
            }

            drain.Tailor.LoseHpInternal(drain.Amount, drain.Props);
        }

        if (drains.Count > 0)
        {
            // 统一走 TailorCmd 的清理：衣匠攻击掉血致死（v0.1.3）走的是同一套收尾。
            await TailorCmd.CleanupDeadTailors(choiceContext, Owner.Player, cardSource);
        }
    }
}

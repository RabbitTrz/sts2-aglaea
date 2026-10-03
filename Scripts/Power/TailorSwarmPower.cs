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
    // - 默认：优先让血量最低的衣匠抵挡，若其无法完全吸收，则下一个衣匠继续抵挡，
    //   直到所有衣匠耗尽；剩余伤害仍由玩家承受。
    // - 若玩家有「锻泪匠躯」：改为把伤害均摊给所有存活衣匠（每个衣匠承受份额差值最多 1，
    //   如 8 点伤害分 3 个衣匠 -> 3+3+2），每个衣匠最多承受自身当前血量，剩余由玩家承受。
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

        if (Owner.GetPower<ForgedTearBodyPower>() != null)
        {
            return DistributeEvenly(tailors, amount, props);
        }

        return DrainSequentially(tailors, amount, props);
    }

    // 依次抵挡：血量最低的衣匠优先吸收，直至耗尽。
    private decimal DrainSequentially(List<Creature> tailors, decimal amount, ValueProp props)
    {
        decimal remaining = amount;

        foreach (var tailor in tailors.OrderBy(c => c.CurrentHp))
        {
            if (remaining <= 0m)
            {
                break;
            }

            decimal absorbed = Math.Min(remaining, tailor.CurrentHp);
            tailor.LoseHpInternal(absorbed, props);
            remaining -= absorbed;
        }

        return remaining;
    }

    // 均摊：把伤害平均分给所有衣匠（差值最多 1），每个衣匠最多承受自身当前血量。
    private decimal DistributeEvenly(List<Creature> tailors, decimal amount, ValueProp props)
    {
        decimal remaining = amount;

        // 份额分配：base + 前 rem 个衣匠额外 +1，使差值最多为 1。
        int n = tailors.Count;
        int baseShare = (int)(amount / n);
        int rem = (int)(amount % n);

        for (int i = 0; i < n; i++)
        {
            decimal share = baseShare + (i < rem ? 1 : 0);
            if (share <= 0m)
            {
                continue;
            }

            decimal absorbed = Math.Min(share, tailors[i].CurrentHp);
            tailors[i].LoseHpInternal(absorbed, props);
            remaining -= absorbed;
        }

        return Math.Max(0m, remaining);
    }

    // 玩家受到伤害后，清理掉因抵挡而阵亡的衣匠（正常播死亡动画并移出战斗），
    // 并为每个阵亡的衣匠往玩家抽牌堆放一张若虫。
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
        {
            return;
        }

        var deadTailors = Owner.Pets
            .Where(c => !c.IsAlive && c.Monster is AglaeaTailorMonster)
            .ToList();
        if (deadTailors.Count == 0)
        {
            return;
        }

        await CreatureCmd.Kill(deadTailors);

        if (Owner.Player != null)
        {
            await TailorCmd.AddChrysalisToDrawPile(
                choiceContext, Owner.Player, cardSource, deadTailors.Count);
        }
    }
}

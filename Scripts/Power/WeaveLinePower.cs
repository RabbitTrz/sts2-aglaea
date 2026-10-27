using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Monsters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 间隙织线（Debuff）：标记一名敌人为衣匠优先攻击的目标。
/// 该敌人每累计受到 N 次【衣匠的】攻击，就在本回合失去 1 点力量，然后计数归零。
///
/// N 由全场玩家构成动态决定（公式不体现在描述文本中）：
///     2 + (玩家数 - 1) + (其中 Aglaea 角色数 - 1)
/// 单人单 Aglaea 时为 2。
///
/// 计数为全场共享：此 debuff 全场同时只存在于一名敌人身上。
/// v0.1.4 规则：计数**跨回合保留**（回合开始不清零）；
/// 对同一目标**重复施加**时，视为额外累计 1 次衣匠攻击。
///
/// Amount 的编码：Amount = 已累计攻击次数 + 1（至少为 1）。
/// 之所以不直接用 0 表示 0 次，是因为 Counter 型 power 的 Amount 归零会被原版自动移除。
/// 图标数字由 <see cref="DisplayAmount"/> 反向显示为「还差几次」。
/// </summary>
public sealed class WeaveLinePower : AglaeaPower
{
    /// <summary>首次施加时用来吞掉 Apply 触发的 AfterPowerAmountChanged（首次不计数）。</summary>
    private bool _justApplied;

    public override PowerType Type => PowerType.Debuff;

    // 必须为 Counter，NPower 才会渲染图标上的数字（见 NCard/NPower 的 DisplayAmount 逻辑）。
    public override PowerStackType StackType => PowerStackType.Counter;

    // 说明里提到「衣匠」，所以把衣匠也挂上；力量下降那层是原有的。
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MonarchsGazeStrengthDownPower>(),
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    /// <summary>
    /// 触发力量下降所需的衣匠攻击次数。
    /// 2 + (玩家数 - 1) + (Aglaea 角色数 - 1)，最少为 1。
    /// </summary>
    public int HitsRequired
    {
        get
        {
            var combat = Owner.CombatState;
            if (combat == null)
            {
                return 2;
            }

            // 绞紧：本回合触发力量下降所需的攻击次数降为 1。
            // 由「绞紧」技能牌挂 TwistTightPower 标记，作用对象是整条间隙织线（全场唯一）。
            if (combat.Players.Any(p => p.Creature.GetPower<TwistTightPower>() != null))
            {
                return 1;
            }

            int playerCount = combat.Players.Count;
            int aglaeaCount = combat.Players.Count(p => p.Character.Id.Entry.RemovePrefix() == "AGLAEA");

            return System.Math.Max(1, 2 + (playerCount - 1) + (aglaeaCount - 1));
        }
    }

    /// <summary>图标显示「还差几次衣匠攻击触发力量下降」。</summary>
    public override int DisplayAmount => System.Math.Max(0, HitsRequired + 1 - Amount);

    /// <summary>首次挂载：Amount = 1（0 次累计）。Apply 传入的层数不参与计数。</summary>
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _justApplied = true;
        SetAmount(1);
        return Task.CompletedTask;
    }

    // 赋予到目标身上时，解除其他敌人身上的间隙织线，保证全局唯一。
    public override Task BeforeApplied(Creature receiver, decimal amount, Creature? applier, CardModel? cardSource)
    {
        var combat = receiver.CombatState;
        if (combat != null)
        {
            foreach (var enemy in combat.Enemies)
            {
                if (enemy == receiver)
                {
                    continue;
                }

                var existing = enemy.GetPower<WeaveLinePower>();
                if (existing != null)
                {
                    PowerCmd.Remove(existing);
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 重复施加：视为额外累计 1 次衣匠攻击。
    /// PowerCmd.ModifyAmount 已经把施加层数加进 Amount，这里先把编码修正回「+1 次」，
    /// 再走统一的阈值结算。
    /// </summary>
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power != this || amount <= 0)
        {
            return;
        }

        // 首次施加时 Apply 也会触发一次本 hook；那一次不算累计。
        if (_justApplied)
        {
            _justApplied = false;
            return;
        }

        int applied = (int)amount;
        if (applied != 1)
        {
            // 规则是「每次重复施加只算 1 次」，不按施加层数计算。
            SetAmount(Amount - applied + 1);
        }

        await CheckThreshold(choiceContext, applier, cardSource);
    }

    // 只统计来自衣匠的攻击；每累计 N 次使其本回合失去 1 点力量并归零。
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner)
        {
            return;
        }

        // 仅响应衣匠造成的伤害；若某玩家本回合打出过「逮捕」，
        // 则该玩家造成的伤害也可触发（逮捕：本回合其他玩家的攻击可触发力量下降）。
        if (dealer == null)
        {
            return;
        }

        bool fromTailor = dealer.Monster is AglaeaTailorMonster;
        if (!fromTailor)
        {
            bool arrested = Owner.CombatState?.Players.Any(
                p => p.Creature.GetPower<ArrestPower>() != null) ?? false;
            if (!arrested || !dealer.IsPlayer)
            {
                return;
            }
        }

        // Amount = 次数 + 1，所以一次攻击把 Amount +1 即可。
        SetAmount(Amount + 1);
        await CheckThreshold(choiceContext, dealer, cardSource);
    }

    /// <summary>
    /// Amount - 1 = 已累计次数。达到阈值时归零（Amount 回到 1）并结算力量变化。
    /// 注意 Amount 不能设成 0：Counter 型 power 归零会被原版移除。
    /// </summary>
    private async Task CheckThreshold(
        PlayerChoiceContext choiceContext, Creature? dealer, CardModel? cardSource)
    {
        if (Amount - 1 < HitsRequired)
        {
            return;
        }

        SetAmount(1);

        // 脆若游丝要按【攻击者的主人】判断，而不是固定取 Players[0]：
        // 多人下衣匠可能属于任何一个玩家，用 1 号玩家的状态会判断错人。
        var attackerOwner = dealer?.PetOwner ?? dealer?.Player;

        // 脆若游丝：力量降低改为力量提升。
        if (attackerOwner != null && attackerOwner.Creature.GetPower<GossamerPower>() != null)
        {
            await PowerCmd.Apply<FlexPotionPower>(
                choiceContext, Owner, 1, dealer, cardSource, false);
            return;
        }

        // 绞紧只改写触发门槛（见 HitsRequired），力量下降本身固定 1 点。
        await PowerCmd.Apply<MonarchsGazeStrengthDownPower>(
            choiceContext, Owner, 1, dealer, cardSource, false);
    }
}

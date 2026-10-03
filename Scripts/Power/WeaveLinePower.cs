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
/// 计数在回合结束时清零（力量损失本身由 TemporaryStrength 在回合结束清除）。
///
/// Amount 语义 = 本回合已受到的衣匠攻击次数；图标数字由 <see cref="DisplayAmount"/>
/// 反向显示为「还差几次」。
/// </summary>
public sealed class WeaveLinePower : AglaeaPower
{
    public override PowerType Type => PowerType.Debuff;

    // 必须为 Counter，NPower 才会渲染图标上的数字（见 NCard/NPower 的 DisplayAmount 逻辑）。
    public override PowerStackType StackType => PowerStackType.Counter;

    // 说明里提到「衣匠」，所以把衣匠也挂上；力量下降那层是原有的。
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MonarchsGazeStrengthDownPower>(),
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    private sealed class Data
    {
        // 本回合已受到的衣匠攻击次数。与 Amount 保持同步，便于在 hook 中读写。
        public int HitsThisTurn;
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

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

            int playerCount = combat.Players.Count;
            int aglaeaCount = combat.Players.Count(p => p.Character.Id.Entry.RemovePrefix() == "AGLAEA");

            return System.Math.Max(1, 2 + (playerCount - 1) + (aglaeaCount - 1));
        }
    }

    /// <summary>图标显示「还差几次衣匠攻击触发力量下降」。</summary>
    public override int DisplayAmount => System.Math.Max(0, HitsRequired - Amount);

    /// <summary>首次挂载时把计数归零（Apply 传入的 amount 仅用于让 power 存在，不参与计数）。</summary>
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SetAmount(0);
        GetInternalData<Data>().HitsThisTurn = 0;
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

        var data = GetInternalData<Data>();
        data.HitsThisTurn++;

        int required = HitsRequired;
        if (data.HitsThisTurn < required)
        {
            SetAmount(data.HitsThisTurn);
            return;
        }

        // 达成阈值：归零后施加力量下降。
        data.HitsThisTurn = 0;
        SetAmount(0);

        var player = Owner.CombatState?.Players.FirstOrDefault();
        if (player == null)
        {
            return;
        }

        // 脆若游丝：力量降低改为力量提升。
        if (player.Creature.GetPower<GossamerPower>() != null)
        {
            await PowerCmd.Apply<FlexPotionPower>(
                choiceContext, Owner, 1, dealer, cardSource, false);
            return;
        }

        // 绞紧：玩家身上有该能力时，力量下降翻倍。
        int loss = player.Creature.GetPower<TwistTightPower>() != null ? 2 : 1;

        await PowerCmd.Apply<MonarchsGazeStrengthDownPower>(
            choiceContext, Owner, loss, dealer, cardSource, false);
    }

    // 回合结束重置计数（力量损失本身由 TemporaryStrengthPower 在回合结束清除）。
    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> creatures)
    {
        var data = GetInternalData<Data>();
        data.HitsThisTurn = 0;
        SetAmount(0);
        return Task.CompletedTask;
    }
}

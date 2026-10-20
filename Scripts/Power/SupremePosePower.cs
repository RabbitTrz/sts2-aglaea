using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MoeNegiMod.Aglaea.Character;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 至高之姿（玩家身上的 Buff，Counter 叠层）：
/// 每打出 1 张攻击牌时，消耗 1 层「至高之姿」，抽 1 张牌，并获得 1 层「金丝」。
/// 当「至高之姿」层数归 0（或被其他机制移除）时，移除玩家身上所有的「金丝」。
/// </summary>
public sealed class SupremePosePower : AglaeaPower
{
    /// <summary>角色身上的环状形态特效（进入至高之姿时挂上、失去时移除）。</summary>
    private Node2D? _vfx;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>获得至高之姿：挂上金色环特效（原版恶魔形态同款挂点）。</summary>
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _vfx = AglaeaVfx.AttachFormVfx(Owner, AglaeaVfx.SupremePoseAuraScenePath);
        return Task.CompletedTask;
    }

    // 每打出 1 张攻击牌：消耗 1 层 → 抽 1 张 → 获得 1 层金丝。
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        // 只响应【自己】打出的攻击牌。
        // 钩子会收到全场所有角色的出牌——不判断归属的话，队友打攻击牌会消耗自己的
        // 至高之姿层数、并替自己抽牌（多人实测到的 bug）。
        if (cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        var player = Owner.Player;
        if (player == null)
        {
            return;
        }

        // 已无层数（理论上不应发生，兜底）：不再触发，避免降为负数。
        if (Amount <= 0)
        {
            await PowerCmd.Remove<SupremePosePower>(Owner);
            return;
        }

        // 消耗 1 层至高之姿。
        SetAmount(Amount - 1);

        // 通知「至高之姿消耗」监听者（命运眼睑、蓄势绵延等）。
        foreach (var listener in Owner.Powers.OfType<ISupremePoseConsumeListener>())
        {
            await listener.OnSupremePoseConsumed(1);
        }

        // 抽 1 张牌。
        await CardPileCmd.Draw(choiceContext, 1, player);

        // 归 0 时移除自身（金丝清理统一在 AfterRemoved 中处理）。
        if (Amount <= 0)
        {
            await PowerCmd.Remove<SupremePosePower>(Owner);
        }
        else
        {
            await PowerCmd.Apply<GoldenThreadPower>(
                choiceContext, Owner, 1, Owner, cardPlay.Card);
        }
    }

    // 至高之姿被移除（无论何种方式）时：撤掉环状特效，并一并移除玩家身上所有金丝。
    public override async Task AfterRemoved(Creature oldOwner)
    {
        _vfx?.QueueFree();
        _vfx = null;

        await PowerCmd.Remove<GoldenThreadPower>(oldOwner);
    }
}

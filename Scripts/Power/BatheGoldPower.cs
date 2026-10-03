using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 沐浴黄金（玩家身上的 Buff，Counter 叠层）：下一次衣匠攻击后，移除 Amount 层金丝。
/// 由「沐浴黄金」能力牌应用（7 层，升级 10 层）。
/// </summary>
public sealed class BatheGoldPower : AglaeaPower, ITailorAttackListener
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<GoldenThreadPower>(),
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public Task BeforeTailorAttack(
        PlayerChoiceContext choiceContext, Player player, Creature tailor, Creature target)
    {
        return Task.CompletedTask;
    }

    public async Task AfterTailorAttack(
        PlayerChoiceContext choiceContext, Player player, Creature tailor, Creature target, int damage)
    {
        var golden = player.Creature.GetPower<GoldenThreadPower>();
        if (golden == null || golden.Amount <= 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        // 移除 min(Amount, 当前金丝) 层金丝，然后移除自身。
        int remove = Math.Min(Amount, golden.Amount);
        await PowerCmd.ModifyAmount(choiceContext, golden, -remove, Owner, null, true);
        await PowerCmd.Remove(this);
    }
}

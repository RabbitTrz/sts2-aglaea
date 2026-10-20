using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 战甲（天谴斗士自身 Buff，Counter 递减）：
/// 每被攻击 N 次，就在我方所有玩家手牌中放入 1 张【战甲】，然后重新开始计数。
/// N 为动态阈值：1 + 玩家数 × 3（单人 = 4；2026-10-06 用户定的公式）。
///
/// Amount 语义 = 距离下次触发还差几次被攻击（N → 1），图标数字直接就是它。
/// 计数用 <see cref="PowerModel.SetAmount"/> 手改而不是 <c>PowerCmd.Decrement</c> ——
/// 后者在归零时会顺手把 power 移除（见 <c>ShouldRemoveDueToAmount</c>）。
/// </summary>
public sealed class WarplatePower : AglaeaPower
{
    /// <summary>触发所需的被攻击次数：1 + 玩家数 × 3。</summary>
    public static int GetHitsRequired(int playerCount)
    {
        return 1 + playerCount * 3;
    }

    /// <summary>本场战斗的阈值（跟随队伍人数）。</summary>
    private int HitsRequired => GetHitsRequired(Owner.CombatState?.Players.Count ?? 1);

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>说明文本里的阈值数字（人数不同会变）。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Hits", 4m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<AglaeaWarplate>()
    ];

    /// <summary>挂上时把计数设成当前阈值（Apply 只传占位 amount）。</summary>
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SetAmount(HitsRequired);
        DynamicVars["Hits"].BaseValue = HitsRequired;
        return Task.CompletedTask;
    }

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

        // 「被攻击」= 任何带 Move 语义的伤害，包括衣匠这种 Unpowered 的攻击；
        // 不带 Move 的伤害（失去生命、毒等）不算被攻击。
        if (!props.HasFlag(ValueProp.Move))
        {
            return;
        }

        if (Amount > 1)
        {
            SetAmount(Amount - 1);
            return;
        }

        // 计数归位后发牌（先复位再发，避免发牌过程里再吃到攻击导致计数错乱）。
        SetAmount(HitsRequired);
        await DeliverWarplates(choiceContext);
    }

    /// <summary>向所有存活玩家手中各放入 1 张【战甲】。</summary>
    private async Task DeliverWarplates(PlayerChoiceContext choiceContext)
    {
        var combat = Owner.CombatState;
        if (combat == null)
        {
            return;
        }

        List<Creature> targets = combat.Players
            .Select(p => p.Creature)
            .Where(c => c.IsAlive)
            .ToList();
        if (targets.Count == 0)
        {
            return;
        }

        // 塞牌逻辑与原版【实验体】塞灼烧一致：同一 API、对所有玩家生效。
        await CardPileCmd.AddToCombatAndPreview<AglaeaWarplate>(
            targets, PileType.Hand, 1, null);
    }
}

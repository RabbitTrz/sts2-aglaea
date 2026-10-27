using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Commands;
using System.Collections.Generic;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 金丝成障（玩家身上的 Buff）：**存在衣匠时，你受到的伤害 -1**（固定 1 点，不随数量增加）。
/// 升级后由卡牌获得「固有」，本 power 本身不变。
///
/// 减伤走原版伤害修正（Additive）：多段攻击的每一段都各减一次。
/// </summary>
public sealed class GoldBarrierPower : AglaeaPower
{
    /// <summary>减伤量（固定 1 点）。</summary>
    private const decimal Reduction = 1m;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    // 单一常驻效果，不叠层。
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 只保护玩家自己；只要还有存活衣匠就固定减 1 点，且不会把伤害减成负数。
    /// </summary>
    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner)
        {
            return 0m;
        }

        var player = Owner.Player;
        if (player == null)
        {
            return 0m;
        }

        if (TailorCmd.AliveTailors(player).Count == 0)
        {
            return 0m;
        }

        return -System.Math.Min(Reduction, amount);
    }
}

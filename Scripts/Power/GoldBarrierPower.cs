using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Monsters;
using System.Collections.Generic;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 金丝成障（玩家身上的 Buff，v0.1.3 重做）：**自己的衣匠**受到伤害时，降低 1 点受到的伤害。
/// 升级后由卡牌获得「固有」，本 power 本身不变。
///
/// v0.1.3 之前它的效果是「回合开始时若处于至高之姿则获得格挡」，已整体替换。
/// </summary>
public sealed class GoldBarrierPower : AglaeaPower
{
    /// <summary>每次受到伤害时减免的点数。</summary>
    private const decimal Reduction = 1m;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    // 单一常驻效果，不叠层。
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 只对【自己的】衣匠生效。返回的是加法修正，所以减伤是负值。
    /// </summary>
    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target == null || target.Monster is not AglaeaTailorMonster)
        {
            return 0m;
        }
        if (target.PetOwner != Owner.Player)
        {
            return 0m;
        }

        // 不要把伤害减成负数。
        return -System.Math.Min(Reduction, amount);
    }
}

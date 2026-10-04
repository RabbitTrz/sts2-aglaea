using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 静息蓄势（v0.1.3 重做）：通过卡牌【直接移除】至高之姿时，每移除 1 层获得 1 点力量。
/// Amount = 每层换算的力量点数（升级后为 2）。
///
/// 原先的效果是「一回合没受伤，下回合开始获得至高之姿」，已整体替换。
///
/// 「直接移除」专指卡牌主动清层（绞丝 / 火种归还 / 史诗终结），
/// 不包含至高之姿自己每打一张攻击牌消耗 1 层的「消耗」。
/// 回调入口见 <see cref="Commands.SupremePoseCmd.RemoveAllDirectly"/>。
/// </summary>
public sealed class RestAccumulatePower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<GoldenThreadPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 由 <see cref="Commands.SupremePoseCmd.RemoveAllDirectly"/> 调用：
    /// 卡牌直接移除了 <paramref name="removed"/> 层至高之姿，按此给力量。
    /// </summary>
    public async Task OnSupremePoseRemovedDirectly(
        PlayerChoiceContext choiceContext, int removed, AbstractModel? source)
    {
        if (Amount <= 0 || removed <= 0)
        {
            return;
        }

        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner, Amount * removed, Owner, source as CardModel);
    }
}

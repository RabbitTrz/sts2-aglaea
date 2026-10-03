using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 领袖魅力（玩家身上的 Buff）：所有玩家每打出 Threshold 张攻击牌，你获得 1 层至高之姿。
///
/// Threshold 由卡牌 Apply 传入（基础 10，升级 7），保存在 Data.Threshold 中。
/// Amount 语义 = 全员累计的攻击次数（0..Threshold-1）。
/// 图标数字由 <see cref="DisplayAmount"/> 反向显示为「还差几次」。
/// </summary>
public sealed class LeadershipCharmPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 每次打出都新建独立实例（同原版 TheBombPower 的「炸弹」）：
    // 每份各自按自己的门槛累计，互不干扰。这样叠层 = 产出频率翻倍，
    // 也天然避免「第二张改写了第一张的门槛/进度」。
    // Instanced 下 FindExistingInstanceForStacking 恒返回 null，
    // 因此 AfterApplied 每次挂载都会跑，Threshold 与进度各实例独立。
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    private sealed class Data
    {
        public int Threshold;
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

    /// <summary>门槛（基础 10，升级 7）。每个实例各自持有，互不影响。</summary>
    public int Threshold => GetInternalData<Data>().Threshold;

    /// <summary>图标显示「还差几次全员攻击获得 1 层至高之姿」。</summary>
    public override int DisplayAmount => System.Math.Max(0, GetInternalData<Data>().Threshold - Amount);

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var data = GetInternalData<Data>();
        data.Threshold = Amount;
        SetAmount(0);
        return Task.CompletedTask;
    }

    // 任意玩家打出攻击牌都计数。
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只数【攻击牌】，与「每打出 N 张攻击牌」的措辞一致。
        // 不用 AfterDamageGiven 计数：那样多段攻击会按命中次数重复计数，与牌面不符。
        if (cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        var data = GetInternalData<Data>();
        if (data.Threshold <= 0)
        {
            return;
        }

        SetAmount(Amount + 1);
        if (Amount < data.Threshold)
        {
            return;
        }

        SetAmount(0);
        Flash();
        await PowerCmd.Apply<SupremePosePower>(
            choiceContext, Owner, 1, Owner, null);
    }
}

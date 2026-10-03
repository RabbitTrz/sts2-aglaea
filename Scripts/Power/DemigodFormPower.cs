using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 半神形态（玩家身上的 Buff）：每攻击 Threshold 次获得 1 层神性。
/// 衣匠攻击时，消耗所有神性，每消耗一层该衣匠额外攻击一次。
///
/// Threshold 由卡牌 Apply 传入（基础 3，升级 2），保存在 Data.Threshold 中。
/// Amount 语义 = 已达到的攻击次数（0..Threshold-1）。
/// 图标数字由 <see cref="DisplayAmount"/> 反向显示为「还差几次攻击获得神性」。
///
/// 仅角色攻击计入（AfterDamageGiven，dealer == Owner，多段攻击每段计一次），
/// 而不是打出攻击牌的次数。
/// </summary>
public sealed class DemigodFormPower : AglaeaPower, ITailorAttackListener
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DivinityPower>(),
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    private const int TailorAttackDamage = 3;

    public override PowerType Type => PowerType.Buff;

    // 必须为 Counter，NPower 才会渲染图标上的数字。
    public override PowerStackType StackType => PowerStackType.Counter;

    // 每次打出都新建独立实例（同原版 TheBombPower 的「炸弹」）：
    // 每份各自按自己的门槛累计，互不干扰。这样叠层 = 产出频率翻倍，
    // 也天然避免「第二张改写了第一张的门槛/进度」。
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    private sealed class Data
    {
        // 达到神性所需的攻击次数。由卡牌 Apply 的 amount 初始化。
        public int Threshold;
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

    /// <summary>图标显示「还差几次角色攻击获得 1 层神性」。</summary>
    public override int DisplayAmount => System.Math.Max(0, GetInternalData<Data>().Threshold - Amount);

    /// <summary>首次挂载时把 Apply 传入的 amount 存作阈值，进度计数归零。</summary>
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var data = GetInternalData<Data>();
        data.Threshold = Amount;
        SetAmount(0);
        return Task.CompletedTask;
    }

    // 角色每攻击一次 +1，达到阈值就 +1 神性并清零。
    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner)
        {
            return;
        }
        if (!props.IsPoweredAttack())
        {
            return;
        }

        var data = GetInternalData<Data>();
        if (data.Threshold <= 0)
        {
            return;
        }

        int next = Amount + 1;
        if (next < data.Threshold)
        {
            SetAmount(next);
            return;
        }

        SetAmount(0);
        await PowerCmd.Apply<DivinityPower>(choiceContext, Owner, 1, Owner, cardSource);
    }

    public async Task BeforeTailorAttack(
        PlayerChoiceContext choiceContext, Player player, Creature tailor, Creature target)
    {
        if (_consuming || player == null)
        {
            return;
        }

        var divinity = player.Creature.GetPower<DivinityPower>();
        if (divinity == null || divinity.Amount <= 0)
        {
            return;
        }

        // 先清零神性标记（仅改数值，不修改 Powers 集合，避免打断 AttackWith 的枚举），
        // 再驱动同一衣匠连续额外攻击。_consuming 防止额外攻击递归再触发。
        _consuming = true;
        int extra = divinity.Amount;
        divinity.SetAmount(0);
        try
        {
            for (int i = 0; i < extra; i++)
            {
                await TailorCmd.AttackWith(choiceContext, player, tailor, TailorAttackDamage);
            }
        }
        finally
        {
            _consuming = false;
        }
    }

    public Task AfterTailorAttack(
        PlayerChoiceContext choiceContext, Player player, Creature tailor, Creature target, int damage)
    {
        return Task.CompletedTask;
    }

    // 防止衣匠攻击触发的「额外攻击」再递归消耗神性。
    private bool _consuming;
}

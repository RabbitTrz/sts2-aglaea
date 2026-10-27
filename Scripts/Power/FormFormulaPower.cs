using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 化形公式（赞达尔自身 Debuff，Counter）：
/// 每次切换形态时 +1 层，每层使自身受到的伤害提高 50%，**永久叠加**（v0.1.4 裁定）。
///
/// 初始层数为 0：Apply 传入的 amount 只为让 power 存在，随后在 AfterApplied 里清 0
/// （与间隙织线同一套写法）。
///
/// 说明文本里的「当前提高 X%」由 <c>DamageIncrease</c> 这个 DynamicVar 提供
/// （受伤倍率 = 1 + 0.5×层数），层数变化时同步刷新；格式化用游戏内置的 percentMore()
/// ——把 1.5 显示成「50」。注意 SmartFormat 没有通用的乘法格式化器，
/// 不要写 <c>{Amount:times(50)}</c>（会报 formatter 不存在的错误）。
/// </summary>
public sealed class FormFormulaPower : AglaeaPower
{
    /// <summary>每层提高的受伤倍率。</summary>
    private const decimal IncreasePerStack = 0.25m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>受伤倍率（1.0 = 未提高）。供悬浮说明用 percentMore() 显示为百分比。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DamageIncrease", 1m)
    ];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SetAmount(0);
        RefreshDamageIncrease();
        return Task.CompletedTask;
    }

    /// <summary>层数变化（切换形态）时同步受伤倍率。只会因为自己的层数变化刷新。</summary>
    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature applier,
        CardModel cardSource)
    {
        if (power == this)
        {
            RefreshDamageIncrease();
        }

        return Task.CompletedTask;
    }

    private void RefreshDamageIncrease()
    {
        DynamicVars["DamageIncrease"].BaseValue = 1m + IncreasePerStack * Amount;
    }

    /// <summary>受到的所有伤害 ×(1 + 0.25 × 层数)。</summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner)
        {
            return 1m;
        }

        return 1m + IncreasePerStack * Amount;
    }
}

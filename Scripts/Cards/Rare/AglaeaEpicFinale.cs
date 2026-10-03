using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Powers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 史诗终结：移除自身的金丝、至高之姿，以及大于 0 的力量与敏捷，
/// 造成 1 点伤害。每移除 1 层上述效果，本牌伤害 +3。
/// 升级：每层加成 +2（即 +5）。
///
/// 力量/敏捷只移除「大于 0」的部分：负数是敌人给的负面效果，
/// 若一并移除反而等于帮玩家解掉，不符合卡牌设计意图。
/// </summary>
public class AglaeaEpicFinale() : AglaeaCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(1m),
        new ExtraDamageVar(3m),
        // CalculatedDamage 只是一个「显示用」的伤害值：它的数值由
        // CalculationBase(=1) + ExtraDamage(=3) × 乘数 得来，但真正结算的伤害仍是
        // OnPlay 里用实际移除的层数现算的。乘数必须与 OnPlay 的计数口径【完全一致】，
        // 否则卡面预览与实战伤害会对不上。
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(
            static (card, _) => CountRemovableStacks(card.Owner.Creature))
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<GoldenThreadPower>(),
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        var creature = cardPlay.Player.Creature;
        int removed = 0;

        removed += await RemoveAll<GoldenThreadPower>(creature);
        removed += await RemoveAll<SupremePosePower>(creature);

        // 力量/敏捷只移除正数部分。
        removed += await RemoveAll<StrengthPower>(creature);
        removed += await RemoveAll<DexterityPower>(creature);

        // 每移除 1 层，本牌伤害 +ExtraDamage。
        //
        // 【不要】写成 DynamicVars.Damage.BaseValue += ...：改的是卡牌【实例】的
        // DynamicVars，而实例会进牌堆、随后被序列化进存档。一旦带着被改过的
        // BaseValue 落盘，下次读档这张牌的基础伤害就永久偏大。
        // 这里跟「命定共舞」一样，把加成算进【本次攻击的局部变量】，只对这一次结算生效。
        int damage = DynamicVars.CalculationBase.IntValue
            + removed * DynamicVars.ExtraDamage.IntValue;

        await CreatureCmd.Damage(
            choiceContext, target, damage, ValueProp.Move, cardPlay.Player.Creature);
    }

    /// <summary>
    /// 数出【当前】能被本牌移除的层数，供 <see cref="CalculatedDamageVar"/> 的乘数使用
    /// （卡面指向目标后显示的伤害 = 1 + ExtraDamage × 本值）。
    ///
    /// 口径必须与 <see cref="RemoveAll{T}"/> 严格一致：同类型只算一个实例，
    /// 且只在层数大于 0 时计入。力量/敏捷的负数层数（敌人施加的负面效果）不计。
    ///
    /// <c>Owner</c> / <c>Owner.Creature</c> 理论上非 null，但战斗外重算预览时
    /// 确有取到 null 的路径，这里保守判空而不是让它抛。
    /// </summary>
    private static decimal CountRemovableStacks(Creature? creature)
    {
        if (creature == null)
        {
            return 0m;
        }

        int stacks = PositiveAmount<GoldenThreadPower>(creature)
            + PositiveAmount<SupremePosePower>(creature)
            + PositiveAmount<StrengthPower>(creature)
            + PositiveAmount<DexterityPower>(creature);

        return stacks;
    }

    private static int PositiveAmount<T>(Creature creature) where T : PowerModel
    {
        var power = creature.GetPower<T>();
        return power != null && power.Amount > 0 ? power.Amount : 0;
    }

    /// <summary>
    /// 移除该 power（仅当其层数大于 0 时），返回被移除的层数。
    /// 负数层数（敌方施加的负面效果）保持不动。
    /// </summary>
    private static async Task<int> RemoveAll<T>(Creature creature) where T : PowerModel
    {
        var power = creature.GetPower<T>();
        if (power == null || power.Amount <= 0)
        {
            return 0;
        }

        int amount = power.Amount;
        await PowerCmd.Remove(power);
        return amount;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }
}

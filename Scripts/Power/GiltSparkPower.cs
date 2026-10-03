using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 金石闪烁（玩家身上的 Buff）：所有攻击牌获得消耗；每消耗 2 张攻击牌，获得 Amount 点力量。
///
/// <para>Amount = 「每消耗 2 张攻击牌获得的力量数」，由卡牌应用（1 点），
/// 重复打出「金石闪烁」会叠加 Amount（不重置 <see cref="ExhaustCount"/>）。这是 power 图标上显示的数字。</para>
///
/// <para><see cref="ExhaustCount"/> 是已消耗攻击牌的累计张数，只增不减；每满 2 张结算一次力量，
/// 余数保留到下一次消耗。它不显示在图标上，只作为内部进度。</para>
/// </summary>
public sealed class GiltSparkPower : AglaeaPower
{
    /// <summary>每消耗多少张攻击牌结算一次力量。</summary>
    private const int CardsPerTrigger = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 已消耗的攻击牌张数。用普通属性而非 [SavedProperty]：
    /// 该 power 本身随战斗结束清理，计数不需要跨战斗保留。
    /// </summary>
    public int ExhaustCount { get; set; }

    // 所有攻击牌获得消耗。
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card.Type != CardType.Attack || card.Owner?.Creature != Owner)
        {
            return false;
        }
        keywords.Add(CardKeyword.Exhaust);
        return true;
    }

    // 累计消耗的攻击牌，每满 2 张给 Amount 点力量，余数保留。
    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Type != CardType.Attack || card.Owner?.Creature != Owner)
        {
            return;
        }

        ExhaustCount += 1;

        int triggers = ExhaustCount / CardsPerTrigger;
        if (triggers <= 0)
        {
            return;
        }

        // 只扣掉已结算的部分，余数留到下次（不重置计数）。
        ExhaustCount -= triggers * CardsPerTrigger;

        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner, triggers * Amount, Owner, card);
    }
}

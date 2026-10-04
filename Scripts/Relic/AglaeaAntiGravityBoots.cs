using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 反重力皮靴（专属遗物，不进池）：战斗中生成的卡牌获得附魔「迅速 1」。
/// 由「火种」累计满 12 层时发放。
///
/// 「迅速」（Swift）是游戏自带的附魔：打出时抽 1 张牌，然后自身失效。
/// v0.1.3 之前的效果是「每打出 5 张攻击牌抽 1 张」，已替换。
/// </summary>
public class AglaeaAntiGravityBoots : AglaeaExclusiveRelic
{
    /// <summary>每张生成卡获得的附魔层数（1 = 打出时抽 1 张）。</summary>
    private const int EnchantAmount = 1;

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        // 只给自己生成的卡附魔。
        if (creator != Owner || card.Enchantment != null)
        {
            return Task.CompletedTask;
        }

        // 已有附魔的卡跳过：不覆盖原有附魔，也避免 Enchant 抛异常。
        var swift = ModelDb.Enchantment<Swift>().ToMutable();
        if (swift.CanEnchant(card))
        {
            CardCmd.Enchant(swift, card, EnchantAmount);
        }

        return Task.CompletedTask;
    }
}

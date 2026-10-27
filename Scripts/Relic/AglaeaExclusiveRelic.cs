using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 阿格莱雅专属遗物的公共基类（反重力皮靴 / 火力风暴潮 / 光速螺旋桨）。
///
/// 稀有度标为 <see cref="RelicRarity.Event"/>：
///   · 会出现在图鉴的「事件」分类里（需要加入遗物池，见 AglaeaRelicPool）；
///   · 原版随机遗物奖励只抽 Common/Uncommon/Rare，Event 不会被随机掉落；
///   · <see cref="IsAllowedInShops"/> = false，商店也不会出售。
/// 实际只能由「火种」累计满 12 层时发放。
///
/// v0.1.3 之前三个遗物共用「每打出 5 张攻击牌触发一次」的机制，现已各自重做，
/// 因此基类不再提供计数器，各子类自己实现所需的钩子。
/// </summary>
public abstract class AglaeaExclusiveRelic : AglaeaRelics
{
    public override RelicRarity Rarity => RelicRarity.Event;

    // 不走商店。
    public override bool IsAllowedInShops => false;
}

/// <summary>
/// 阿格莱雅专属遗物的发放池。用于「火种」满 12 层时随机发放，并保证不重复发放已拥有的。
/// </summary>
public static class AglaeaExclusiveRelicPool
{
    /// <summary>玩家尚未拥有的专属遗物实例（可变副本），供发放。</summary>
    public static List<RelicModel> GetUnobtained(Player player)
    {
        var result = new List<RelicModel>();

        if (player.Relics.All(r => r is not AglaeaAntiGravityBoots))
        {
            result.Add(ModelDb.Relic<AglaeaAntiGravityBoots>().ToMutable());
        }

        if (player.Relics.All(r => r is not AglaeaFirestormTide))
        {
            result.Add(ModelDb.Relic<AglaeaFirestormTide>().ToMutable());
        }

        if (player.Relics.All(r => r is not AglaeaLightSpeedPropeller))
        {
            result.Add(ModelDb.Relic<AglaeaLightSpeedPropeller>().ToMutable());
        }

        return result;
    }
}

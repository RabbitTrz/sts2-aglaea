using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 阿格莱雅专属遗物的公共基类（反重力皮靴 / 火力风暴潮 / 光速螺旋桨）。
///
/// 共同行为：
/// - 不进入遗物池（<see cref="Rarity"/> = None + <see cref="IsAllowedInShops"/> = false），
///   只能由「火种」累计满 12 层时发放。
/// - 每打出 <see cref="AttacksPerTrigger"/> 张攻击牌，触发一次 <see cref="OnAttackThresholdReached"/>。
///
/// 计数器随遗物实例存活；本类不持久化计数（跨战斗清零属于设计预期——每场战斗重新计数），
/// 如需跨战斗保留，子类可自行加 [SavedProperty]。
/// </summary>
public abstract class AglaeaExclusiveRelic : AglaeaRelics
{
    /// <summary>触发一次效果所需的攻击牌张数。</summary>
    protected virtual int AttacksPerTrigger => 5;

    private int _attackCount;

    public override RelicRarity Rarity => RelicRarity.None;

    // 不走商店/掉落池。
    public override bool IsAllowedInShops => false;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        // 只统计本次行动玩家自己的攻击牌（多人对局中避免统计他人）。
        if (cardPlay.Player != Owner)
        {
            return;
        }

        _attackCount++;
        if (_attackCount < AttacksPerTrigger)
        {
            return;
        }

        _attackCount = 0;
        await OnAttackThresholdReached(choiceContext);
    }

    /// <summary>每累计 <see cref="AttacksPerTrigger"/> 张攻击牌触发一次的效果。</summary>
    protected abstract Task OnAttackThresholdReached(PlayerChoiceContext choiceContext);
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

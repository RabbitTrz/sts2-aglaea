using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MoeNegiMod.Aglaea.Powers;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Commands;

/// <summary>
/// 「至高之姿」的移除入口。
///
/// 设计上区分两种失去至高之姿的方式（v0.1.3「静息蓄势」依赖这个区分）：
///
///   · **消耗** —— 至高之姿自身的效果：每打出一张攻击牌消耗 1 层。
///     走 <see cref="SupremePosePower"/> 自己的 AfterCardPlayed，**不**通知静息蓄势。
///
///   · **直接移除** —— 卡牌主动把层数清掉（绞丝 / 火种归还 / 史诗终结）。
///     走这里，移除后通知静息蓄势按层数给力量。
///
/// 层数归零时能力自动消失，也属于「消耗」那条路，不经过这里。
/// </summary>
public static class SupremePoseCmd
{
    /// <summary>
    /// 由卡牌直接移除玩家身上所有的至高之姿，并通知静息蓄势。
    /// </summary>
    /// <returns>实际移除的层数；玩家身上没有至高之姿时为 0。</returns>
    public static async Task<int> RemoveAllDirectly(
        PlayerChoiceContext choiceContext, Player player, AbstractModel? source)
    {
        var creature = player.Creature;
        var pose = creature.GetPower<SupremePosePower>();
        if (pose == null)
        {
            return 0;
        }

        int removed = pose.Amount;
        await PowerCmd.Remove<SupremePosePower>(creature);

        // 移除至高之姿会连带清空金丝（SupremePosePower.AfterRemoved 的既有设计）。

        if (removed > 0)
        {
            var rest = creature.GetPower<RestAccumulatePower>();
            if (rest != null)
            {
                await rest.OnSupremePoseRemovedDirectly(choiceContext, removed, source);
            }
        }

        return removed;
    }
}

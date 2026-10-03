using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 浪漫合奏（玩家身上的 Buff）：每当其他玩家打出攻击牌时，
/// 把玩家牌堆中的【浪漫合奏】从任意位置收回手牌。
///
/// 该 power 在【浪漫合奏】被打出时挂载，并持续到战斗结束（打出后本牌进入弃牌堆/消耗堆，
/// 仍可被本 power 找回）。
/// </summary>
public sealed class RomanticEnsemblePower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    // Single：重复打出不会带来额外收益，因此不显示层数（否则图标数字会误导玩家）。
    // 本 power 只是「浪漫合奏已生效」的记录器，与 TheHuntPower 的角色相同。
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 其他玩家打出「除浪漫合奏本身以外」的攻击牌时触发。
    /// </summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        // 仅响应「其他玩家」打出的攻击牌。
        if (cardPlay.Player == Owner.Player)
        {
            return;
        }

        // 「除本张以外」：其他玩家打出的恰好也是浪漫合奏时，不触发。
        if (cardPlay.Card is AglaeaRomanticEnsemble)
        {
            return;
        }

        // 找出所有属于本玩家的【浪漫合奏】（可能在弃牌堆/消耗堆/抽牌堆/出牌区）。
        var ensembles = Owner.Player.PlayerCombatState.AllCards
            .Where(c => c is AglaeaRomanticEnsemble && c.Pile.Type != PileType.Hand)
            .ToList();

        if (ensembles.Count == 0)
        {
            return;
        }

        await CardPileCmd.Add(ensembles, PileType.Hand);
    }
}

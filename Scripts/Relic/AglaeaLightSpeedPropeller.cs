using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 光速螺旋桨（专属遗物，不进池）：回合内每抽 6 张牌，获得 1 点力量。
/// 由「火种」累计满 12 层时发放。
///
/// 计数在玩家回合开始时清零，即「回合内」的语义。
/// v0.1.3 之前的效果是「每打出 5 张攻击牌放 1 张若虫+」，已替换。
/// </summary>
public class AglaeaLightSpeedPropeller : AglaeaExclusiveRelic
{
    /// <summary>每多少张抽牌给 1 点力量。</summary>
    private const int CardsPerTrigger = 6;

    private int _drawCount;

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        // 只数自己的抽牌。
        if (card.Owner != Owner)
        {
            return;
        }

        _drawCount++;
        if (_drawCount < CardsPerTrigger)
        {
            return;
        }

        _drawCount = 0;
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            _drawCount = 0;
        }
        return Task.CompletedTask;
    }
}

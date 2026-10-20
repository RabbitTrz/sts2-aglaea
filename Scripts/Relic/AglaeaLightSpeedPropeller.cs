using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 光速螺旋桨（专属遗物，不进池）：每抽 10 张牌，获得 1 点力量。
/// 由「火种」累计满 12 层时发放。
///
/// 计数跨回合累计，每场战斗开始时清零（牌面没有「本回合」限定）。
/// </summary>
public class AglaeaLightSpeedPropeller : AglaeaExclusiveRelic
{
    /// <summary>每多少张抽牌给 1 点力量。</summary>
    private const int CardsPerTrigger = 10;

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

    /// <summary>跨回合累计，所以只在战斗开始时清零。</summary>
    public override Task BeforeCombatStart()
    {
        _drawCount = 0;
        return Task.CompletedTask;
    }
}

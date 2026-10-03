using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Commands;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 光速螺旋桨（专属遗物，不进池）：每打出 5 张攻击牌，将 1 张【若虫+】放入抽牌堆。
/// 由「火种」累计满 12 层时发放。
/// </summary>
public class AglaeaLightSpeedPropeller : AglaeaExclusiveRelic
{
    protected override async Task OnAttackThresholdReached(PlayerChoiceContext choiceContext)
    {
        // 放入升级后的若虫（若虫+）。
        await TailorCmd.AddChrysalisToDrawPile(
            choiceContext, Owner, null, 1, CardPilePosition.Random, upgraded: true);
    }
}

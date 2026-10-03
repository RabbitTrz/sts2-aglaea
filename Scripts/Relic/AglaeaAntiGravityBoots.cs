using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 反重力皮靴（专属遗物，不进池）：每打出 5 张攻击牌，抽 1 张牌。
/// 由「火种」累计满 12 层时发放。
/// </summary>
public class AglaeaAntiGravityBoots : AglaeaExclusiveRelic
{
    protected override async Task OnAttackThresholdReached(PlayerChoiceContext choiceContext)
    {
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }
}

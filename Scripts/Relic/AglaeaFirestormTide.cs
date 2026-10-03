using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Powers;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 火力风暴潮（专属遗物，不进池）：每打出 5 张攻击牌，获得 1 层至高之姿。
/// 由「火种」累计满 12 层时发放。
/// </summary>
public class AglaeaFirestormTide : AglaeaExclusiveRelic
{
    protected override async Task OnAttackThresholdReached(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<SupremePosePower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }
}

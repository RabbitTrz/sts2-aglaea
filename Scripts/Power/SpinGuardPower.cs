using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 回纺为守（玩家身上的 Buff）：衣匠每攻击 1 次，玩家获得 Amount 点[gold]格挡[/gold]。
/// Amount 由「回纺为守」能力牌应用（1 点）。
/// </summary>
public sealed class SpinGuardPower : AglaeaPower, ITailorAttackListener
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public Task BeforeTailorAttack(
        PlayerChoiceContext choiceContext, Player player, Creature tailor, Creature target)
    {
        return Task.CompletedTask;
    }

    public async Task AfterTailorAttack(
        PlayerChoiceContext choiceContext, Player player, Creature tailor, Creature target, int damage)
    {
        if (Amount <= 0)
        {
            return;
        }
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
    }
}

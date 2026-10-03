using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 金丝成障（玩家身上的 Buff）：回合开始时，若玩家处于「至高之姿」，则获得[gold]格挡[/gold]。
/// [gold]格挡[/gold]数值由「金丝成障」能力牌应用（6 点，升级 9 点），存于 Amount。
/// </summary>
public sealed class GoldBarrierPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        if (Owner.GetPower<SupremePosePower>() == null)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
    }
}

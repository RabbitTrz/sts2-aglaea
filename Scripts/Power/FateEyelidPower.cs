using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 命运眼睑（玩家身上的 Buff，本回合限定）：本回合每消耗 1 层「至高之姿」，
/// 立即获得 1 层「至高之姿」。回合结束时移除。
/// </summary>
public sealed class FateEyelidPower : AglaeaPower, ISupremePoseConsumeListener
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public async Task OnSupremePoseConsumed(int layers)
    {
        if (layers <= 0)
        {
            return;
        }

        var player = Owner.Player;
        if (player == null)
        {
            return;
        }

        await PowerCmd.Apply<SupremePosePower>(
            new ThrowingPlayerChoiceContext(), Owner, layers, Owner, null);
    }

    // 本回合限定：玩家回合结束时移除。
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 用 participants 而不是 side 判断：多人「额外回合」时 playersEndingTurn
        // 只包含部分玩家，用 side 会把还没结束回合的队友的「本回合」效果一并清掉。
        // 与 ArrestPower 的写法保持一致。
        if (!participants.Contains(Owner))
        {
            return;
        }
        await PowerCmd.Remove(this);
    }
}

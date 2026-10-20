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
/// 绞紧（玩家身上的 Buff，本回合限定）：本回合触发「间隙织线」力量下降所需的攻击次数降为 1。
/// 由「绞紧」技能牌应用，回合结束时移除。
///
/// 本类只是【标记】：实际的阈值改写写在 <see cref="WeaveLinePower.HitsRequired"/> 里
/// （那里会检查全场是否有人挂着本 power）。旧版「力量下降翻倍」是遗留实现，已移除。
/// </summary>
public sealed class TwistTightPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeaveLinePower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

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

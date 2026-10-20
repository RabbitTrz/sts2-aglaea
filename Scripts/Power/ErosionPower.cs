// ============================================================
// 侵蚀（暂时搁置 2026-10-04）
//
// 机制本身已写好（回合结束判定「层数 > 生命值」死亡、每打 1 张攻击牌 -1 层），
// 但设计要求的「血条变黑」蹭不到现成实现 —— NHealthBar 把 DoomPower 写死在代码里
// （HasPower<DoomPower>() / GetPowerAmount<DoomPower>()），自定义 power 无效。
// 在「要不要给 NHealthBar 打补丁」定下来之前，黑潮蚀刃先做成白板小怪，
// 这个 power 整体注释停用。
//
// 启用时：解开下面的块注释，并把黑潮蚀刃接回去。
// 本地化文案保留（MOENEGIMOD-EROSION_POWER.*）。
// ============================================================
/*
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 侵蚀（玩家身上的 Debuff，Counter 叠层）：黑潮蚀刃每次攻击时给予 2 层。
///
/// · **回合结束时**，若层数 **大于** 当前生命值，则立即死亡。
/// · **每打出 1 张攻击牌，减少 1 层**。
///
/// 【表现待补】设计要求的「血条变黑」目前没有实现。
/// 原因：<c>NHealthBar</c> 把 <see cref="DoomPower"/> 写死在代码里
/// （`HasPower&lt;DoomPower&gt;()` / `GetPowerAmount&lt;DoomPower&gt;()` 决定那条黑色覆盖层），
/// 自定义 power 直接蹭不到。要做出同样效果，需要给 NHealthBar 打 Harmony 补丁。
/// </summary>
public sealed class ErosionPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DoomPower>()
    ];

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>每打出 1 张攻击牌减 1 层（只算自己的攻击牌）。</summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }
        if (cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        await PowerCmd.Decrement(this);
    }

    /// <summary>
    /// 回合结束时判定：层数 &gt; 当前生命值 → 立即死亡。
    /// 注意比较符与灾厄【不同】：灾厄是 <c>CurrentHp &lt;= Amount</c>（≥），
    /// 侵蚀按设计是严格大于，所以这里写 <c>CurrentHp &lt; Amount</c>。
    /// </summary>
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Amount <= 0)
        {
            return;
        }

        if (Owner.CurrentHp < Amount)
        {
            await CreatureCmd.Kill(Owner);
        }
    }
}
*/

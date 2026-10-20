// ============================================================
// 震荡（2026-10-05 取消）
//
// 用户决定：取消【天谴斗士】的震荡效果（攻击未被格挡时塞纠缠）。
// 天谴斗士现在只有【战甲】一个机制。
//
// 本地化文案保留（MOENEGIMOD-CONCUSSION_POWER.*），需要恢复时解开下面的块注释、
// 并在 AglaeaScourgeFighter.AfterAddedToRoom 里重新挂载即可。
// ============================================================
/*
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 震荡（天谴斗士自身 Buff，已停用）：
/// 它的攻击未被格挡时，在被攻击玩家的抽牌堆中加入 1 层【纠缠】。
/// </summary>
public sealed class ConcussionPower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<EntanglePower>()
    ];

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner)
        {
            return;
        }

        // 只对攻击生效（天谴斗士目前只有攻击伤害，留个保险）。
        if (!props.HasFlag(ValueProp.Move))
        {
            return;
        }

        // 未被格挡 = 不是「完全被挡下」。
        if (result.WasFullyBlocked)
        {
            return;
        }

        if (target.Player == null)
        {
            return;
        }

        await PowerCmd.Apply<EntanglePower>(choiceContext, target, 1, Owner, null);
    }
}
*/

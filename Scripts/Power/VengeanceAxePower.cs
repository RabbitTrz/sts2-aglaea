using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 复仇（黑潮蚀斧 / 黑潮蚀刃自身 Buff）：
/// 每当有其他敌人被消灭时（其所在阵营的其它生物死亡），获得 1 点[gold]力量[/gold]。
///
/// 「敌方目标」是设计文档中站在玩家视角的写法 —— 即自己的同阵营生物：
/// 玩家先杀爪牙会让它们越来越强（复仇）。
/// 2026-10-05：黑潮蚀刃也挂上了这个效果，显示名从「复仇之斧」改为「复仇」。
/// </summary>
public sealed class VengeanceAxePower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        // 只有真正死掉的同阵营生物才算「被消灭」；自己死了就不再触发。
        if (wasRemovalPrevented || creature == Owner || !Owner.IsAlive)
        {
            return;
        }

        if (creature.Side != Owner.Side)
        {
            return;
        }

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1, Owner, null);
    }
}

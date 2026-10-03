using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MoeNegiMod.Aglaea.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 锚定时刻：将 1 名敌人身上的临时力量下降效果变为永久。
/// 升级：保留（Retain）。
///
/// 实现方式：临时力量下降（<see cref="TemporaryStrengthPower"/> 的下降型子类，如
/// <see cref="MonarchsGazeStrengthDownPower"/> 王之凝视、<see cref="PiercingWailPower"/> 尖啸、
/// <see cref="ManglePower"/> 凌虐）已在其 BeforeApplied 时同步扣减了
/// <see cref="StrengthPower"/>，并在回合结束时移除自身、把扣掉的力量加回去。
/// 因此「变为永久」= 直接移除这个临时 power 而不触发它的返还逻辑，力量惩罚就留在身上。
///
/// 之所以成立：<see cref="TemporaryStrengthPower.AfterSideTurnEnd"/> 是**唯一**的返还路径，
/// 而 <see cref="TemporaryStrengthPower"/> 未重写 AfterRemoved（基类为 no-op）。
/// power 一旦被移除，该 hook 便不再触发，力量惩罚即永久化。
///
/// 认**任意来源**的临时力量下降（不只间隙织线体系），但排除临时力量**上升**
/// （见 <see cref="TemporaryStrengthExtension.GetTemporaryStrengthDown"/>）——
/// 否则对 FlexPotion 的 +2 力量套用同一逻辑会变成「永久 +2 力量」，语义完全相反。
/// </summary>
public class AglaeaAnchorMoment() : AglaeaCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<MonarchsGazeStrengthDownPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Target 可为 null（战斗外 CombatState 为 null），显式早返回与「命定共舞」等保持一致。
        if (cardPlay.Target == null)
        {
            return;
        }

        // 任意来源的临时力量下降（王之凝视、尖啸、凌虐等），排除临时力量上升。
        // 逐个移除【全部】符合条件的 power：同一个敌人身上可能同时挂着多个不同来源的
        // 临时力量下降（例如先被尖啸削过、又被凌虐削过），牌面要求把它们一并永久化。
        var powers = cardPlay.Target.GetTemporaryStrengthDowns();
        if (powers.Count == 0)
        {
            return;
        }

        // 移除临时力量下降，但不返还已扣除的力量 → 该力量下降变为永久。
        //
        // 表现：无需手写任何 VFX。移除会经
        //   PowerCmd.Remove → PowerModel.RemoveInternal → Creature.RemovePowerInternal
        //   → Creature.PowerRemoved 事件 → NCreature.OnPowerRemoved → NPowerRemovedVfx
        // 自动弹出原版「XX 效果结束」漂浮文字（vfx 配置键 POWER_WEARS_OFF + 该 power 的标题与图标）。
        // 该表现受 PowerModel.ShouldPlayVfx 控制，而 TemporaryStrengthPower 未将其置 false，
        // 因此这里天然有反馈。
        //
        // 注意：必须先 ToList 快照再遍历 —— PowerCmd.Remove 会改动 Creature 的 Powers 集合，
        // 直接遍历原集合会抛「集合已被修改」。
        foreach (var power in powers)
        {
            await PowerCmd.Remove(power);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}

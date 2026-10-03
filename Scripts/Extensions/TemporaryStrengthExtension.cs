using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Linq;

namespace MoeNegiMod.Aglaea.Extensions;

/// <summary>
/// 临时力量（<see cref="TemporaryStrengthPower"/>）的查询辅助。
///
/// 背景：<see cref="TemporaryStrengthPower"/> 同时表示「临时力量**上升**」和「临时力量**下降**」，
/// 由子类的 <c>IsPositive</c> 区分：
///   · 上升（IsPositive = true）  —— 如 FlexPotionPower（药水）、CoordinatePower、
///                                    FeedingFrenzyPower、ReptileTrinketPower、SetupStrikePower
///   · 下降（IsPositive = false） —— 如 MonarchsGazeStrengthDownPower（王之凝视）、
///                                    PiercingWailPower（尖啸）、ManglePower（凌虐）、
///                                    CrushUnderPower、DarkShacklesPower、DyingStarPower、
///                                    EnfeeblingTouchPower、ShacklingPotionPower
///
/// 区分方式：<see cref="PowerModel.TypeForCurrentAmount"/>。因为所有这些子类都未重写
/// <c>AllowNegative</c>（默认 false），<c>TypeForCurrentAmount</c> 直接返回 <c>Type</c>，
/// 而 <c>Type</c> 由 <c>IsPositive</c> 决定 —— 下降者为 <see cref="PowerType.Debuff"/>。
///
/// 「锚定时刻」要把临时力量下降永久化、「致命浪漫」要按临时力量下降给衣匠增伤，
/// 两者都需要这个判定，故集中于此，避免两处各写一份而漂移。
/// </summary>
internal static class TemporaryStrengthExtension
{
    /// <summary>
    /// 取目标身上「临时力量**下降**」的 power（任意来源）。
    /// 临时力量上升（如 FlexPotion 的 +2 力量）会被排除，无则返回 null。
    /// </summary>
    public static TemporaryStrengthPower? GetTemporaryStrengthDown(this Creature? creature)
    {
        var power = creature?.GetPower<TemporaryStrengthPower>();
        if (power == null)
        {
            return null;
        }

        return power.TypeForCurrentAmount == PowerType.Debuff ? power : null;
    }

    /// <summary>
    /// 取目标身上【全部】「临时力量**下降**」的 power（任意来源）。
    /// <see cref="Creature.GetPower{T}"/> 只返回第一个匹配实例，而同一个敌人身上可能
    /// 同时挂着多个不同来源的临时力量下降（例如先被尖啸削过、又被凌虐削过），
    /// 所以「锚定时刻」要一次性全部拿到时不能走单数接口。
    /// </summary>
    public static List<TemporaryStrengthPower> GetTemporaryStrengthDowns(this Creature? creature)
    {
        if (creature == null)
        {
            return [];
        }

        return creature.Powers
            .OfType<TemporaryStrengthPower>()
            .Where(p => p.TypeForCurrentAmount == PowerType.Debuff)
            .ToList();
    }

    /// <summary>目标身上是否有「临时力量下降」效果（任意来源）。</summary>
    public static bool HasTemporaryStrengthDown(this Creature? creature)
    {
        return creature.GetTemporaryStrengthDown() != null;
    }
}

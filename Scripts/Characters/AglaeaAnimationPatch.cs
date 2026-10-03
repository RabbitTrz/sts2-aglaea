using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Character;

/// <summary>
/// 把原版的 Spine 动画触发桥接到本 mod 静态图立绘上的 sprite 动画。
///
/// 原版 <c>NCreature.SetAnimationTrigger</c> 只做 <c>_spineAnimator?.SetTrigger(trigger)</c>；
/// 静态图占位没有 Spine 骨架，整个触发被静默吞掉。
///
/// 驱动节点不在这里挂载，而是由 <c>AglaeaSpriteDriver</c> 在自身 _Ready 时接管
/// <c>%Visuals</c>。转换后的 <see cref="NCreatureVisuals"/> 是 BaseLib 用
/// <c>new</c> 造出来的（见 <c>NodeFactory.CreateFromNode</c>），
/// 它的 _Ready 永远不会被 Godot 调用，所以驱动由本补丁在第一次触发时补挂。
///
/// 只对本 mod 自己的静态图立绘生效（Aglaea 与衣匠），其它角色仍走原版 Spine 路径。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
internal static class AglaeaAnimationPatch
{
    /// <summary>
    /// 原版触发名 → 本场景的动画名。
    /// 原版常量见 <c>CreatureAnimator</c>：Idle / Attack / Cast / PowerUp / Dead / Hit / Revive。
    /// </summary>
    private static readonly Dictionary<string, string> TriggerToAnim = new()
    {
        ["Idle"] = "idle",
        ["Attack"] = "attack",
        ["Hit"] = "hit",

        // 目前只做了 idle / attack / hit，其余触发先就近映射：
        // 能力牌与技能牌暂时借用 attack 的姿势。
        // Dead / Revive 不在这里——它们走 PlayDeath / PlayRevive 的补间演出。
        ["Cast"] = "attack",
        ["PowerUp"] = "attack",
    };

    /// <summary>播完要回到 idle 的一次性动画。</summary>
    private static readonly HashSet<string> OneShotAnims = new() { "attack", "hit" };

    private static void Postfix(NCreature __instance, string trigger)
    {
        if (!TriggerToAnim.TryGetValue(trigger, out string? animName))
        {
            return;
        }

        AglaeaSpriteDriver? driver = FindDriver(__instance);
        if (driver == null)
        {
            return;
        }

        if (OneShotAnims.Contains(animName))
        {
            driver.PlayOneShot(animName);
        }
        else
        {
            driver.Play(animName);
        }
    }

    /// <summary>
    /// 在 creature 的视觉节点里找本 mod 的立绘驱动；不是本 mod 的立绘就返回 null。
    /// 转换后的节点是 BaseLib <c>new</c> 出来的，_Ready 不会跑，
    /// 所以驱动永远不会自己挂上——这里必须补挂。
    /// </summary>
    internal static AglaeaSpriteDriver? FindDriver(NCreature creature)
    {
        NCreatureVisuals? visuals = creature.Visuals;
        if (visuals == null || !AglaeaVisuals.IsOurVisuals(visuals))
        {
            return null;
        }

        AglaeaSpriteDriver? driver = visuals.GetNodeOrNull<AglaeaSpriteDriver>(nameof(AglaeaSpriteDriver));
        if (driver == null)
        {
            driver = new AglaeaSpriteDriver();
            visuals.AddChild(driver);
        }
        return driver;
    }
}

/// <summary>
/// 死亡时起播死亡演出。
///
/// 为什么不走动画触发：原版 <c>NCreature.StartDeathAnim</c> 里发 "Dead" 的前提是
/// <c>_spineAnimator != null</c>，静态图立绘没有 Spine 骨架，那条分支进不去；
/// 而 <c>IDeathDelayer</c> 那条路只对怪物有效（玩家的 shouldRemove 恒为 false）。
/// 所以直接补在 StartDeathAnim 上，这是所有角色死亡都会经过的公共入口。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
internal static class AglaeaDeathPatch
{
    private static void Postfix(NCreature __instance)
    {
        AglaeaAnimationPatch.FindDriver(__instance)?.PlayDeath();
    }
}

/// <summary>
/// 复活时解除死亡演出状态。
///
/// 为什么不走动画触发：原版 <c>NCreature.StartReviveAnim</c> 里发 "Revive" 的前提是
/// <c>_spineAnimator.HasTrigger("Revive")</c>，静态图立绘没有 Spine 骨架、<c>_spineAnimator</c>
/// 为 null，那条分支根本进不去（会改走原版的 <c>AnimTempRevive</c> 闪一下）。
/// 所以这里直接补在 StartReviveAnim 上：原版负责把 modulate 拉回不透明，
/// 我们负责把 _dying 标记清掉，否则复活后所有动画都会被挡住。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartReviveAnim))]
internal static class AglaeaRevivePatch
{
    private static void Postfix(NCreature __instance)
    {
        AglaeaAnimationPatch.FindDriver(__instance)?.PlayRevive();
    }
}

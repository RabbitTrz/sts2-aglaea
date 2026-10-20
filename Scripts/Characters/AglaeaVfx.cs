using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MoeNegiMod.Aglaea.Powers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Character;

/// <summary>
/// 本 mod 的特效接入点。
///
/// 两个挂点（都抄的原版做法）：
/// · <b>全屏演出</b>：挂 <c>NCombatRoom.CombatVfxContainer</c>（原版【肾上腺素】【戏剧登场】同款），
///   z 序在战场之上、所有 UI 之下 —— 正好「盖住画面但不盖 UI」。
/// · <b>形态特效</b>（至高之姿的金色环）：挂 <c>NCreatureVisuals.FormVfxHolder</c>
///   （原版恶魔形态/<c>NDemonFormVfx</c> 同款挂点），随角色移动、战斗结束一起销毁。
/// </summary>
public static class AglaeaVfx
{
    /// <summary>求解大招的全屏演出（1920×1080 序列帧，播一次）。</summary>
    public const string SolveFullscreenScenePath = "res://Aglaea/scene/Vfx/solve_fullscreen.tscn";

    /// <summary>至高之姿的环状形态特效（循环）。</summary>
    public const string SupremePoseAuraScenePath = "res://Aglaea/scene/Vfx/supreme_pose_aura.tscn";

    /// <summary>获得至高之姿的技能牌打出的全屏演出（华珠之礼 / 苦难织者 / 披姿 / 蓄姿）。</summary>
    public const string AglaeaSupremeFullscreenScenePath = "res://Aglaea/scene/Vfx/aglaea_supreme_fullscreen.tscn";

    /// <summary>
    /// 播放全屏演出但**不等它播完**（卡牌演出用：牌照常结算，动画自己播完消失）。
    /// </summary>
    public static void PlayFullscreenNoWait(Creature? spawner, string scenePath, bool allySide)
    {
        _ = PlayFullscreen(spawner, scenePath, allySide);
    }

    /// <summary>
    /// 本场战斗是否已经播过「获得至高之姿」的登场演出。
    /// key 是战斗状态对象，战斗结束后对象被回收，计数自然失效（不用手动清）。
    /// </summary>
    private static readonly ConditionalWeakTable<ICombatState, object> SupremeIntroPlayed = new();

    /// <summary>
    /// 「获得至高之姿」类卡牌的登场演出（华珠之礼 / 苦难织者 / 披姿 / 蓄姿）。
    /// 只有两个条件同时满足才播：
    /// ① 当前至高之姿层数为 0（不是从已有层数继续叠加）；
    /// ② 本场战斗还没播过 —— 每场战斗最多一次。
    /// </summary>
    public static void PlaySupremeIntroOnce(Creature? creature)
    {
        if (creature == null)
        {
            return;
        }

        // 已经有层数：这次不是「第一次获得」，不播。
        if (creature.GetPower<SupremePosePower>() is { Amount: > 0 })
        {
            return;
        }

        ICombatState? combat = creature.CombatState;
        if (combat == null)
        {
            return;
        }

        if (SupremeIntroPlayed.TryGetValue(combat, out _))
        {
            return;
        }

        SupremeIntroPlayed.Add(combat, new object());
        PlayFullscreenNoWait(creature, AglaeaSupremeFullscreenScenePath, allySide: true);
    }

    /// <summary>
    /// 播放一次全屏演出，**等它播完**再返回（调用方接着结算）。
    /// 时长从场景里的 AnimatedSprite2D 现读（帧数 ÷ 帧率），改帧率不用改代码。
    /// <paramref name="allySide"/> 决定受哪一个游戏内开关管：true = 我方长特效，false = 敌方长特效。
    /// </summary>
    public static async Task PlayFullscreen(Creature? spawner, string scenePath, bool allySide)
    {
        // 游戏内设置里对应的「长特效」关闭时跳过（战斗结算不受影响）。
        if (!(allySide ? AglaeaConfig.AllyLongVfx : AglaeaConfig.EnemyLongVfx))
        {
            return;
        }

        Control? container = spawner?.GetVfxContainer();
        if (container == null)
        {
            return;
        }

        Node2D node = PreloadManager.Cache.GetScene(scenePath)
            .Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
        container.AddChildSafely(node);
        node.GlobalPosition = NGame.Instance.GetViewportRect().Size * 0.5f;

        await Cmd.Wait(GetAnimationSeconds(node));
        node.QueueFree();
    }

    /// <summary>
    /// 把形态特效挂到角色身上（**画在角色身后**），返回节点（供移除）。
    /// 找不到角色节点（战斗外/测试模式）时返回 null。
    ///
    /// 为什么不用原版恶魔形态的 <c>FormVfxHolder</c>：那个挂点在立绘之后，特效会盖在角色上；
    /// 而 z_index 解决不了「背景之上、角色之下」——战场容器整体是 ZIndex -10，
    /// 再减 1 就沉到背景底下（踩过这个坑）。所以直接挂到立绘节点、并插到**子节点最前**：
    /// 与立绘同 z、但更早绘制 → 正好在角色身后、背景之上。
    /// </summary>
    public static Node2D? AttachFormVfx(Creature creature, string scenePath)
    {
        NCreature? creatureNode = creature.GetCreatureNode();
        NCreatureVisuals? visuals = creatureNode?.Visuals;
        if (visuals == null)
        {
            MainFile.Logger.Warn("[Aglaea] 形态特效：找不到角色立绘节点，特效未挂载。");
            return null;
        }

        Node2D vfx = PreloadManager.Cache.GetScene(scenePath)
            .Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
        visuals.AddChildSafely(vfx);
        visuals.MoveChild(vfx, 0);
        vfx.Position = Vector2.Zero;

        MainFile.Logger.Info(
            $"[Aglaea] 形态特效已挂载：场景={scenePath} 挂点={visuals.Name}（子节点第 0 位，角色身后）可见={vfx.Visible}");
        return vfx;
    }

    /// <summary>读出节点下 "Sprite"（AnimatedSprite2D）当前动画的总时长。</summary>
    private static float GetAnimationSeconds(Node node)
    {
        if (node.GetNodeOrNull<AnimatedSprite2D>("Sprite") is not AnimatedSprite2D sprite)
        {
            return 0f;
        }

        SpriteFrames frames = sprite.SpriteFrames;
        string anim = sprite.Animation;
        int count = frames.GetFrameCount(anim);
        double total = 0;
        for (int i = 0; i < count; i++)
        {
            total += frames.GetFrameDuration(anim, i);
        }

        double speed = frames.GetAnimationSpeed(anim);
        return speed > 0 ? (float)(total / speed) : (float)total;
    }
}

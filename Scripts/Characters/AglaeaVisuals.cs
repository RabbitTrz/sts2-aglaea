using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Character;

/// <summary>
/// Aglaea 的战斗立绘接入点。
///
/// 为什么不用 <c>.tscn</c> 里的 <c>script = ExtResource(...)</c>：
/// 导出的 pck 会排除 <c>Scripts/*.cs</c>（C# 靠编译好的 dll 运行），
/// 于是场景里的脚本引用在运行时解析不到类，整棵场景实例化失败、
/// 退化成 placeholder 节点（日志：Cannot instantiate C# script ... / Cannot get class 'NCreatureVisuals'）。
/// 所以场景里只留纯节点。
///
/// 为什么不能用 <c>NCreatureVisuals._Ready</c> 的 Harmony Postfix：
/// BaseLib 的转换发生在 <c>PackedScene.Instantiate</c> 的 Postfix 里（SceneConversionPatch），
/// 它拿到的是场景原本的 <c>Node2D</c> 实例（此时 _Ready 已在 Node2D 上跑过），
/// 然后 <c>new NCreatureVisuals()</c> 造一个全新节点、把原 Node2D 的子节点搬过去，
/// 最后替换 <c>Instantiate</c> 的返回值。这个新节点从未进入场景树，
/// Godot 永远不会调用它的 <c>_Ready</c> —— 该 Postfix 对它必然静默。
///
/// 转换后的节点无法靠路径识别：BaseLib 走 FlexibleStructure 分支时
/// 只把原场景的子节点搬进新节点，原 Node2D 本身被丢弃，
/// 于是 SceneFilePath 在整个节点树上都变成空串（实测：Aglaea 的
/// <c>visuals.SceneFilePath == ''</c>，而原版 nibbit 是
/// <c>'res://scenes/creature_visuals/nibbit.tscn'</c>）。
/// 所以改为按结构指纹识别，见 <see cref="IsOurVisuals"/>。
///
/// 动画状态机由 <c>AglaeaAnimationPatch</c> 驱动：它监听原版的
/// <c>NCreature.SetAnimationTrigger</c>，把 Spine 触发名翻译成这里的 sprite 动画名。
/// </summary>
public static class AglaeaVisuals
{
    /// <summary>Aglaea 战斗立绘场景的 res:// 路径。</summary>
    public const string ScenePath = "res://Aglaea/scene/Battle/aglaea.tscn";

    /// <summary>衣匠战斗立绘场景的 res:// 路径。</summary>
    public const string TailorScenePath = "res://Aglaea/scene/Battle/tailor.tscn";

    /// <summary>火堆立绘场景：静态图，节点结构照抄原版 ironclad_rest_site.tscn。</summary>
    public const string RestScenePath = "res://Aglaea/scene/Rest/aglaea_rest.tscn";

    /// <summary>商店立绘场景：静态图，Sprite 必须是第一个子节点。</summary>
    public const string MerchantScenePath = "res://Aglaea/scene/Merchant/aglaea_merchant.tscn";

    /// <summary>
    /// 在 BaseLib 的自动转换表里登记两个立绘场景，让它们被转成 <see cref="NCreatureVisuals"/>。
    ///
    /// 只能注册「类型」而不能带回调：BaseLib 在角色/怪物模型加载时会走
    /// <c>RegisterSceneConversions()</c>，用 <c>null</c> 回调重新注册同一路径，
    /// 而 <c>RegisterSceneType</c> 是无条件覆盖字典的（同类型时不告警，但照样覆盖），
    /// 回调必被抹掉。所以驱动的挂载改由 Harmony 补丁完成，见 <c>AglaeaAnimationPatch</c>。
    ///
    /// 必须在 Harmony 打好补丁之后调用一次。
    /// </summary>
    public static void Register()
    {
        NodeFactory.RegisterSceneType<NCreatureVisuals>(ScenePath);
        NodeFactory.RegisterSceneType<NCreatureVisuals>(TailorScenePath);

        // 火堆 / 商店立绘同样是「静态图场景 → 引擎节点」的转换，各注册一次。
        NodeFactory.RegisterSceneType<NRestSiteCharacter>(RestScenePath);
        NodeFactory.RegisterSceneType<NMerchantCharacter>(MerchantScenePath);
    }

    /// <summary>
    /// 判断一个视觉节点是否来自本 mod 的某个静态图立绘场景（Aglaea 或衣匠）。
    ///
    /// 不能看 <c>SceneFilePath</c>：转换后它一定是空串（原 Node2D 被 BaseLib 丢弃了）。
    /// 改按结构指纹识别，两个条件同时成立才算：
    ///
    /// 1. <c>%Visuals</c> 是 <see cref="AnimatedSprite2D"/> —— 原版所有敌人的
    ///    <c>%Visuals</c> 都是 <c>Node2D</c>（Spine 骨架挂在下面），只有静态图占位是直接一个 AnimatedSprite2D。
    /// 2. 存在 <c>%FormVfx</c> —— 原版敌人的视觉场景里没有这个节点，这是角色专用的形态特效容器。
    ///
    /// 实测日志：Aglaea 的 children 为
    /// <c>[AnimatedSprite2D(Visuals), Control(FormVfx), Control(Bounds), Marker2D(CenterPos), Marker2D(IntentPos)]</c>，
    /// 而同场战斗的 nibbit / osty 均为
    /// <c>[Node2D(Visuals), Control(Bounds), Marker2D(CenterPos), Marker2D(IntentPos)]</c>。
    /// </summary>
    public static bool IsOurVisuals(NCreatureVisuals visuals)
    {
        return visuals.GetNodeOrNull<Node2D>("%Visuals") is AnimatedSprite2D
            && visuals.HasNode("%FormVfx");
    }
}

/// <summary>
/// 真正驱动 <c>%Visuals</c> 上那个 <see cref="AnimatedSprite2D"/> 的节点。
///
/// 一次性动画（attack / hit）的收尾不依赖 <c>AnimationFinished</c> 信号——实测该信号
/// 在此场景下不可靠。改为按帧时长自行计时，到点强制切回 idle。
///
/// 同时实现 <see cref="IDeathDelayer"/>：原版 <c>NCreature.AnimDie</c> 会
/// <c>await</c> 所有实现该接口的子节点的 <c>GetDelayTask()</c>，等它完成才 <c>QueueFree</c>。
/// 这是官方给的扩展点，死亡演出靠它才播得完——否则静态图角色会被立刻移除，什么都看不见。
/// </summary>
public partial class AglaeaSpriteDriver : Node, IDeathDelayer
{
    private const string IdleAnim = "idle";

    /// <summary>死亡演出的标记名。不要求场景里有同名帧动画——它由补间驱动，不是帧序列。</summary>
    private const string DeathAnim = "die";

    /// <summary>本场景的 <c>%Visuals</c> 节点，即那个 AnimatedSprite2D。可能为 null。</summary>
    public AnimatedSprite2D? Sprite { get; private set; }

    /// <summary>当前正在播放的动画名，用于避免重复 Play 导致动画被重置。</summary>
    private string _playing = "";

    /// <summary>当前一次性动画的收尾计时器。非空表示有一次性的 attack/hit 正在播。</summary>
    private SceneTreeTimer? _oneShotTimer;

    /// <summary>立绘在场景里的原始位置。抖动是相对它做的偏移，结束后要摆回去。</summary>
    private Vector2 _basePosition;

    /// <summary>是否处于死亡演出中。期间忽略一切常规动画触发，只有复活能解除。</summary>
    private bool _dying;

    private Tween? _shakeTween;
    private Tween? _fadeTween;

    /// <summary>死亡演出的完成信号。原版 AnimDie 会 await 它，等它结束才移除角色。</summary>
    private TaskCompletionSource<bool>? _deathCompletion;

    public override void _Ready()
    {
        // 挂在 NCreatureVisuals 下，用相对路径取它的 %Visuals。
        Node? parent = GetParent();
        Sprite = parent?.GetNodeOrNull<Node2D>("%Visuals") as AnimatedSprite2D;

        if (Sprite == null)
        {
            MainFile.Logger.Warn(
                "[Aglaea] 立绘场景的 %Visuals 不是 AnimatedSprite2D，静态图动画不可用。");
            return;
        }

        _basePosition = Sprite.Position;
        MainFile.Logger.Info($"[Aglaea] 立绘驱动就绪，frames={DescribeAnimations()}");
        Play(IdleAnim);
    }

    /// <summary>
    /// 播放指定名字的循环动画；名字不存在或已在播放时不做任何事。
    /// 死亡演出期间一律忽略——死人不会回到待机。
    /// </summary>
    public void Play(string animName)
    {
        if (Sprite == null || _dying || _playing == animName)
        {
            return;
        }
        if (!Sprite.SpriteFrames.HasAnimation(animName))
        {
            return;
        }

        CancelOneShot();
        ResetVisualState();
        _playing = animName;
        Sprite.Play(animName);
    }

    /// <summary>
    /// 死亡演出：原地抖几下，同时渐隐到透明，之后保持透明直到复活。
    ///
    /// 用补间而不是帧序列，是为了不改美术资源就能有效果——抖动幅度、时长
    /// 都在这里调。等以后画了正经的 die 帧动画，把这里换回 PlayOneShot("die") 即可。
    /// </summary>
    public void PlayDeath()
    {
        if (Sprite == null || _dying)
        {
            return;
        }

        CancelOneShot();
        CancelDeath();

        _dying = true;
        _playing = DeathAnim;
        _deathCompletion = new TaskCompletionSource<bool>();

        // 左右来回摆，幅度递减——像脱力前最后几下。
        float[] offsets = { -16f, 13f, -10f, 8f, -6f, 4f, -2f, 0f };
        const float shakeStep = 0.05f;

        _shakeTween = CreateTween();
        foreach (float dx in offsets)
        {
            _shakeTween.TweenProperty(
                Sprite, "position", _basePosition + new Vector2(dx, 0f), shakeStep);
        }

        // 渐隐比抖动慢一点收尾，让抖动看得见。
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(Sprite, "modulate:a", 0f, shakeStep * offsets.Length + 0.3f);
        _fadeTween.TweenCallback(Callable.From(OnDeathAnimFinished));
    }

    /// <summary>
    /// 原版 <c>NCreature.AnimDie</c> 会 <c>await</c> 这个 Task，等它完成才移除角色节点。
    /// 只有怪物（含宠物）会走到这里——玩家的 <c>shouldRemove</c> 恒为 false，由
    /// <c>StartDeathAnim</c> 的补丁负责起播。演出已经起播时不重复触发，直接返回同一个完成信号。
    /// </summary>
    public Task GetDelayTask()
    {
        if (Sprite == null)
        {
            return Task.CompletedTask;
        }

        PlayDeath();
        return _deathCompletion?.Task ?? Task.CompletedTask;
    }

    private void OnDeathAnimFinished()
    {
        _deathCompletion?.TrySetResult(true);
        _deathCompletion = null;
    }

    /// <summary>复活：解除死亡状态，恢复不透明与原始位置，回到待机。</summary>
    public void PlayRevive()
    {
        if (Sprite == null)
        {
            return;
        }

        CancelDeath();
        _dying = false;
        ResetVisualState();
        Play(IdleAnim);
    }

    /// <summary>把立绘恢复到原始位置和不透明（死亡演出改过这两个值）。</summary>
    private void ResetVisualState()
    {
        if (Sprite == null)
        {
            return;
        }

        Sprite.Position = _basePosition;
        Color modulate = Sprite.Modulate;
        modulate.A = 1f;
        Sprite.Modulate = modulate;
    }

    private void CancelDeath()
    {
        _shakeTween?.Kill();
        _shakeTween = null;
        _fadeTween?.Kill();
        _fadeTween = null;

        // 被中途取消时也要放行等待者，否则原版 AnimDie 会一直 await 下去、角色永不消失。
        _deathCompletion?.TrySetResult(true);
        _deathCompletion = null;
    }

    /// <summary>
    /// 播放一次性动画（attack / hit），到时自动回到 idle。
    ///
    /// 场景里没有该动画时静默降级为 idle —— 衣匠目前只有 idle / attack 两个动画，
    /// 受击触发会走到这里。
    /// </summary>
    public void PlayOneShot(string animName)
    {
        if (Sprite == null || _dying)
        {
            return;
        }

        if (!Sprite.SpriteFrames.HasAnimation(animName))
        {
            Play(IdleAnim);
            return;
        }

        // 上一次的一次性动画可能还没播完就被新的打断（比如受击插队），
        // 先取消旧计时器，避免它提前把新动画切回 idle。
        CancelOneShot();
        ResetVisualState();

        _playing = animName;
        Sprite.Play(animName);

        float duration = GetAnimationDuration(animName);
        MainFile.Logger.Info($"[Aglaea] one-shot '{animName}' duration={duration}");

        SceneTree? tree = GetTree();
        if (duration <= 0f || tree == null)
        {
            Play(IdleAnim);
            return;
        }

        _oneShotTimer = tree.CreateTimer(duration);
        _oneShotTimer.Timeout += OnOneShotTimeout;
    }

    private void OnOneShotTimeout()
    {
        _oneShotTimer = null;
        _playing = IdleAnim;
        if (Sprite != null && Sprite.SpriteFrames.HasAnimation(IdleAnim))
        {
            Sprite.Play(IdleAnim);
        }
    }

    /// <summary>取消尚未触发的一次性动画收尾。</summary>
    private void CancelOneShot()
    {
        if (_oneShotTimer == null)
        {
            return;
        }

        _oneShotTimer.Timeout -= OnOneShotTimeout;
        _oneShotTimer = null;
    }

    /// <summary>
    /// 累加指定动画所有帧的时长（秒），再除以动画 speed。
    /// </summary>
    private float GetAnimationDuration(string animName)
    {
        if (Sprite == null)
        {
            return 0f;
        }

        SpriteFrames frames = Sprite.SpriteFrames;
        int count = frames.GetFrameCount(animName);
        if (count <= 0)
        {
            return 0f;
        }

        float total = 0f;
        for (int i = 0; i < count; i++)
        {
            total += frames.GetFrameDuration(animName, i);
        }

        double speed = frames.GetAnimationSpeed(animName);
        return speed > 0.0 ? (float)(total / speed) : total;
    }

    /// <summary>诊断用：列出场景里所有动画名与帧数。</summary>
    private string DescribeAnimations()
    {
        if (Sprite == null)
        {
            return "(no sprite)";
        }

        SpriteFrames frames = Sprite.SpriteFrames;
        List<string> parts = new();
        foreach (StringName name in frames.GetAnimationNames())
        {
            parts.Add($"{name}:{frames.GetFrameCount(name)}f/loop={frames.GetAnimationLoop(name)}");
        }
        return string.Join(", ", parts);
    }
}

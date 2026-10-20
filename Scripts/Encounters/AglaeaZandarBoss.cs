using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MoeNegiMod.Aglaea.Monsters;
using System.Collections.Generic;

namespace MoeNegiMod.Aglaea.Encounters;

/// <summary>
/// v0.1.4 Boss 遭遇战：第一位天才，隐德来希，赞达尔。
///
/// 开场只有赞达尔一个敌人，天谴斗士由【秘所思公理】召唤、黑潮蚀刃/斧由【DEFRAG 生命优化】召唤。
/// 槽位顺序 = 敌方行动顺序（颂达尔永远第 1 个行动，召唤物排在它后面）。
///
/// 测试：`fight MOENEGIMOD-AGLAEA_ZANDAR_BOSS`（开发控制台）。
/// </summary>
public sealed class AglaeaZandarBoss : CustomEncounterModel
{
    public AglaeaZandarBoss() : base(RoomType.Boss)
    {
    }

    /// <summary>
    /// 槽位名即场景里的 Marker2D 名；顺序 = 敌方行动顺序。
    /// 赞达尔占第 1 位（站在正上方），之后是 4 个爪牙位（下方从左到右弧形排开）。
    /// 4 个位置足够：演化形态「小怪 ≥3 时改放潜能」把场上小怪压到 4 只以内，
    /// 论证形态的 DEFRAG 也只补到「含赞达尔共 5 个」= 4 只小怪。
    /// </summary>
    public override IReadOnlyList<string> Slots =>
        new[] { "zandar", "add1", "add2", "add3", "add4" };

    /// <summary>遭遇战场景：只含 7 个 Marker2D（BaseLib 的 Slots 就是从这个场景的直接子节点读的）。</summary>
    public override string? CustomScenePath => "res://Aglaea/scene/Encounters/zandar_boss.tscn";

    /// <summary>Boss 地图节点图标（PNG + _outline.png，352×300，放 mod 自己的目录）。</summary>
    public override string BossNodePath => "res://Aglaea/images/map/zandar_icon";

    /// <summary>没有 Spine 资源 —— 走 PNG 分支（同原版几个用 placeholder 图标的 Boss）。</summary>
    public override MegaSkeletonDataResource? BossNodeSpineResource => null;

    /// <summary>
    /// 自定义 BGM：返回哨兵字符串，由 <see cref="Audio.ZandarBossMusic"/> 的补丁接管 ——
    /// 停掉游戏音乐、直接播放我们打包的音频文件（游戏只认 FMOD 事件，播文件要自己接管）。
    /// </summary>
    public override string CustomBgm => Audio.ZandarBossMusic.Sentinel;

    /// <summary>自定义战斗背景：赞达尔专属的全屏背景（单层，BaseLib 的动态背景场景）。</summary>
    public override BackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets(
            "res://BaseLib/scenes/dynamic_background.tscn",
            new List<string> { "res://Aglaea/scene/Backgrounds/zandar_bg.tscn" },
            null);
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
    {
        ModelDb.Monster<AglaeaZandar>(),
        ModelDb.Monster<AglaeaScourgeFighter>(),
        ModelDb.Monster<AglaeaTideBlade>(),
        ModelDb.Monster<AglaeaTideAxe>(),
    };

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        // 开场只有赞达尔自己；爪牙全部由它自己召唤。
        return new (MonsterModel, string?)[]
        {
            (ModelDb.Monster<AglaeaZandar>().ToMutable(), Slots[0]),
        };
    }

    /// <summary>
    /// 章节投放位置【留空】—— 用户 2026-10-04 裁定「稍后处理」。
    ///
    /// 恒返回 false：这个遭遇战不会进入任何一章的 Boss 池，正常跑图完全不受影响。
    /// 想测试就用开发控制台：`fight MOENEGIMOD-AGLAEA_ZANDAR_BOSS`。
    /// 以后定了章节归属，把对应章节 return true 即可。
    /// </summary>
    public override bool IsValidForAct(ActModel act) => false;
}

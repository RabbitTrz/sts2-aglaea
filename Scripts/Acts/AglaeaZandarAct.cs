using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using MoeNegiMod.Aglaea.Encounters;
using System;
using System.Collections.Generic;

namespace MoeNegiMod.Aglaea.Acts;

/// <summary>
/// 第四章（Index 3）：隐德来希。
///
/// 极短单线章节：事件「往昔的涟漪」 → 商店 → 赞达尔 Boss，没有常规战斗。
/// 这张地图是自绘的（<see cref="AglaeaZandarActMap"/>），起点是问号事件而不是 Ancient ——
/// 原版每章开头必有古神事件，但游戏本身允许没有（<c>RoomSet.HasAncient</c> 的注释写明），
/// 只要起点不是 <c>MapPointType.Ancient</c>，就不会去读 <c>PullAncient()</c>，也就不会因 ancient 为空而抛异常。
///
/// 放进 <c>ModelDb.Acts</c> 后章节数自动变成 4：
/// 第三幕 Boss 打完 <c>EnterNextAct</c> 会进入本章，本章 Boss 打完才算通关。
/// </summary>
public sealed class AglaeaZandarAct : CustomActModel
{
    /// <summary>显示用的章节序号（第 4 幕）。</summary>
    public AglaeaZandarAct() : base(4)
    {
    }

    /// <summary>Index 决定它在章节列表里的位置。原版 0/1/2 = 密林（或暗港）/ 巢穴 / 荣耀。</summary>
    public override int Index => 3;

    /// <summary>
    /// 必须为 true：<c>ActModel.GetDefaultList()</c> 要求每个 index 都有默认章节，
    /// 否则测试/空状态（NullRunState）会抛 "No default act for index 3!"。
    /// </summary>
    public override bool IsDefault => true;

    public override bool IsUnlocked(UnlockState unlockState) => true;

    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        // 本章只有 Boss 战，BOSS 因此必然是赞达尔。
        return new[] { ModelDb.Encounter<AglaeaZandarBoss>() };
    }

    public override IEnumerable<EncounterModel> BossDiscoveryOrder
        => new[] { ModelDb.Encounter<AglaeaZandarBoss>() };

    protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
    {
    }

    /// <summary>没有 Ancient（地图起点是问号事件，不会被查询）。</summary>
    public override IEnumerable<AncientEventModel> AllAncients => Array.Empty<AncientEventModel>();

    public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState unlockState)
        => Array.Empty<AncientEventModel>();

    /// <summary>
    /// 本类不直接列事件；「往昔的涟漪」由 BaseLib 按 <c>AglaeaRippleEvent.Acts</c> 自动并入，
    /// 并由 <c>Act4EventRoomPatch</c> 固定抽取。
    /// </summary>
    public override IEnumerable<EventModel> AllEvents => Array.Empty<EventModel>();

    /// <summary>自绘单线地图（往昔的涟漪 → 商店 → Boss）。</summary>
    protected override ActMap CustomCreateMap(RunState runState, bool replaceTreasureWithElites)
        => new AglaeaZandarActMap(runState);

    /// <summary>
    /// 仅用于「继续游戏」界面估算总楼层。原版公式是 BaseNumberOfRooms + 2（Ancient + Boss），
    /// 本章没有 Ancient，所以填 1 才能让估算楼层等于实际的「事件 + 商店 + Boss」3 层。
    /// </summary>
    protected override int BaseNumberOfRooms => 1;

    public override MapPointTypeCounts GetMapPointTypes(Rng mapRng) => new(0, 0);

    // ───────────────────────── 占位素材（全部复用第三章「荣耀」） ─────────────────────────
    // 等第四章专属美术到位后再逐项替换；替换时只需改这里的路径。

    protected override string CustomBackgroundScenePath
        => "res://scenes/backgrounds/glory/glory_background.tscn";

    protected override BackgroundAssets CustomGenerateBackgroundAssets(Rng rng) => new("glory", rng);

    protected override string CustomMapTopBgPath
        => "res://images/packed/map/map_bgs/glory/map_top_glory.png";

    protected override string CustomMapMidBgPath
        => "res://images/packed/map/map_bgs/glory/map_middle_glory.png";

    protected override string CustomMapBotBgPath
        => "res://images/packed/map/map_bgs/glory/map_bottom_glory.png";

    protected override string CustomRestSiteBackgroundPath
        => "res://scenes/rest_site/glory_rest_site.tscn";

    public override string[] BgMusicOptions
        => new[] { "event:/music/act3_a1_v1", "event:/music/act3_a2_v1" };

    public override string[] MusicBankPaths
        => new[] { "res://banks/desktop/act3_a1.bank", "res://banks/desktop/act3_a2.bank" };

    public override string AmbientSfx => "event:/sfx/ambience/act3_ambience";

    // 本章没有宝箱，纯粹是防御性复用，避免以后加宝箱时报资源缺失。
    public override string ChestSpineResourcePath
        => "res://animations/backgrounds/treasure_room/chest_room_act_3_skel_data.tres";

    public override string ChestSpineSkinNameNormal => "act3";

    public override string ChestSpineSkinNameStroke => "act3_stroke";

    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";

    // 地图配色暂用第三章的，和占位地图背景配套。
    public override Color MapTraveledColor => new("1D1E2F");

    public override Color MapUntraveledColor => new("60717C");

    public override Color MapBgColor => new("819A97");
}

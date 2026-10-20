using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Audio;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Audio;

/// <summary>
/// 赞达尔 Boss 战的自定义 BGM。
///
/// 游戏音乐控制器只认 FMOD 事件（<c>CustomBgm</c> / <c>NRunMusicController.PlayCustomMusic</c>），
/// 所以做法是：遭遇战返回一个**哨兵字符串**，我们在补丁里拦下它 —— 停掉游戏当前音乐、
/// 用 BaseLib 的 <c>FmodAudio</c> 直接播我们的音频文件（FMOD 原生支持 mp3/wav/ogg）。
///
/// 音频文件以裸字节形式打包在 pck 里（<c>Aglaea/music/zandar_boss.bin</c>；
/// 用 .bin 是为了让 Godot 不把它当资源导入），首次播放时解包到 <c>user://</c>，
/// 因为 FMOD 需要真实的文件系统路径。
///
/// 停播：游戏的 <c>StopMusic</c>（游戏结束/退出）与战斗结束时的 <c>UpdateTrack</c>、
/// 以及其它遭遇战调用 <c>PlayCustomMusic</c> 时，都会把我们这条音轨停掉。
/// </summary>
public static class ZandarBossMusic
{
    /// <summary>遭遇战 CustomBgm 返回这个哨兵，补丁据此接管。</summary>
    public const string Sentinel = "MOENEGIMOD-ZANDAR-BOSS-BGM";

    /// <summary>打包在 pck 里的音频（.bin = 原始字节，避免被 Godot 导入成资源）。</summary>
    private const string PackedResourcePath = "res://Aglaea/music/zandar_boss.bin";

    /// <summary>
    /// 解包到的缓存位置（FMOD 要真实路径，所以放 user://）。
    /// 换音频文件时记得改版本号（v2/v3…），否则旧缓存会被直接复用、新音频不生效。
    /// </summary>
    private const string CachePath = "user://zandar_boss_v2.mp3";

    /// <summary>
    /// 自定义 BGM 的播放音量，来自游戏内设置（模组配置 → 音频 → BGM 音量，0~100）。
    /// 文件播放不经过游戏的音乐总线混音，满音量会把音效盖住，所以默认只有 45%。
    /// </summary>
    private static float BgmVolume => AglaeaConfig.BgmVolumePercent / 100f;

    /// <summary>当前播放的音乐实例（用于停播）。</summary>
    private static GodotObject? _instance;

    private static string? _resolvedPath;

    /// <summary>播放我们的 BGM：先停掉游戏当前音乐，再播文件。</summary>
    internal static void Play(NRunMusicController controller)
    {
        string? path = ResolveFilePath();
        if (path == null)
        {
            MainFile.Logger.Warn("[Aglaea] 赞达尔 BGM 文件不可用，本次不播放自定义音乐。");
            return;
        }

        float volume = BgmVolume;
        if (volume <= 0f)
        {
            // 音量设成 0：直接不播（不能指望实例音量——实测它压不住这条音轨）。
            MainFile.Logger.Info("[Aglaea] BGM 音量设为 0，本场不播放自定义音乐。");
            return;
        }

        StopOurTrack();   // 保险：若上一次播放的实例还活着，先停掉，避免两条音轨叠在一起变响
        StopGameMusic(controller);

        LogBusVolumes("播放前");
        LogInstanceCount("播放前");

#pragma warning disable CS0618 // BaseLib 的 FmodAudio 标记为 Obsolete，但这是目前唯一能播任意音频文件的入口
        _instance = FmodAudio.PlayMusic(path, volume);
#pragma warning restore CS0618
        MainFile.Logger.Info($"[Aglaea] 赞达尔 BGM 开始播放（{(_instance != null ? "成功" : "失败")}，设定音量 {volume}）：{path}");

        // 诊断：实例数量变化 + 音量回读。若数量 +2 或回读值与我们设置的不同，
        // 说明「文件即音乐」这条通道另有我们控制不到的播放实例 / 无视音量参数。
        LogInstanceCount("播放后");
        LogInstanceVolume();
        LogBusVolumes("播放后");
    }

    /// <summary>诊断：FmodServer 当前的活动实例数量。</summary>
    private static void LogInstanceCount(string stage)
    {
        try
        {
            GodotObject? server = Engine.GetSingleton("FmodServer");
            MainFile.Logger.Info($"[Aglaea] FMOD 实例数量（{stage}）：{server?.Call("get_instance_count")}");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Aglaea] 读取 FMOD 实例数量失败：{ex.Message}");
        }
    }

    /// <summary>诊断：回读我们这条音轨实例自身的音量。</summary>
    private static void LogInstanceVolume()
    {
        if (_instance == null)
        {
            return;
        }

        try
        {
            MainFile.Logger.Info($"[Aglaea] BGM 实例音量回读：{_instance.Call("get_volume")}");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Aglaea] 回读 BGM 实例音量失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 诊断：打出三条音频总线的当前音量。
    /// 用来判断「音效听不见」是音量盖住（总线数值正常）还是播放把音效总线弄哑了（sfx 变成 0）。
    /// </summary>
    private static void LogBusVolumes(string stage)
    {
#pragma warning disable CS0618
        try
        {
            MainFile.Logger.Info(
                $"[Aglaea] 音频总线音量（{stage}）：master={FmodAudio.GetBusVolume("bus:/master")} " +
                $"sfx={FmodAudio.GetBusVolume("bus:/master/sfx")} music={FmodAudio.GetBusVolume("bus:/master/music")}");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Aglaea] 读取音频总线音量失败：{ex.Message}");
        }
#pragma warning restore CS0618
    }

    /// <summary>停掉我们的音轨（游戏音乐切换/战斗结束/离开时调用）。</summary>
    internal static void StopOurTrack()
    {
        if (_instance == null)
        {
            return;
        }

        try
        {
            _instance.Call("stop");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Aglaea] 停止赞达尔 BGM 失败：{ex.Message}");
        }

        _instance = null;
    }

    /// <summary>调用游戏音乐控制器的私有 Proxy 停掉当前音乐（只停音乐，不动别的东西）。</summary>
    private static void StopGameMusic(NRunMusicController controller)
    {
        try
        {
            FieldInfo? proxyField = AccessTools.Field(typeof(NRunMusicController), "_proxy");
            if (proxyField?.GetValue(controller) is Node proxy)
            {
                proxy.Call("stop_music");
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Aglaea] 停止游戏当前音乐失败：{ex.Message}");
        }
    }

    /// <summary>把 pck 里的音频解包到 user:// 并返回真实路径；失败返回 null。</summary>
    private static string? ResolveFilePath()
    {
        if (_resolvedPath != null)
        {
            return _resolvedPath;
        }

        string global = ProjectSettings.GlobalizePath(CachePath);
        try
        {
            if (!File.Exists(global) || new FileInfo(global).Length == 0)
            {
                Godot.FileAccess? packed = Godot.FileAccess.Open(PackedResourcePath, Godot.FileAccess.ModeFlags.Read);
                if (packed == null)
                {
                    MainFile.Logger.Warn($"[Aglaea] 找不到打包的 BGM：{PackedResourcePath}");
                    return null;
                }

                byte[] bytes = packed.GetBuffer((long)packed.GetLength());
                packed.Close();

                Godot.FileAccess? cache = Godot.FileAccess.Open(CachePath, Godot.FileAccess.ModeFlags.Write);
                if (cache == null)
                {
                    MainFile.Logger.Warn($"[Aglaea] 写入 BGM 缓存失败：{global}");
                    return null;
                }

                cache.StoreBuffer(bytes);
                cache.Close();
                MainFile.Logger.Info($"[Aglaea] BGM 已解包到 {global}（{bytes.Length} 字节）");
            }

            _resolvedPath = global;
            return global;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Aglaea] 解包 BGM 失败：{ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// 拦下遭遇战的哨兵 BGM：不走游戏的事件流程，改播我们的文件。
/// 其它遭遇战的音乐照常（顺便把我们自己的音轨停掉）。
/// </summary>
[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.PlayCustomMusic))]
internal static class ZandarBossMusicPlayPatch
{
    [HarmonyPrefix]
    private static bool Prefix(NRunMusicController __instance, string customMusic)
    {
        if (customMusic != ZandarBossMusic.Sentinel)
        {
            ZandarBossMusic.StopOurTrack();
            return true;
        }

        ZandarBossMusic.Play(__instance);
        return false;
    }
}

/// <summary>
/// 战斗结束（胜利/失败）时 <c>UpdateTrack()</c> 会被调用一次 —— 此时停掉我们的音轨。
/// 注意必须显式给出空参数表：<c>NRunMusicController</c> 有两个 UpdateTrack 重载，
/// 只写方法名会让 Harmony 报 "Ambiguous match" 并导致整个 mod 初始化失败。
/// </summary>
[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateTrack), new Type[0])]
internal static class ZandarBossMusicCombatEndPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (!CombatManager.Instance.IsInProgress)
        {
            ZandarBossMusic.StopOurTrack();
        }
    }
}

/// <summary>游戏整体停音乐（游戏结束、退出等）时一并停掉我们的音轨。</summary>
[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.StopMusic))]
internal static class ZandarBossMusicStopPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        ZandarBossMusic.StopOurTrack();
    }
}

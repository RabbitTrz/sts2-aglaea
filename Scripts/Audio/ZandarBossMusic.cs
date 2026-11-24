using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Audio;
using System;
using System.Reflection;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Audio;

/// <summary>
/// 赞达尔 Boss 战的自定义 BGM。
///
/// 遭遇战返回一个哨兵字符串 <see cref="Sentinel"/>，补丁拦下它后停掉游戏当前音乐，
/// 再用 Godot 自带的 <see cref="AudioStreamPlayer"/> + <see cref="AudioStreamMP3"/> 播放。
///
/// 音频文件以裸字节形式打包在 pck 里（<c>Aglaea/music/zandar_boss.bin</c>；用 .bin 是为了
/// 让 Godot 不把它当资源导入）。播放时直接从 pck 读进内存建流：
/// 不落盘、不经过 FMOD、不依赖文件路径 —— 旧实现把 mp3 解包到 user:// 再交给 FMOD
/// 文件流播放，在中文用户名（如 C:/Users/陌千刀/...）的机器上进入 Boss 房会原生崩溃。
/// </summary>
public static class ZandarBossMusic
{
    /// <summary>遭遇战 CustomBgm 返回这个哨兵，补丁据此接管。</summary>
    public const string Sentinel = "MOENEGIMOD-ZANDAR-BOSS-BGM";

    /// <summary>打包在 pck 里的音频（.bin = 原始字节，避免被 Godot 导入成资源）。</summary>
    private const string PackedResourcePath = "res://Aglaea/music/zandar_boss.bin";

    /// <summary>
    /// 自定义 BGM 的播放音量，来自游戏内设置（模组配置 → 音频 → BGM 音量，0~100）。
    /// 默认 45%，避免盖住音效。
    /// </summary>
    private static float BgmVolume => AglaeaConfig.BgmVolumePercent / 100f;

    /// <summary>当前播放节点（用于停播）。</summary>
    private static AudioStreamPlayer? _player;

    /// <summary>流对象需要保持引用，避免播放中被 GC 回收。</summary>
    private static AudioStreamMP3? _stream;

    /// <summary>pck 内的 mp3 字节缓存（约 4.9 MB，只在第一次播放时读取）。</summary>
    private static byte[]? _cachedBytes;

    /// <summary>播放我们的 BGM：先停掉游戏当前音乐，再播放内存中的 MP3。</summary>
    internal static void Play(NRunMusicController controller)
    {
        float volume = BgmVolume;
        if (volume <= 0f)
        {
            MainFile.Logger.Info("[Aglaea] BGM 音量设为 0，本场不播放自定义音乐。");
            return;
        }

        byte[]? bytes = LoadPackedBytes();
        if (bytes == null)
        {
            return;
        }

        StopOurTrack();   // 保险：若上一次播放的节点还活着，先停掉，避免两条音轨叠在一起变响
        StopGameMusic(controller);

        try
        {
            _stream = new AudioStreamMP3
            {
                Data = bytes,
                Loop = true,
            };
            _player = new AudioStreamPlayer
            {
                Name = "AglaeaZandarBgm",
                Stream = _stream,
                VolumeDb = Mathf.LinearToDb(volume),
                Bus = "Master",
            };
            controller.AddChild(_player);
            _player.Play();

            MainFile.Logger.Info(
                $"[Aglaea] 赞达尔 BGM 开始播放（AudioStreamMP3，{bytes.Length} 字节，设定音量 {volume}）。");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Aglaea] 赞达尔 BGM 播放失败：{ex.Message}");
            StopOurTrack();
        }
    }

    /// <summary>把 pck 里的音频读进内存；失败返回 null。</summary>
    private static byte[]? LoadPackedBytes()
    {
        if (_cachedBytes != null)
        {
            return _cachedBytes;
        }

        try
        {
            using Godot.FileAccess? packed = Godot.FileAccess.Open(PackedResourcePath, Godot.FileAccess.ModeFlags.Read);
            if (packed == null)
            {
                MainFile.Logger.Warn($"[Aglaea] 找不到打包的 BGM：{PackedResourcePath}");
                return null;
            }

            _cachedBytes = packed.GetBuffer((long)packed.GetLength());
            return _cachedBytes;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Aglaea] 读取打包 BGM 失败：{ex.Message}");
            return null;
        }
    }

    /// <summary>停掉我们的音轨（游戏音乐切换/战斗结束/离开时调用）。</summary>
    internal static void StopOurTrack()
    {
        if (_player != null)
        {
            try
            {
                if (GodotObject.IsInstanceValid(_player))
                {
                    _player.Stop();
                    _player.QueueFree();
                }
            }
            catch (Exception ex)
            {
                MainFile.Logger.Warn($"[Aglaea] 停止赞达尔 BGM 失败：{ex.Message}");
            }
        }

        _player = null;
        _stream = null;
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

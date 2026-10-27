using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Compat;

/// <summary>
/// BaseLib 3.4.7 的兼容补丁。
///
/// BaseLib 给 <c>RunManager.CleanUp</c> 挂了 postfix：在 <c>NetService</c> 上注销自定义消息处理器。
/// 但读档流程（<c>NMainMenu.OnContinueButtonPressedAsync</c>）在失败时会调用
/// <c>RunManager.CleanUp()</c>——如果失败发生在 <c>InitializeShared</c> 之前（此时 NetService 还是 null），
/// BaseLib 的注销就会抛 NullReferenceException，把原始异常吞掉，表现为「点继续游戏后卡死」。
///
/// 这里给注销加一个空值保护：NetService 为 null 时跳过，让游戏照常弹出读档错误、
/// 并把真正的异常写进日志。
/// </summary>
[HarmonyPatch(typeof(CustomMessageWrapper), "Unregister")]
internal static class CustomMessageUnregisterGuardPatch
{
    [HarmonyPrefix]
    private static bool Prefix(INetGameService messageBuffer)
    {
        if (messageBuffer != null)
        {
            return true;
        }

        MainFile.Logger.Warn(
            "[Aglaea] BaseLib.CustomMessageWrapper.Unregister 收到 null NetService，已跳过（避免读档失败时二次崩溃）");
        return false;
    }
}

/// <summary>
/// 同上，BaseLib 还有一套「按地图点定向」的消息处理器（<see cref="CustomTargetedMessageWrapper"/>），
/// 注销时用的是 <c>RunLocationTargetedMessageBuffer</c>——读档失败路径上它同样是 null，
/// 会紧接着前一个 NRE 继续抛。一并加空值保护。
/// </summary>
[HarmonyPatch(typeof(CustomTargetedMessageWrapper), "Unregister")]
internal static class CustomTargetedMessageUnregisterGuardPatch
{
    [HarmonyPrefix]
    private static bool Prefix(RunLocationTargetedMessageBuffer messageBuffer)
    {
        if (messageBuffer != null)
        {
            return true;
        }

        MainFile.Logger.Warn(
            "[Aglaea] BaseLib.CustomTargetedMessageWrapper.Unregister 收到 null 缓冲区，已跳过（避免读档失败时二次崩溃）");
        return false;
    }
}

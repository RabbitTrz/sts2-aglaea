using System.Reflection;
using BaseLib.Config;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Timeline;

namespace Aglaea;

/**
 * Ideas
 * 
 * Self Bind
 * 
 * Bind effect - square texture based on model size, lines random generated (amount equal to bind amount)
 * shader of transparency of line based on average of point spread of the model
 * colored
 * 
 * Bind... rename? Necrobinder kinda overlaps.
 * */

[ModInitializer(nameof(Initialize))]
public class MainFile
{
    public const string ModId = "Aglaea"; //At the moment, this is used only for the Logger and harmony names.

    public static Logger Logger { get; } =
        new(ModId, LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);

        harmony.PatchAll();

        // 自检：把补丁结果写进日志，避免以后再出现「补丁静默没装上」而只能靠猜的情况。
        int patchCount = 0;
        foreach (MethodBase method in harmony.GetPatchedMethods())
        {
            Patches? info = Harmony.GetPatchInfo(method);
            int count = info?.Postfixes.Count ?? 0;
            patchCount += count;
            Logger.Info($"[Aglaea] 已补丁 {method.DeclaringType?.Name}.{method.Name} postfix={count}");
        }
        Logger.Info($"[Aglaea] Harmony 补丁自检完成，共 {patchCount} 个 postfix");

        // 登记立绘场景，让 BaseLib 把它转成 NCreatureVisuals。
        MoeNegiMod.Aglaea.Character.AglaeaVisuals.Register();

        // 注册游戏内「模组配置」（设置菜单里的 Mod Configuration）：长特效开关。
        ModConfigRegistry.Register(ModId, new MoeNegiMod.Aglaea.AglaeaConfig());
    }
}
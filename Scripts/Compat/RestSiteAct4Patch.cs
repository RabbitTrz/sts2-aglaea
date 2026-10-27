using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Compat;

/// <summary>
/// 原版 <c>NRestSiteCharacter._Ready</c> 用 <c>CurrentActIndex</c> 的 switch 选 Spine 动画名，
/// 只写了 0/1/2，其它值直接 <c>throw new InvalidOperationException("Unexpected act")</c>。
/// 第四章（Index 3）进休息点时会抛异常，导致后面连接焦点/悬浮事件的代码全部跳过。
///
/// 这里把默认分支重定向到第三章的 <c>glory_loop</c> 分支。
/// 阿格莱雅的休息点立绘是静态 Sprite2D（没有 Spine 节点），动画名本身没有影响。
/// </summary>
[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
internal static class RestSiteAct4Patch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        // 先定位「第三章」分支（默认分支要跳到这里）。
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "glory_loop"));
        if (matcher.IsInvalid)
        {
            MainFile.Logger.Warn(
                "[Aglaea] 未找到 NRestSiteCharacter._Ready 的 glory 分支，第四章休息点补丁未生效。");
            return instructions;
        }

        int gloryPos = matcher.Pos;

        // 再定位默认分支：ldstr "Unexpected act" / newobj InvalidOperationException / throw。
        matcher.Start().MatchStartForward(
            new CodeMatch(OpCodes.Ldstr, "Unexpected act"),
            new CodeMatch(OpCodes.Newobj,
                AccessTools.Constructor(typeof(InvalidOperationException), new[] { typeof(string) })),
            new CodeMatch(OpCodes.Throw));
        if (matcher.IsInvalid)
        {
            MainFile.Logger.Warn(
                "[Aglaea] 未找到 NRestSiteCharacter._Ready 的默认抛错分支，第四章休息点补丁未生效。");
            return instructions;
        }

        // 默认分支前面是一条跳转（switch 越界时 br.s 到这里）；把它改指向 glory 分支。
        CodeInstruction defaultBranch = matcher.InstructionAt(matcher.Pos - 1);
        if (defaultBranch.opcode != OpCodes.Br_S && defaultBranch.opcode != OpCodes.Br)
        {
            MainFile.Logger.Warn(
                $"[Aglaea] 默认分支前的指令不是跳转（{defaultBranch.opcode}），第四章休息点补丁未生效。");
            return instructions;
        }

        matcher.CreateLabelAt(gloryPos, out Label gloryLabel);
        defaultBranch.operand = gloryLabel;

        MainFile.Logger.Info("[Aglaea] 已补丁 NRestSiteCharacter._Ready：第四章休息点复用第三章动画。");
        return matcher.InstructionEnumeration();
    }
}

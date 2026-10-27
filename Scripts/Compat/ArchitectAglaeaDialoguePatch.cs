using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;
using System.Collections.Generic;

namespace MoeNegiMod.Aglaea.Compat;

/// <summary>
/// 建筑师结局事件的 Aglaea 对话。
///
/// 原版 <c>TheArchitect.DefineDialogues()</c> 只登记了 5 个原版角色；Aglaea 不在里面时
/// <c>LoadDialogue()</c> 找不到对话，结局会直接进「继续」。
///
/// 这里在 DefineDialogues 返回后插入一条 Aglaea 的重复对话：
///   第 1 行 Aglaea → 第 2 行 建筑师 → 第 3 行 Aglaea，
///   结束时 <see cref="ArchitectAttackers.Both"/>：玩家先攻击、建筑师再攻击，然后通关。
/// 文本在 <c>localization/zhs/ancients.json</c> 的
/// <c>THE_ARCHITECT.talk.MOENEGIMOD-AGLAEA.0-*r</c> 键里。
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "DefineDialogues")]
internal static class ArchitectAglaeaDialoguePatch
{
    /// <summary>Aglaea 的角色 ID（与 localization 里的键一致）。</summary>
    internal const string CharacterEntry = "MOENEGIMOD-AGLAEA";

    [HarmonyPostfix]
    private static void Postfix(ref AncientDialogueSet __result)
    {
        // 3 个空字符串 = 3 行对话、不播放专属语音（走原版默认音效）。
        __result.CharacterDialogues[CharacterEntry] = new List<AncientDialogue>
        {
            new AncientDialogue("", "", "")
            {
                VisitIndex = 0,
                IsRepeating = true,
                EndAttackers = ArchitectAttackers.Both,
            },
        };
    }
}

using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 让「欧洛巴斯之触」把阿格莱雅的起始遗物【黄金手链】升级为【熔光手链】。
///
/// <see cref="TouchOfOrobas.GetUpgradedStarterRelic"/> 内部维护一张私有的
/// starter→upgraded 映射表，未命中时回退成 <see cref="Circlet"/>。mod 无法直接改这张表，
/// 因此这里在 postfix 里覆盖回退结果：只要是阿格莱雅的起始遗物，就返回熔光手链。
/// </summary>
[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
internal static class AglaeaOrobasPatch
{
    [HarmonyPostfix]
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is AglaeaToy)
        {
            __result = ModelDb.Relic<AglaeaMoltenBracelet>().ToMutable();
        }
    }
}

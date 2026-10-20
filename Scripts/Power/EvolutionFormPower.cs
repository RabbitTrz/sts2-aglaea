using MegaCrit.Sts2.Core.Entities.Powers;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 演化形态（赞达尔战斗开始时持有的 Buff，Counter 倒数）：
/// 每当有生物生成（敌方目标从场上生成、我方召唤物被召唤）时 -1 层；
/// 层数归零时本 power 消失，赞达尔的下一个行动意图变为【SET 论证方法=黑潮】。
///
/// Amount = 距离形态终结还差几次「生物生成」，图标上的数字直接就是它（5 → 1）。
/// 层数增减由 <see cref="Monsters.AglaeaZandar"/> 的 AfterCreatureAddedToCombat 驱动。
/// </summary>
public sealed class EvolutionFormPower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}

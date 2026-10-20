using MegaCrit.Sts2.Core.Entities.Powers;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 论证形态（赞达尔在论证形态期间持有的 Buff，Counter 倒数）：
/// 每当有生物死亡（敌方目标被消灭、我方角色无法战斗、召唤物死亡）时 -1 层；
/// 层数归零时本 power 消失，赞达尔的下一个行动意图变为【求解：第一因之果】。
///
/// Amount = 距离形态终结还差几次「生物死亡」，图标上的数字直接就是它（10 → 1）。
/// 层数增减由 <see cref="Monsters.AglaeaZandar"/> 的 AfterDeath 驱动；
/// 进入论证形态时由 SET 施加，求解结算后随形态结束一并清除。
/// </summary>
public sealed class ArgumentFormPower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}

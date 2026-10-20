using MegaCrit.Sts2.Core.Entities.Powers;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// DEFRAG 生命优化（赞达尔在论证形态期间持有的 Buff）：
/// 效果在「进入论证形态」的那一刻就已经执行完 —— 场上敌方目标不足 5 个时，
/// 随机召唤【黑潮蚀刃】或【黑潮蚀斧】补足，被召唤者获得【律法的惩戒】。
///
/// 持有期间本身没有持续效果，它只是让玩家看得见「这个形态有这个机制」；
/// 施放【求解：第一因之果】时随 CALL 一起被移除。
/// </summary>
public sealed class DefragPower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}

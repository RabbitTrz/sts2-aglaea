using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 律法的惩戒（DEFRAG 召唤出的爪牙身上的 Debuff）：
/// 它**行动后**（完成一次攻击后），失去最大生命值 25% 的生命（向下取整，不吃格挡）。
///
/// v0.1.4 调整：生效时点从「回合开始时」改为「行动后」——爪牙先攻击，再承受惩戒。
/// 这些爪牙目前只有攻击招式，所以用 <see cref="AfterAttack"/> 代表「行动后」；
/// 如果以后给它们加非攻击行动，需要换一个 per-move 的钩子。
///
/// 这些爪牙由【DEFRAG 生命优化】生成、会在【求解：第一因之果】时随其他敌人一起死亡，
/// 所以不需要额外的持续时间（回合数）处理。
/// </summary>
public sealed class PunishmentOfLawPower : AglaeaPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterAttack(
        PlayerChoiceContext choiceContext, AttackCommand command)
    {
        // 只结算自己完成的攻击；已经死了就不用再扣。
        if (command.Attacker != Owner || !Owner.IsAlive)
        {
            return;
        }

        // 「失去生命」不是伤害：绕过格挡、不触发受击类钩子，与衣匠攻击扣血同一套写法。
        Owner.LoseHpInternal(Owner.MaxHp / 4, ValueProp.Unblockable | ValueProp.Unpowered);

        // LoseHpInternal 只改血量、不处理死亡，必须补一刀 Kill 才会走死亡流程。
        if (!Owner.IsAlive)
        {
            await CreatureCmd.Kill(Owner);
        }
    }
}

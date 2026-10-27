using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MoeNegiMod.Aglaea.Character;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Monsters;

/// <summary>
/// 黑潮蚀刃（赞达尔的爪牙）：35 血，只有一个循环招式【锋芒】（3 伤害 ×2）。
///
/// 【复仇】（<see cref="VengeanceAxePower"/>，2026-10-05 用户新增）：
/// 每当有其他敌人被消灭时，获得 1 点力量 —— 与黑潮蚀斧同款效果。
///
/// 【侵蚀之刃】机制（每次攻击给予 2 层【侵蚀】）**暂时搁置**（2026-10-04 用户决定）：
/// 侵蚀要求「血条变黑」，而原版 NHealthBar 把灾厄 DoomPower 写死在代码里，自定义 power
/// 蹭不到那个表现；相关 power 已写好并整段注释，见 <c>Scripts/Power/ErosionPower.cs</c>。
/// </summary>
public sealed class AglaeaTideBlade : CustomMonsterModel
{
    /// <summary>【锋芒】单次伤害与次数（进阶 9 单次伤害 +1）。</summary>
    private static int EdgeDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    private const int EdgeHits = 2;

    // 进阶 8：生命值 +5。
    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 35);

    public override int MaxInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 35);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<VengeanceAxePower>(
            new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // 单个意图循环：锋芒（攻击 ×2）。
        MoveState edge = new MoveState(
            "EDGE_MOVE",
            EdgeMove,
            new MultiAttackIntent(EdgeDamage, EdgeHits));
        edge.FollowUpState = edge;

        return new MonsterMoveStateMachine(new MonsterState[] { edge }, edge);
    }

    private async Task EdgeMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(EdgeDamage)
            .WithHitCount(EdgeHits)
            .FromMonster(this)
            .OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.2f)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    protected override string VisualsPath => AglaeaVisuals.TideBladeScenePath;
}

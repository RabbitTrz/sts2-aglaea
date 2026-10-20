using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MoeNegiMod.Aglaea.Character;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Monsters;

/// <summary>
/// 黑潮蚀斧（赞达尔的爪牙）：25 血，只有一个循环招式【怒潮】。
///
/// 机制【复仇之斧】：每当有其他敌人被消灭时，获得 1 点力量
/// （<see cref="VengeanceAxePower"/>，由 <see cref="AfterAddedToRoom"/> 挂载）。
/// 由【DEFRAG 生命优化】随机召唤出场，出场时带【律法的惩戒】。
/// </summary>
public sealed class AglaeaTideAxe : CustomMonsterModel
{
    /// <summary>【怒潮】伤害（v0.1.4 单人数值；进阶 8/9 版本暂不做）。</summary>
    private const int SurgeDamage = 10;

    public override int MinInitialHp => 25;

    public override int MaxInitialHp => 25;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<VengeanceAxePower>(
            new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // 单个意图循环：怒潮（攻击）。
        MoveState surge = new MoveState(
            "SURGE_MOVE",
            SurgeMove,
            new SingleAttackIntent(SurgeDamage));
        surge.FollowUpState = surge;

        return new MonsterMoveStateMachine(new MonsterState[] { surge }, surge);
    }

    private async Task SurgeMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SurgeDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.3f)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    protected override string VisualsPath => AglaeaVisuals.TideAxeScenePath;
}

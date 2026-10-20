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
/// 天谴斗士（赞达尔的爪牙）：50 血，只有一个循环招式【冲撞】。
///
/// 机制（由 <see cref="AfterAddedToRoom"/> 挂载）：
/// · 【战甲】：每被攻击 4 次，向所有玩家手牌放入 1 张【战甲】（<see cref="WarplatePower"/>）。
///   （2026-10-05 取消原【震荡】效果。）
///
/// 由赞达尔的【秘所思公理】以每次 2 个的数量召唤出场。
/// </summary>
public sealed class AglaeaScourgeFighter : CustomMonsterModel
{
    /// <summary>【冲撞】伤害（v0.1.4 单人数值；进阶 8/9 版本暂不做）。</summary>
    private const int ChargeDamage = 3;

    public override int MinInitialHp => 50;

    public override int MaxInitialHp => 50;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();

        // amount 只作占位：真正的阈值（1 + 玩家数 × 3）由 power 的 AfterApplied 设置。
        await PowerCmd.Apply<WarplatePower>(
            new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // 单个意图循环：冲撞（攻击 + 卡牌污染）。
        MoveState charge = new MoveState(
            "CHARGE_MOVE",
            ChargeMove,
            new SingleAttackIntent(ChargeDamage),
            new StatusIntent(1));
        charge.FollowUpState = charge;

        return new MonsterMoveStateMachine(new MonsterState[] { charge }, charge);
    }

    private async Task ChargeMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(ChargeDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.25f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }

    protected override string VisualsPath => AglaeaVisuals.ScourgeFighterScenePath;
}

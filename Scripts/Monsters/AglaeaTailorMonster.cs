using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MoeNegiMod.Aglaea.Character;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Monsters;

/// <summary>
/// 衣匠（召唤物/宠物）：阿格莱雅召唤的战士。
/// 拥有固定 5 点最大生命值，用于为玩家抵挡未[gold]格挡[/gold]的伤害。
/// 其「攻击牌后 3 点伤害」和「死亡塞若虫」等行为由玩家身上的
/// <see cref="Powers.TailorSwarmPower"/> 驱动，模型本身使用空移动
/// （与游戏原版 Osty 宠物一致，宠物是被动生物，不靠怪物 AI 走位）。
/// </summary>
public sealed class AglaeaTailorMonster : CustomMonsterModel
{
    // 衣匠基础 5 点生命值，可被「诛线」「金星行列」等牌永久提升。
    public override int MinInitialHp => 5;
    public override int MaxInitialHp => 5;

    public override bool IsHealthBarVisible => true;

    // 衣匠是宠物，不做任何主动移动（与 Osty 相同）。
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle = new MoveState(
            "NOTHING_MOVE",
            (IReadOnlyList<Creature> _) => Task.CompletedTask);
        idle.FollowUpState = idle;
        return new MonsterMoveStateMachine(
            new MonsterState[] { idle }, idle);
    }

    // 衣匠专属静态图立绘场景，已向 BaseLib 登记自动转换（见 AglaeaVisuals.Register）。
    protected override string VisualsPath => AglaeaVisuals.TailorScenePath;
}

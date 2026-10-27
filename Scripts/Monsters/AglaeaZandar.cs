using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MoeNegiMod.Aglaea.Cards;
using MoeNegiMod.Aglaea.Character;
using MoeNegiMod.Aglaea.Powers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Monsters;

/// <summary>
/// 第一位天才，隐德来希，赞达尔（v0.1.4 Boss）：1000 血，战斗开始时处于【演化形态】。
///
/// 形态循环：
/// · 演化形态：公理 → 天球 → 公理 → 虚数 循环；每当有生物生成（敌方目标生成或我方召唤物被召唤）
///   计数 -1，减到 0 后**立即**把行动意图切换为【SET 论证方法=黑潮】（不等下一次掷招）。
///   循环里两次「本该施放公理」的回合改为三岔：计数满 → SET；场上小怪 &lt; 3 → 公理（召唤 2 个）；
///   小怪已经够多（≥3）→【潜能实现猜想】（给其它敌人 +8 力量）。因此场上小怪最多 4 只。
/// · SET：杀死场上其他敌人，进入【论证形态】，获得 DEFRAG（补足 5 个敌方目标，含自己）与 CALL。
/// · 论证形态：DEL → FORMAT 循环；**每次行动都先随机召唤黑潮蚀刃/黑潮蚀斧把敌方补足到 5 个**；
///   每当有生物死亡计数 -1，减到 0 后**立即**切换为【求解：第一因之果】。CALL 每次行动 +1 层。
/// · 求解：杀死其他敌人、造成 39 伤害，按 CALL 层数获得 2×层数力量并向所有玩家抽牌堆塞等量【纠缠】，
///   回到演化形态。
///
/// 两个形态的计数都是带图标、可悬停查看的 power（演化形态 / 论证形态），
/// 用倒数表现「还差几次」；减到 0 时 power 消失，行动意图随即变化。
///
/// 数值为单人数值；进阶 8（ToughEnemies）提升生命，进阶 9（DeadlyEnemies）提升招式伤害
/// 并让每次行动多塞 1 张二律背反。多人平衡留待后续。
/// </summary>
public sealed class AglaeaZandar : CustomMonsterModel
{
    /// <summary>
    /// 演化形态计数阈值：5 + 玩家中 Aglaea 数量 × 2 + 玩家中亡灵契约师数量 × 1
    /// （2026-10-06 用户定的动态公式：多人下召唤物更多）。
    /// </summary>
    private int EvolutionThreshold
    {
        get
        {
            var players = CombatState?.Players;
            if (players == null)
            {
                return 5;
            }

            int aglaea = players.Count(p => p.Character.Id.Entry.RemovePrefix() == "AGLAEA");
            int necrobinder = players.Count(p => p.Character is Necrobinder);
            return 5 + aglaea * 2 + necrobinder;
        }
    }

    /// <summary>论证形态计数阈值：7 + 玩家数 × 1（2026-10-06 用户定的动态公式）。</summary>
    private int ArgumentThreshold => 7 + (CombatState?.Players.Count ?? 1);

    /// <summary>【秘所思公理】每次召唤的天谴斗士数量。</summary>
    private const int SummonPerCast = 2;

    /// <summary>
    /// 演化形态「该召唤的回合」改用【潜能实现猜想】的小怪数量阈值：
    /// 场上小怪 &lt; 3 才召唤，否则给全体小怪 +8 力量。
    /// </summary>
    private const int SummonMinionThreshold = 3;

    /// <summary>【潜能实现猜想】给其它敌人的力量。</summary>
    private const int PotentialStrength = 8;

    /// <summary>【DEFRAG 生命优化】要把场上敌方目标补足到的数量（含赞达尔自己）。</summary>
    private const int DefragTargetCount = 5;

    // 形态终结技的状态引用：计数器归零时要立刻把意图切到它们身上（见 ForceNextMove）。
    private MoveState? _setMove;

    private MoveState? _solveMove;

    // 进阶 9（DeadlyEnemies）：各招式伤害提升。
    private static int OrbitDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);   // 天球偏移论

    private static int GeometryDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);     // 虚数几何演绎法（单次）

    private const int GeometryHits = 3;

    private static int DelDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);   // DEL 入侵变量

    private static int FormatDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);     // FORMAT 故障空间（单次）

    private const int FormatHits = 3;

    private static int SolveDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 42, 39);   // 求解：第一因之果

    /// <summary>进阶 9：每次行动向所有玩家手牌放入的二律背反数量（1 → 2）。</summary>
    private static int AntinomyPerAction =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

    // 进阶 8（ToughEnemies）：生命值 1000 → 1024。
    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 1024, 1000);

    public override int MaxInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 1024, 1000);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();

        var context = new ThrowingPlayerChoiceContext();

        // 【化形公式】从战斗开始就挂在身上（初始 0 层，每次切换形态 +1）。
        await PowerCmd.Apply<FormFormulaPower>(context, Creature, 1, Creature, null);

        // 【演化形态】战斗开始时处于演化形态，计数器以 Buff 形式显示在赞达尔身上。
        await PowerCmd.Apply<EvolutionFormPower>(
            context, Creature, EvolutionThreshold, Creature, null);

        MainFile.Logger.Info(
            $"[Aglaea] 赞达尔入场：演化形态计数 {EvolutionThreshold}（还差 {EvolutionThreshold} 次生物生成）。");
    }

    /// <summary>
    /// 演化形态计数：每当有生物被加入战斗（敌方目标从场上生成、我方召唤物被召唤）时，
    /// 演化形态的倒数 -1；减到 0 时该 power 自动消失，行动状态机的分支随即改放 SET。
    /// </summary>
    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        var form = Creature.GetPower<EvolutionFormPower>();
        if (form == null)
        {
            return;
        }

        await PowerCmd.Decrement(form);
        // 诊断日志：确认生成计数真的在走（v0.1.4 实测期需要，稳定后可删）。
        MainFile.Logger.Info(
            $"[Aglaea] 赞达尔：演化形态计数-1（生成 {creature.LogName}），还差 {form.Amount} 次。");

        // 计数归零 → 立刻把意图切成 SET（不等下一次掷招）。
        if (Creature.GetPower<EvolutionFormPower>() == null)
        {
            MainFile.Logger.Info("[Aglaea] 赞达尔：演化形态计数已满，意图立即切换为 SET 论证方法=黑潮。");
            ForceNextMove(_setMove);
        }
    }

    /// <summary>
    /// 论证形态计数：每当有生物死亡（敌方目标被消灭、玩家无法战斗、召唤物死亡）时，
    /// 论证形态的倒数 -1；减到 0 时该 power 自动消失，行动状态机的分支随即改放求解。
    /// </summary>
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        // 赞达尔倒下 = 这场 Boss 战结束：停掉自定义 BGM（游戏中还有兜底补丁）。
        if (creature == Creature)
        {
            Audio.ZandarBossMusic.StopOurTrack();
        }

        if (wasRemovalPrevented)
        {
            return;
        }

        var form = Creature.GetPower<ArgumentFormPower>();
        if (form == null)
        {
            return;
        }

        await PowerCmd.Decrement(form);
        MainFile.Logger.Info(
            $"[Aglaea] 赞达尔：论证形态计数-1（死亡 {creature.LogName}），还差 {form.Amount} 次。");

        // 计数归零 → 立刻把意图切成求解（不等下一次掷招）。
        if (Creature.GetPower<ArgumentFormPower>() == null)
        {
            MainFile.Logger.Info("[Aglaea] 赞达尔：论证形态计数已满，意图立即切换为求解：第一因之果。");
            ForceNextMove(_solveMove);
        }
    }

    /// <summary>
    /// 把下一个行动立刻切换成指定招式并刷新意图图标。
    ///
    /// 配合 <see cref="MoveState.MustPerformOnceBeforeTransitioning"/>：被强制切过去的招式
    /// 在真正打出之前不会被下一次掷招顶掉（否则若强制发生在赞达尔自己的行动中，
    /// 下一回合的掷招会从它身上直接滚到后续招式，把 SET/求解跳过）。
    /// </summary>
    private void ForceNextMove(MoveState? state)
    {
        if (state == null || !Creature.IsAlive)
        {
            return;
        }

        SetMoveImmediate(state, forceTransition: true);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // ---- 演化形态 ----
        MoveState axiom1 = new MoveState(
            "AXIOM_MOVE", AxiomMove, new SummonIntent(), new StatusIntent(1));
        MoveState orbit = new MoveState(
            "ORBIT_MOVE", OrbitMove, new SingleAttackIntent(OrbitDamage), new StatusIntent(1));
        // 公理在循环里出现两次，第二次的后续不同（要接虚数而不是天球），所以需要两个状态节点。
        MoveState axiom2 = new MoveState(
            "AXIOM_MOVE_2", AxiomMove, new SummonIntent(), new StatusIntent(1));
        MoveState geometry = new MoveState(
            "GEOMETRY_MOVE", GeometryMove, new MultiAttackIntent(GeometryDamage, GeometryHits), new StatusIntent(1));
        // 【潜能实现猜想】：本该再召唤、但场上小怪已经够多（≥3）时改放这招，
        // 使除自己以外的所有敌人 +8 力量（强化+卡牌污染）。
        // 它在循环里替代两个不同的公理节点，后续也不同（虚数 / 天球），
        // 所以必须拆成两个状态节点——否则 MoveState 的 FollowUp 为空会直接抛异常。
        MoveState potentialAfterOrbit = new MoveState(
            "POTENTIAL_MOVE", PotentialMove, new BuffIntent(), new StatusIntent(1));
        MoveState potentialAfterGeometry = new MoveState(
            "POTENTIAL_MOVE_2", PotentialMove, new BuffIntent(), new StatusIntent(1));
        // SET / 求解是形态终结技：一旦被选中（或被强制切过来）必须先打出来，
        // 不能被下一次掷招顶掉（见 ForceNextMove）。
        MoveState set = new MoveState(
            "SET_MOVE", SetMove, new BuffIntent(), new StatusIntent(1))
        {
            MustPerformOnceBeforeTransitioning = true,
        };
        _setMove = set;

        // ---- 论证形态（每次行动都会先召唤补场，所以意图里带召唤图标）----
        MoveState del = new MoveState(
            "DEL_MOVE", DelMove, new SingleAttackIntent(DelDamage), new DebuffIntent(), new SummonIntent(), new StatusIntent(1));
        MoveState format = new MoveState(
            "FORMAT_MOVE", FormatMove, new MultiAttackIntent(FormatDamage, FormatHits), new DebuffIntent(), new SummonIntent(), new StatusIntent(1));
        MoveState solve = new MoveState(
            "SOLVE_MOVE", SolveMove, new SingleAttackIntent(SolveDamage), new BuffIntent(), new DebuffIntent(), new StatusIntent(1))
        {
            MustPerformOnceBeforeTransitioning = true,
        };
        _solveMove = solve;

        // 每次行动后检查对应形态的计数（计数器减到 0 时 power 消失），满则改放形态终结技。
        // 演化形态里「本该施放公理」的两处（循环的第 1、3 个动作）多一个分支：
        // 场上小怪 < 3 才召唤，否则改放【潜能实现猜想】——保证场上最多 4 只小怪
        // （结构参考原版组装师：能召唤才做召唤动作，否则走替代招式）。
        ConditionalBranchState afterAxiom1 = Branch("AFTER_AXIOM_1", set, orbit, EvolutionCompleted);
        ConditionalBranchState afterAxiom2 = Branch("AFTER_AXIOM_2", set, geometry, EvolutionCompleted);
        ConditionalBranchState afterOrbit = SummonOrPotential("AFTER_ORBIT", set, axiom2, potentialAfterOrbit);
        ConditionalBranchState afterGeometry = SummonOrPotential("AFTER_GEOMETRY", set, axiom1, potentialAfterGeometry);
        ConditionalBranchState afterDel = Branch("AFTER_DEL", solve, format, ArgumentCompleted);
        ConditionalBranchState afterFormat = Branch("AFTER_FORMAT", solve, del, ArgumentCompleted);

        axiom1.FollowUpState = afterAxiom1;
        orbit.FollowUpState = afterOrbit;
        axiom2.FollowUpState = afterAxiom2;
        geometry.FollowUpState = afterGeometry;
        potentialAfterOrbit.FollowUpState = geometry;      // 替代公理2 → 接虚数
        potentialAfterGeometry.FollowUpState = orbit;      // 替代公理1 → 接天球
        set.FollowUpState = del;        // SET 后进入论证形态
        del.FollowUpState = afterDel;
        format.FollowUpState = afterFormat;
        solve.FollowUpState = axiom1;   // 求解后回到演化形态

        List<MonsterState> states = new()
        {
            axiom1, orbit, axiom2, geometry, potentialAfterOrbit, potentialAfterGeometry, set,
            del, format, solve,
            afterAxiom1, afterOrbit, afterAxiom2, afterGeometry, afterDel, afterFormat,
        };
        return new MonsterMoveStateMachine(states, axiom1);
    }

    private static ConditionalBranchState Branch(
        string id, MoveState whenFull, MoveState otherwise, Func<bool> isFull)
    {
        ConditionalBranchState branch = new(id);
        // 先判「计数已满」，否则走循环里的下一个招式。
        branch.AddState(whenFull, isFull);
        branch.AddState(otherwise, () => true);
        return branch;
    }

    /// <summary>
    /// 「该召唤的那一回合」的三岔分支：计数已满 → 形态终结技；小怪不足 → 公理（召唤）；
    /// 小怪已够 → 潜能实现猜想。
    /// </summary>
    private ConditionalBranchState SummonOrPotential(
        string id, MoveState whenFull, MoveState summon, MoveState potential)
    {
        ConditionalBranchState branch = new(id);
        branch.AddState(whenFull, EvolutionCompleted);
        branch.AddState(summon, () => AliveMinionCount() < SummonMinionThreshold);
        branch.AddState(potential, () => true);
        return branch;
    }

    /// <summary>场上除赞达尔以外的存活敌人数量。</summary>
    private int AliveMinionCount()
    {
        return CombatState?.Enemies.Count(c => c != Creature && c.IsAlive) ?? 0;
    }

    /// <summary>演化形态计数是否已满 = 计数器 power 已经减到 0 消失。</summary>
    private bool EvolutionCompleted()
    {
        bool completed = Creature.GetPower<EvolutionFormPower>() == null;
        if (completed)
        {
            MainFile.Logger.Info("[Aglaea] 赞达尔：演化形态计数已满，下一行动意图变为 SET 论证方法=黑潮。");
        }

        return completed;
    }

    /// <summary>论证形态计数是否已满 = 计数器 power 已经减到 0 消失。</summary>
    private bool ArgumentCompleted()
    {
        bool completed = Creature.GetPower<ArgumentFormPower>() == null;
        if (completed)
        {
            MainFile.Logger.Info("[Aglaea] 赞达尔：论证形态计数已满，下一行动意图变为求解：第一因之果。");
        }

        return completed;
    }

    // ================= 演化形态的招式 =================

    /// <summary>【秘所思公理】召唤 2 个天谴斗士，并向所有玩家手中塞 1 张二律背反。</summary>
    private async Task AxiomMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Cast", 0.4f);

        for (int i = 0; i < SummonPerCast; i++)
        {
            if (await SummonMinion(ModelDb.Monster<AglaeaScourgeFighter>()) == null)
            {
                // 槽位已满（正常情况下打不到：演化形态最多 3 次公理 = 6 个爪牙，槽位备了 6 个）。
                MainFile.Logger.Warn("[Aglaea] 赞达尔：没有空槽位，秘所思公理少召唤了天谴斗士。");
                break;
            }
        }

        await PolluteHands();
    }

    /// <summary>
    /// 【潜能实现猜想】使除自己以外的所有敌人获得 8 点力量，并向所有玩家手中塞 1 张二律背反。
    /// （本该召唤、但场上小怪已经够多时的替代招式。）
    /// </summary>
    private async Task PotentialMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Cast", 0.4f);

        List<Creature> minions = CombatState?.Enemies
            .Where(c => c != Creature && c.IsAlive)
            .ToList() ?? new List<Creature>();

        if (minions.Count > 0)
        {
            await PowerCmd.Apply<StrengthPower>(
                new ThrowingPlayerChoiceContext(), minions, PotentialStrength, Creature, null);
        }

        await PolluteHands();
    }

    /// <summary>【天球偏移论】造成 15 点伤害。</summary>
    private async Task OrbitMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(OrbitDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.3f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
        await PolluteHands();
    }

    /// <summary>【虚数几何演绎法】造成 8 点伤害 3 次。</summary>
    private async Task GeometryMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(GeometryDamage)
            .WithHitCount(GeometryHits)
            .FromMonster(this)
            .OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.2f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
        await PolluteHands();
    }

    /// <summary>【SET 论证方法=黑潮】杀死其他敌方目标，进入论证形态并发放 DEFRAG / CALL。</summary>
    private async Task SetMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Cast", 0.6f);

        // 这些死亡发生在进入论证形态之前，不计入论证形态的死亡计数。
        await KillOtherEnemies();

        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<DefragPower>(context, Creature, 1, Creature, null);
        await PowerCmd.Apply<CallDestructionPower>(context, Creature, 1, Creature, null);

        // 【DEFRAG 生命优化】立即发动：补足 5 个敌方目标，新爪牙带【律法的惩戒】。
        await DefragReinforce();

        // 进入论证形态：挂上死亡计数器（这之后发生的死亡才开始倒数）。
        await PowerCmd.Apply<ArgumentFormPower>(
            context, Creature, ArgumentThreshold, Creature, null);
        // 立绘切成论证形态贴图。
        AglaeaVisuals.SetMonsterForm(Creature, "argument");
        MainFile.Logger.Info(
            $"[Aglaea] 赞达尔施放 SET：进入论证形态，死亡计数 {ArgumentThreshold}；DEFRAG 已补足敌方目标。");

        // 切换形态：化形公式 +1 层。
        await PowerCmd.Apply<FormFormulaPower>(context, Creature, 1, Creature, null);

        await PolluteHands();
    }

    // ================= 论证形态的招式 =================

    /// <summary>【DEL 入侵变量】先补场，再造成 10 点伤害并给予 1 层脆弱。</summary>
    private async Task DelMove(IReadOnlyList<Creature> targets)
    {
        // 论证形态：每次行动都先把敌方目标补足到 5 个（含赞达尔）。
        await DefragReinforce();

        await DamageCmd.Attack(DelDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.3f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
        // 2026-10-05 用户改：由易伤改为脆弱。
        await PowerCmd.Apply<FrailPower>(
            new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);

        await AfterArgumentAction();
    }

    /// <summary>【FORMAT 故障空间】先补场，再造成 4 点伤害 3 次并给予 1 层虚弱。</summary>
    private async Task FormatMove(IReadOnlyList<Creature> targets)
    {
        // 论证形态：每次行动都先把敌方目标补足到 5 个（含赞达尔）。
        await DefragReinforce();

        await DamageCmd.Attack(FormatDamage)
            .WithHitCount(FormatHits)
            .FromMonster(this)
            .OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.2f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(
            new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);

        await AfterArgumentAction();
    }

    /// <summary>
    /// 【求解：第一因之果】杀死其他敌方目标，造成 39 点伤害；
    /// 结算 CALL（每层 2 点力量 + 每层 1 张纠缠进所有玩家抽牌堆），回到演化形态。
    /// </summary>
    private async Task SolveMove(IReadOnlyList<Creature> targets)
    {
        // 先播全屏演出（盖住战场、不盖 UI），播完再结算。
        await AglaeaVfx.PlayFullscreen(Creature, AglaeaVfx.SolveFullscreenScenePath, allySide: false);

        // 形态终结时清场，随后才回到演化形态 —— 这些死亡不计入下一轮的任何计数
        // （论证计数器此刻已经减到 0 消失，演化计数器还没挂上）。
        await KillOtherEnemies();

        await DamageCmd.Attack(SolveDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.4f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);

        var context = new ThrowingPlayerChoiceContext();

        // ---- CALL 结算：每层 1 点力量 + 每人 1 层【纠缠】----
        var call = Creature.GetPower<CallDestructionPower>();
        int stacks = call?.Amount ?? 0;
        if (call != null)
        {
            await PowerCmd.Remove(call);
        }

        if (stacks > 0)
        {
            await PowerCmd.Apply<StrengthPower>(context, Creature, stacks * CallDestructionPower.StrengthPerStack, Creature, null);

            List<Creature> players = AlivePlayers();
            if (players.Count > 0)
            {
                await PowerCmd.Apply<EntanglePower>(context, players, stacks, Creature, null);
            }
        }

        await PowerCmd.Remove<DefragPower>(Creature);

        // ---- 回到演化形态：挂上新的生成计数器 ----
        await PowerCmd.Apply<EvolutionFormPower>(
            context, Creature, EvolutionThreshold, Creature, null);
        // 立绘切回演化形态贴图。
        AglaeaVisuals.SetMonsterForm(Creature, "");
        MainFile.Logger.Info(
            $"[Aglaea] 赞达尔施放求解：回到演化形态，生成计数重置为 {EvolutionThreshold}；CALL 结算 {stacks} 层。");

        // 切换形态：化形公式 +1 层。
        await PowerCmd.Apply<FormFormulaPower>(context, Creature, 1, Creature, null);

        await PolluteHands();
    }

    // ================= 共用逻辑 =================

    /// <summary>论证形态每次行动后：CALL 计数 +1，再向所有玩家手中塞 1 张二律背反。</summary>
    private async Task AfterArgumentAction()
    {
        var call = Creature.GetPower<CallDestructionPower>();
        if (call != null)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), call, 1, Creature, null);
        }

        await PolluteHands();
    }

    /// <summary>
    /// 【智识的二律背反】：赞达尔每次行动时，向所有玩家手中放入二律背反
    /// （普通 1 张；进阶 9 起 2 张）。
    /// </summary>
    private async Task PolluteHands()
    {
        List<Creature> players = AlivePlayers();
        if (players.Count == 0)
        {
            return;
        }

        // 塞牌逻辑与原版【实验体】塞灼烧一致。
        await CardPileCmd.AddToCombatAndPreview<AglaeaAntinomy>(
            players, PileType.Hand, AntinomyPerAction, null);
    }

    /// <summary>
    /// 【DEFRAG 生命优化】：场上敌方目标（含赞达尔）不足 5 个时，随机召唤黑潮蚀刃/黑潮蚀斧补足。
    /// 触发点：进入论证形态时（SET）+ 论证形态中每次行动（DEL/FORMAT）。
    /// </summary>
    private async Task DefragReinforce()
    {
        var combat = CombatState;
        if (combat == null)
        {
            return;
        }

        var options = new MonsterModel[]
        {
            ModelDb.Monster<AglaeaTideBlade>(),
            ModelDb.Monster<AglaeaTideAxe>(),
        };

        while (combat.Enemies.Count(c => c.IsAlive) < DefragTargetCount)
        {
            // 用怪物 AI 的同步随机流选种类（同原版 Fabricator），多人下两端一致。
            MonsterModel? choice = RunRng.MonsterAi.NextItem(options);
            Creature? summoned = await SummonMinion(choice);
            if (summoned == null)
            {
                MainFile.Logger.Warn("[Aglaea] 赞达尔：没有空槽位，DEFRAG 未能补足 5 个敌方目标。");
                break;
            }

            await PowerCmd.Apply<PunishmentOfLawPower>(
                new ThrowingPlayerChoiceContext(), summoned, 1, Creature, null);
        }
    }

    /// <summary>
    /// 在第一个空槽位召唤一个爪牙。槽位由遭遇战按顺序分配，保证赞达尔永远排在爪牙前面行动。
    /// 召唤成功的爪牙会获得原版【爪牙 MinionPower】标记（同原版 Fabricator）：
    /// 赞达尔倒下时，这些爪牙会一起退场。无空槽位时返回 null。
    /// </summary>
    private async Task<Creature?> SummonMinion(MonsterModel? monster)
    {
        var combat = CombatState;
        if (monster == null || combat == null || !combat.IsLiveCombat())
        {
            return null;
        }

        string slot = combat.Encounter?.GetNextSlot(combat) ?? string.Empty;
        if (string.IsNullOrEmpty(slot))
        {
            return null;
        }

        Creature summoned = await CreatureCmd.Add(monster.ToMutable(), combat, CombatSide.Enemy, slot);
        await PowerCmd.Apply<MinionPower>(
            new ThrowingPlayerChoiceContext(), summoned, 1, Creature, null);
        return summoned;
    }

    /// <summary>杀死场上除自己以外的所有敌人。</summary>
    private async Task KillOtherEnemies()
    {
        var combat = CombatState;
        if (combat == null)
        {
            return;
        }

        List<Creature> others = combat.Enemies
            .Where(c => c != Creature && c.IsAlive)
            .ToList();
        if (others.Count > 0)
        {
            await CreatureCmd.Kill(others);
        }
    }

    private List<Creature> AlivePlayers()
    {
        return CombatState?.Players
            .Select(p => p.Creature)
            .Where(c => c.IsAlive)
            .ToList() ?? new List<Creature>();
    }

    /// <summary>重复出现的招式节点（第二次公理 / 第二次潜能）图鉴里只显示一条。</summary>
    protected override bool ShouldShowMoveInBestiary(string moveStateId)
    {
        return moveStateId != "AXIOM_MOVE_2" && moveStateId != "POTENTIAL_MOVE_2";
    }

    protected override string VisualsPath => AglaeaVisuals.ZandarScenePath;
}

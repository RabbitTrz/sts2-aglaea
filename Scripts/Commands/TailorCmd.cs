using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Cards;
using MoeNegiMod.Aglaea.Character;
using MoeNegiMod.Aglaea.Extensions;
using MoeNegiMod.Aglaea.Monsters;
using MoeNegiMod.Aglaea.Powers;
using MoeNegiMod.Aglaea.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Commands;

/// <summary>
/// 衣匠召唤命令：召唤一名衣匠宠物，并确保玩家身上挂载衣匠集体 power。
/// </summary>
public static class TailorCmd
{
    public const int MaxTailors = 3;

    /// <summary>衣匠单次攻击的基础伤害（金丝等外部加成在此基础上叠加）。</summary>
    public const int TailorAttackDamage = 3;

    /// <summary>玩家当前可拥有的衣匠最大数量（基础上限 + 能力加成）。</summary>
    public static int GetMaxTailors(Player player)
    {
        int bonus = 0;
        var limit = player.Creature.GetPower<TailorLimitPower>();
        if (limit != null)
        {
            bonus = limit.Amount;
        }
        return MaxTailors + bonus;
    }

    /// <summary>当前存活的衣匠数量。</summary>
    public static int TailorCount(Player player)
    {
        int count = 0;
        foreach (var pet in player.PlayerCombatState.Pets)
        {
            if (pet.IsAlive && pet.Monster is AglaeaTailorMonster)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 召唤一名衣匠。
    /// 已达上限时（v0.1.3 机制改动）改为【把生命值最低的衣匠回满血】——
    /// 原先的行为是往抽牌堆放一张若虫，因为衣匠攻击现在会掉血，需要一条补血途径。
    /// </summary>
    public static async Task<Creature?> Summon(
        PlayerChoiceContext choiceContext, Player player, AbstractModel? source)
    {
        if (TailorCount(player) >= GetMaxTailors(player))
        {
            var lowest = AliveTailors(player).OrderBy(c => c.CurrentHp).FirstOrDefault();
            if (lowest != null && lowest.CurrentHp < lowest.MaxHp)
            {
                await CreatureCmd.Heal(lowest, lowest.MaxHp - lowest.CurrentHp);
            }
            return null;
        }

        Creature tailor = await PlayerCmd.AddPet<AglaeaTailorMonster>(player);

        // 应用衣匠额外最大生命（由诛线/金星行列等牌叠加）。
        var maxHpBonus = player.Creature.GetPower<TailorMaxHpBonusPower>();
        if (maxHpBonus != null && maxHpBonus.Amount > 0)
        {
            await CreatureCmd.GainMaxHp(tailor, maxHpBonus.Amount);
        }

        // 显示所有衣匠的血条：普通宠物在 AddCreature 中会被强制 ToggleIsInteractable(false)
        // 从而隐藏血条（只有内置 Osty 走特殊分支保留血条），且每次召唤新衣匠时，循环会
        // 把该玩家旗下【所有】宠物都重新置为 false。因此这里要把所有衣匠的血条都重新打开。
        foreach (var pet in player.PlayerCombatState.Pets)
        {
            if (pet.IsAlive && pet.Monster is AglaeaTailorMonster)
            {
                NCombatRoom.Instance?.SetCreatureIsInteractable(pet, true);
            }
        }

        // 确保玩家身上有衣匠集体 power（不存在则挂载）。
        if (player.Creature.GetPower<TailorSwarmPower>() == null)
        {
            await PowerCmd.Apply<TailorSwarmPower>(
                choiceContext, player.Creature, 1, player.Creature, source as CardModel);
        }

        // 即织即战：玩家身上有该能力时，被召唤的衣匠自动发动 N 次攻击（N = 层数）。
        var weaveAndFight = player.Creature.GetPower<WeaveAndFightPower>();
        if (weaveAndFight != null && weaveAndFight.Amount > 0)
        {
            for (int i = 0; i < weaveAndFight.Amount; i++)
            {
                // 「即织即战」的自动攻击属于额外攻击，不消耗衣匠生命。
                await AttackOnce(choiceContext, player, TailorAttackDamage, extraAttack: true);
            }
        }

        // 召唤后重排编队（本体左移、衣匠排到右侧，与亡灵契约师一致）。
        AglaeaFormation.Refresh(player);

        return tailor;
    }

    /// <summary>当前存活且属于本玩家的所有衣匠。</summary>
    public static List<Creature> AliveTailors(Player player)
    {
        return player.PlayerCombatState.Pets
            .Where(c => c.IsAlive && c.Monster is AglaeaTailorMonster)
            .ToList();
    }

    /// <summary>往该玩家抽牌堆的随机位置放若干张若虫。<paramref name="upgraded"/> 为 true 时放入升级后的「若虫+」。</summary>
    public static async Task AddChrysalisToDrawPile(
        PlayerChoiceContext choiceContext, Player player, AbstractModel? source,
        int count, CardPilePosition position = CardPilePosition.Random, bool upgraded = false)
    {
        if (count <= 0)
        {
            return;
        }

        // 确保玩家身上挂载若虫驱动 power（不存在则挂载），
        // 这样放入抽牌堆的若虫被抽到时才会自动打出。
        if (player.Creature.GetPower<ChrysalisSwarmPower>() == null)
        {
            await PowerCmd.Apply<ChrysalisSwarmPower>(
                choiceContext, player.Creature, 1, player.Creature, source as CardModel);
        }

        if (!upgraded)
        {
            await CardPileCmd.AddToCombatAndPreview<AglaeaChrysalis>(
                player.Creature, PileType.Draw, count, player, position);
            return;
        }

        // 升级版「若虫+」：先创建卡牌实例并升级，再放入抽牌堆。
        var combat = player.Creature.CombatState;
        for (int i = 0; i < count; i++)
        {
            var card = combat.CreateCard<AglaeaChrysalis>(player);
            CardCmd.Upgrade(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, player, position);
        }
    }

    // 「诛线」已改为无条件 +1 且不跨战斗继承，不再需要跨局斩杀记录，故整段注释掉。
    // 保留代码以便日后恢复「斩杀累计」机制。
    // /// <summary>
    // /// 记录一次「诛线」斩杀：确保玩家身上挂着诛线记录遗物，并把其层数 +1。
    // /// 层数跨战斗保留（随存档持久化），战斗开始时由遗物重新挂载衣匠最大生命加成。
    // /// </summary>
    // public static async Task RecordExecution(
    //     PlayerChoiceContext choiceContext, Player player, AbstractModel? source)
    // {
    //     var record = player.Relics.OfType<AglaeaExecutionRecord>().FirstOrDefault();
    //     if (record == null)
    //     {
    //         // index = -1：追加到遗物栏末尾。
    //         record = await RelicCmd.Obtain<AglaeaExecutionRecord>(player);
    //     }
    //
    //     record.Kills += 1;
    // }

    /// <summary>
    /// 优先选择带「间隙织线」的存活敌人，否则随机一名存活敌人；无敌人返回 null。
    /// </summary>
    public static Creature? PickTarget(Player player)
    {
        var combat = player.Creature.CombatState;
        if (combat == null)
        {
            return null;
        }

        var enemies = combat.Enemies.Where(e => e.IsAlive).ToList();
        if (enemies.Count == 0)
        {
            return null;
        }

        var marked = enemies.FirstOrDefault(e => e.GetPower<WeaveLinePower>() != null);

        // 必须用 run 级同步随机数流（CombatTargets = 原版随机选目标用的那条）。
        // 多人是确定性锁步：本地随机数（Random.Shared）会让每个客户端选到不同敌人，
        // 直接导致状态分歧、游戏报错闪退。单机测不出来，只有多人会暴露。
        return marked ?? player.RunState.Rng.CombatTargets.NextItem(enemies);
    }

    /// <summary>
    /// 计算衣匠本次攻击的最终伤害（开放入口，供金丝与能力牌等外部加成扩展）。
    /// 基础伤害 + 金丝每层 +1 + 锐丝如刃加成。
    /// </summary>
    public static int GetTailorDamage(Player player, int baseDamage)
    {
        int damage = baseDamage;

        var goldenThread = player.Creature.GetPower<GoldenThreadPower>();
        if (goldenThread != null)
        {
            damage += goldenThread.Amount;
        }

        var edgeThread = player.Creature.GetPower<EdgeThreadPower>();
        if (edgeThread != null)
        {
            damage += edgeThread.Amount;
        }

        return damage;
    }

    /// <summary>
    /// 随机一名衣匠攻击一次（伤害经金丝等加成，优先带间隙织线的敌人）。
    /// <paramref name="extraAttack"/> = true 表示这是「额外攻击」，不消耗衣匠生命。
    /// </summary>
    public static async Task AttackOnce(
        PlayerChoiceContext choiceContext, Player player, int damage, bool extraAttack = false)
    {
        var tailors = AliveTailors(player);
        if (tailors.Count == 0)
        {
            return;
        }

        // 同上：选哪名衣匠也必须所有客户端一致。
        var tailor = player.RunState.Rng.CombatTargets.NextItem(tailors);
        if (tailor == null)
        {
            return;
        }

        await AttackWith(choiceContext, player, tailor, damage, extraAttack);
    }

    /// <summary>所有衣匠各攻击一次（伤害经金丝等加成，优先带间隙织线的敌人）。</summary>
    public static async Task AttackAll(
        PlayerChoiceContext choiceContext, Player player, int damage, bool extraAttack = false)
    {
        var tailors = AliveTailors(player);
        foreach (var tailor in tailors)
        {
            await AttackWith(choiceContext, player, tailor, damage, extraAttack);
        }
    }

    /// <summary>
    /// 衣匠攻击的代价：攻击后失去 1 点生命（v0.1.3 机制）。
    /// 「额外攻击」不走这里。
    ///
    /// 扣到 0 会真的死：<see cref="Creature.LoseHpInternal"/> 只改血量、不处理死亡，
    /// 必须再走 <see cref="CreatureCmd.Kill"/> 才会触发 AfterDeath 等钩子
    /// （密针的减费、锻泪匠躯的死亡 AOE 都靠它），也才会把衣匠移出战斗。
    /// </summary>
    private static async Task PayAttackCost(PlayerChoiceContext choiceContext, Player player, Creature tailor)
    {
        if (!tailor.IsAlive)
        {
            return;
        }

        tailor.LoseHpInternal(1, ValueProp.Unblockable | ValueProp.Unpowered);

        if (!tailor.IsAlive)
        {
            await CleanupDeadTailors(choiceContext, player);
        }
    }

    /// <summary>
    /// 把已经死掉的衣匠真正移除，并按个数往抽牌堆放若虫。
    ///
    /// 原先这段只写在 <c>TailorSwarmPower.AfterDamageReceived</c>（衣匠代伤致死那条路），
    /// v0.1.3 之后衣匠自己攻击也会掉血致死，所以提出来两处公用。
    /// </summary>
    public static async Task CleanupDeadTailors(
        PlayerChoiceContext choiceContext, Player player, AbstractModel? source = null)
    {
        var dead = player.PlayerCombatState.Pets
            .Where(c => !c.IsAlive && c.Monster is AglaeaTailorMonster)
            .ToList();
        if (dead.Count == 0)
        {
            return;
        }

        await CreatureCmd.Kill(dead);
        await AddChrysalisToDrawPile(choiceContext, player, source, dead.Count);

        // 有衣匠阵亡：重排剩余衣匠的站位（本体不回弹，与亡灵契约师一致）。
        AglaeaFormation.Refresh(player);
    }

    /// <summary>
    /// 指定衣匠攻击一次（伤害经金丝等加成，优先带间隙织线的敌人）。
    /// 供「半神形态」等需要在衣匠攻击后驱动同一衣匠连续攻击的能力使用。
    /// </summary>
    public static async Task AttackWith(
        PlayerChoiceContext choiceContext, Player player, Creature tailor, int damage,
        bool extraAttack = false)
    {
        var target = PickTarget(player);
        if (target == null)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(tailor, "Attack", 0f);

        // 广织成网：衣匠攻击对所有敌人造成伤害。
        var wideNet = player.Creature.GetPower<WideNetPower>() != null;
        var targets = wideNet
            ? player.Creature.CombatState.Enemies.Where(e => e.IsAlive).ToList()
            : new List<Creature> { target };

        int finalDamage = GetTailorDamage(player, damage);

        // 致命浪漫：衣匠对处于「临时力量**下降**」效果的敌人造成额外伤害。
        // 1 层 = 2 倍，2 层 = 3 倍（倍率 = 1 + 层数），叠加写法对齐原版猎手的「追踪」。
        // 注意必须排除临时力量【上升】（HasTemporaryStrengthDown 内部按 Debuff 判定），
        // 否则敌人身上挂着 FlexPotion 的 +2 力量时也会被误判为满足条件。
        var fatalRomance = player.Creature.GetPower<FatalRomancePower>();

        foreach (var t in targets)
        {
            // 通知玩家身上的「衣匠攻击前」监听者（用于锻泪匠躯等）。
            foreach (var listener in player.Creature.Powers.OfType<ITailorAttackListener>())
            {
                await listener.BeforeTailorAttack(choiceContext, player, tailor, t);
            }

            int damageToTarget = finalDamage;
            if (fatalRomance != null && t.HasTemporaryStrengthDown())
            {
                damageToTarget = (int)(damageToTarget * FatalRomancePower.GetMultiplier(fatalRomance.Amount));
            }

            // 衣匠攻击：不受角色力量/虚弱影响，但受敌人易伤影响。
            // Move 保留"攻击"语义（吃易伤等），Unpowered 使其不吃力量/虚弱。
            await CreatureCmd.Damage(
                choiceContext, t, damageToTarget, ValueProp.Move | ValueProp.Unpowered, tailor);

            // 通知玩家身上的「衣匠攻击后」监听者（用于沐浴黄金、回纺为守等）。
            // damage 参数 = 本次攻击【实际结算】的伤害，已含致命浪漫的加成。
            // 注：当前三个实现（沐浴黄金/回纺为守/半神形态）都不读此参数，
            // 各自读自己需要的 power；新增监听者若要用它，按"实际伤害"理解。
            foreach (var listener in player.Creature.Powers.OfType<ITailorAttackListener>())
            {
                await listener.AfterTailorAttack(choiceContext, player, tailor, t, damageToTarget);
            }
        }

        // v0.1.3：每次攻击后衣匠失去 1 点生命。「额外攻击」不消耗。
        // 放在 target 循环之外——「广织成网」一次攻击打所有敌人，也只算一次攻击。
        if (!extraAttack)
        {
            await PayAttackCost(choiceContext, player, tailor);
        }
    }
}

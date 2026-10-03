// ==================== 已停用：诛线记录遗物 ====================
// 原因：诛线改为「无条件 +1、不跨战斗继承」后，本遗物失去唯一调用方（TailorCmd.RecordExecution 已注释）。
// 按项目惯例整文件注释保留，以便日后恢复「斩杀累计 / 跨战斗成长」机制时直接还原。
// 恢复时需同时还原：TailorCmd.RecordExecution，以及 AglaeaExecuteThread 中的调用。
// ================================================================
// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Relics;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.Localization.DynamicVars;
// using MegaCrit.Sts2.Core.Saves.Runs;
// using MoeNegiMod.Aglaea.Commands;
// using MoeNegiMod.Aglaea.Powers;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// 
// namespace MoeNegiMod.Aglaea.Relics;
// 
// /// <summary>
// /// 诛线记录（阿格莱雅专属的隐藏计数遗物）：记录本局内「诛线」斩杀敌人的次数。
// ///
// /// 该遗物不参与任何掉落池，只在诛线首次斩杀时由卡牌创建，并附加到遗物栏末尾。
// /// 层数（<see cref="Kills"/>）通过 [SavedProperty] 随存档持久化，战斗结束时不会被清理，
// /// 因此同一局中跨战斗保留；一整局结束后随 run 一起清空。
// ///
// /// 战斗开始时按层数给玩家挂载 <see cref="TailorMaxHpBonusPower"/>，
// /// 使之后召唤的衣匠获得等量的最大生命加成。
// /// </summary>
// public class AglaeaExecutionRecord : AglaeaRelics
// {
//     private const string _killsKey = "Kills";
// 
//     private int _kills;
// 
//     public override RelicRarity Rarity => RelicRarity.None;
// 
//     // 不从商店/掉落获得。
//     public override bool IsAllowedInShops => false;
// 
//     // 层数自己管理，不使用游戏内置的 StackCount。
//     public override bool IsStackable => false;
// 
//     // 在遗物栏显示累计层数。
//     public override bool ShowCounter => true;
// 
//     public override int DisplayAmount => Kills;
// 
//     protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(_killsKey, _kills)];
// 
//     /// <summary>本局内「诛线」斩杀敌人的累计次数。</summary>
//     [SavedProperty]
//     public int Kills
//     {
//         get => _kills;
//         set
//         {
//             AssertMutable();
//             _kills = value;
//             // 用索引器访问：DynamicVar 没有强类型 Kills getter，直接访问会抛 InvalidCastException。
//             DynamicVars[_killsKey].BaseValue = _kills;
//             InvokeDisplayAmountChanged();
//         }
//     }
// 
//     /// <summary>
//     /// 战斗开始时，把累计的衣匠最大生命加成重新施加。
//     /// 起始遗物「黄金手链」(<see cref="AglaeaToy"/>) 在本遗物之前执行（它在 player.Relics 中排在前，
//     /// 本遗物由 Obtain 追加到末尾），因此此时场上已有衣匠——需要直接给它们加最大生命，
//     /// 而不能只挂 <see cref="TailorMaxHpBonusPower"/>（该 power 仅在召唤新衣匠时才会被读取）。
//     /// </summary>
//     public override async Task BeforeCombatStart()
//     {
//         if (Kills <= 0)
//         {
//             return;
//         }
// 
//         // 对已在场的衣匠直接增加最大生命。
//         foreach (var tailor in TailorCmd.AliveTailors(Owner))
//         {
//             await CreatureCmd.GainMaxHp(tailor, Kills);
//         }
// 
//         // 挂载 power，使本场后续召唤的衣匠也获得加成。
//         await PowerCmd.Apply<TailorMaxHpBonusPower>(
//             new BlockingPlayerChoiceContext(), Owner.Creature, Kills, Owner.Creature, null);
//     }
// }

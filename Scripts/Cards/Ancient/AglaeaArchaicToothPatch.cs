using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;
using System.Linq;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 让「古老牙齿」把阿格莱雅的起始牌【裁剪】变化为【执行者】。
///
/// <see cref="ArchaicTooth"/> 内部有一张私有的 starter→ancient 静态映射表（只含原版 5 角色的初始牌），
/// 挑选可转化起始牌时用 <c>TranscendenceUpgrades.ContainsKey(c.Id)</c> 判断，mod 的牌进不去。
///
/// ==================== 实机观察得到的真实时序（别再猜） ====================
/// 在欧洛巴斯房间实测，对阿格莱雅（牌组里有裁剪）依次发生：
///   GetTranscendenceStarterCard(player)   → 原版返回 null（裁剪不在表里）
///   SetupForPlayer 里 if (c != null) 判断为假 → 直接 return false → 牙齿不出现在选项里
///
/// 原版回退分支存在缺陷：裁剪进不去映射表时会走
///   return base.Owner.RunState.CreateCard&lt;Doubt&gt;(starterCard.Owner);
/// 而此处遗物刚 ToMutable()、Owner 还没挂上（Owner 是在 EventModel.RelicOption 里才赋的），
/// base.Owner 为 null → NullReferenceException → GenerateInitialOptions 中断 → 事件房间卡死。
/// 这条分支原版五角色永远走不到，所以从未暴露。
///
/// 实测还确认（用于避免重犯）：starterCard 是牌组里那张运行时实例，
/// IsMutable=True、Owner 有值、RunState 有值，只有 CombatState=null。
/// ==================== 实现思路 ====================
/// 让原版彻底不碰回退分支：
///   prefix  prefix 交出裁剪，并把执行者一并建好、登记；
///   postfix 只做「查表替换」，绝不读 base.Owner / starterCard.Owner。
/// 建牌用 player.RunState（prefix 拿得到 player，这是唯一可靠的来源）。
/// </summary>
[HarmonyPatch(typeof(ArchaicTooth))]
internal static class AglaeaArchaicToothPatch
{
    /// <summary>
    /// 前缀里建好的 裁剪→执行者 映射，供 postfix 取用。
    /// 用引用相等比较：<see cref="AbstractModel"/> 重写了 Equals，默认比较器会被 Id 等值判定干扰。
    /// </summary>
    private static readonly Dictionary<CardModel, CardModel> PendingTransform =
        new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// 原版只认内置映射表；这里在牌组中优先找出【裁剪】并作为转化源。
    /// 同时把转化结果预先建好——因为 postfix 拿不到 player，建不了牌。
    /// </summary>
    [HarmonyPatch("GetTranscendenceStarterCard")]
    [HarmonyPrefix]
    private static bool GetStarterCardPrefix(Player player, ref CardModel? __result)
    {
        var trim = player.Deck.Cards.FirstOrDefault(c => c is AglaeaTrim);

        if (trim == null)
        {
            // 牌组里没有裁剪，交回原版逻辑（原版角色不受影响）。
            return true;
        }

        // 先把执行者建出来。player.RunState 是此时唯一可靠的建牌入口。
        var executor = player.RunState.CreateCard(ModelDb.Card<AglaeaExecutor>(), player);

        // 沿用原版行为：起始牌已升级则转化结果也升级。
        if (trim.IsUpgraded)
        {
            MegaCrit.Sts2.Core.Commands.CardCmd.Upgrade(executor);
        }

        // 沿用原版行为：起始牌的附魔一并继承到转化结果上。
        if (trim.Enchantment != null)
        {
            var enchantment = (EnchantmentModel)trim.Enchantment.MutableClone();
            MegaCrit.Sts2.Core.Commands.CardCmd.Enchant(enchantment, executor, enchantment.Amount);
        }

        PendingTransform[trim] = executor;

        __result = trim;
        return false;
    }

    /// <summary>
    /// 裁剪不在内置映射表内，原版会走
    /// <c>base.Owner.RunState.CreateCard&lt;Doubt&gt;(...)</c> —— 而此刻 base.Owner 为 null，必崩。
    /// 这里用前缀建好的执行者直接替换结果，让原版那行永远不会被执行到。
    ///
    /// 注意：不能调用原版（会崩），必须 prefix 拦下；替换用的执行者已在前缀建好。
    /// </summary>
    [HarmonyPatch("GetTranscendenceTransformedCard")]
    [HarmonyPrefix]
    private static bool TransformedPrefix(CardModel starterCard, ref CardModel __result)
    {
        if (!PendingTransform.TryGetValue(starterCard, out var executor))
        {
            // 不是我们登记的裁剪 → 交回原版（原版角色走这里，行为不变）。
            return true;
        }

        PendingTransform.Remove(starterCard);
        __result = executor;
        return false;
    }
}

/// <summary>
/// 把【执行者】登记进「先古转化牌」名单。
///
/// 原版 <see cref="ArchaicTooth.TranscendenceCards"/> 只返回内置五角色的转化结果（Break / Suppress / …），
/// 而 <see cref="DustyTome"/>（尘封魔典）挑牌时的过滤条件是
/// <c>Rarity == Ancient &amp;&amp; !TranscendenceCards.Contains(c)</c> —— 意图是「别把转化奖励再发一遍」。
/// 执行者对阿格莱雅正是转化奖励，但它是 mod 加的先古牌、进不了那张内置表，
/// 于是被魔典当普通先古牌抽走了（多人实测：达弗的魔典发了执行者而不是金血浇铸）。
///
/// <c>TranscendenceCards</c> 全库只有 DustyTome 一个消费者，所以在这里补一条即可，
/// 语义上也最贴切：执行者本来就应该算转化牌。
/// </summary>
[HarmonyPatch(typeof(ArchaicTooth), nameof(ArchaicTooth.TranscendenceCards), MethodType.Getter)]
internal static class AglaeaTranscendenceCardsPatch
{
    private static void Postfix(ref List<CardModel> __result)
    {
        __result.Add(ModelDb.Card<AglaeaExecutor>());
    }
}

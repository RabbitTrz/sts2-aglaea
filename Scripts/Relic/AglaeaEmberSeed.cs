using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 火种（阿格莱雅专属的隐藏计数遗物）：由卡牌「逐火之路」在战斗结束后发放。
///
/// 可叠加：每获得 1 个火种层数 +1；累计达到 12 层时清零，并随机发放 1 个阿格莱雅专属遗物
/// （反重力皮靴 / 火力风暴潮 / 光速螺旋桨）。
///
/// 该遗物不参与任何掉落池（RelicRarity.None + IsAllowedInShops=false），
/// 层数（<see cref="Embers"/>）通过 [SavedProperty] 随存档持久化，跨战斗保留。
/// 这个「遗物自己存进度」的写法与原版 <c>PaelsWing</c> 同构
/// （<c>[SavedProperty] int RewardsSacrificed</c> + <c>ShowCounter => true</c>）。
///
/// 容易踩的坑：STS2 的遗物模型**没有**「同一遗物只留一个实例」的机制
/// （去重只在 <c>PowerCmd</c> 里有），而 <c>RelicReward</c> 每次都会新建实例。
/// 所以「叠加」这件事必须由 <see cref="AfterObtained"/> 自己实现，见其注释。
/// </summary>
public class AglaeaEmberSeed : AglaeaRelics
{
    private const int _threshold = 12;

    private const string _embersKey = "Embers";

    private int _embers;

    public override RelicRarity Rarity => RelicRarity.None;

    // 不从商店/掉落获得。
    public override bool IsAllowedInShops => false;

    // 层数自己管理，不使用游戏内置的 StackCount。
    public override bool IsStackable => false;

    // 在遗物栏显示累计层数。
    public override bool ShowCounter => true;

    public override int DisplayAmount => Embers;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(_embersKey, _embers)];

    /// <summary>本局内累计的火种层数。</summary>
    [SavedProperty]
    public int Embers
    {
        get => _embers;
        set
        {
            AssertMutable();
            _embers = value;
            // 用索引器访问：DynamicVar 没有强类型 Embers getter，直接访问会抛 InvalidCastException。
            DynamicVars[_embersKey].BaseValue = _embers;
            InvokeDisplayAmountChanged();
        }
    }

    /// <summary>
    /// 获得 <paramref name="count"/> 个火种：加上层数，并结算「累计满 12 层」的发放。
    /// 由 <see cref="AfterObtained"/> 在每次获得火种时调用（首次与叠加都走这里）。
    /// </summary>
    public async Task GainEmbers(PlayerChoiceContext choiceContext, int count)
    {
        if (count <= 0)
        {
            return;
        }

        Embers += count;

        while (Embers >= _threshold)
        {
            Embers -= _threshold;
            await GrantRandomExclusiveRelic(choiceContext);
        }
    }

    /// <summary>
    /// 「已有火种 → 层数 +1；没有火种 → 获得火种（层数 = 1）」。
    ///
    /// 为什么必须在这里手写：<see cref="RelicCmd.Obtain"/> **没有**去重逻辑
    /// （直接 <c>AddRelicInternal</c> + 无条件调 <c>AfterObtained</c>）。
    /// 去重只存在于 <c>PowerCmd.Apply</c>（<c>FindExistingInstanceForStacking</c>），
    /// 遗物侧没有对应物 —— 所以我上面那段「Obtain 已替我们做好」的旧注释是错的。
    ///
    /// 现场：<c>EmberPathPower</c> 每次战斗结束都新建一个
    /// <c>ModelDb.Relic&lt;AglaeaEmberSeed&gt;().ToMutable()</c>，走 <c>RelicReward</c>。
    /// 点击格子时若已有火种，列表里就会多出第二个实例，而**新实例的 Embers 是 0**
    /// （<c>ToMutable()</c> 走 <c>MemberwiseClone</c>，规范单例的 0 被原样带过来）。
    /// 于是两种分支都得自己盖：
    ///   · <c>existing == null</c>（首次）→ 补 1，否则首领是 0 层；
    ///   · <c>existing != null</c>（叠加）→ 旧层数 +1。**不能写成 <c>existing.Embers</c>**，
    ///     那是「把旧层数搬过来」，会把这次该加的 1 层吞掉（表现为完全不叠层）。
    ///
    /// 【为什么删的是旧实例而不是新实例】——不能删 <c>this</c>。
    /// <c>NRelicInventory.AnimateRelic</c> 内部是
    /// <c>_relicNodes.First(n => n.Relic.Model == relic)</c>（**First，不是 FirstOrDefault**），
    /// 且它跑在 <c>AfterObtained</c> **之后**。若在这里删掉 <c>this</c>，
    /// <c>RelicRemoved</c> 会先把节点移出 <c>_relicNodes</c>，<c>First</c> 随即抛
    /// <c>InvalidOperationException</c>，<c>GetReward</c> 的续行（<c>EmitSignal(RewardClaimed)</c>）
    /// 不再执行 —— 表现就是「格子不消失，但已 Disable 无法再点」。
    /// 删旧实例则不影响 <c>ClaimedRelic</c>（= this），动画与信号流程均正常。
    /// </summary>
    public override async Task AfterObtained()
    {
        var choiceContext = new BlockingPlayerChoiceContext();
        var existing = Owner.Relics.OfType<AglaeaEmberSeed>().FirstOrDefault(r => r != this);

        if (existing == null)
        {
            // 首次获得。新实例的 Embers 默认是 0，不补这 1 就是 0 层。
            await GainEmbers(choiceContext, 1);
            return;
        }

        // 叠加：把【旧层数 + 本次 1 层】接到自己身上，再移除旧实例，
        // 保证遗物栏里始终只有一个火种。注意是 existing.Embers + 1，不是 existing.Embers。
        await GainEmbers(choiceContext, existing.Embers + 1);
        await RelicCmd.Remove(existing);
    }

    /// <summary>随机发放 1 个阿格莱雅专属遗物。</summary>
    private async Task GrantRandomExclusiveRelic(PlayerChoiceContext choiceContext)
    {
        var pool = AglaeaExclusiveRelicPool.GetUnobtained(Owner);
        if (pool.Count == 0)
        {
            return;
        }

        // 用 run 级同步随机数流。Niche 是原版留给「不影响其他系统的杂项随机」的那条，
        // 用它发遗物不会扰动原版的奖励/掉落流，同时保证所有客户端发到同一个遗物。
        // 本地随机数（Random.Shared）在多人下会让各人拿到不同遗物，直接状态分歧闪退。
        var relic = Owner.RunState.Rng.Niche.NextItem(pool);
        if (relic == null)
        {
            return;
        }
        await RelicCmd.Obtain(relic, Owner);
    }
}

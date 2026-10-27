using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MoeNegiMod.Aglaea.Acts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Events;

/// <summary>
/// 第四章事件「往昔的涟漪」（替换原火堆房间）。
///
/// 结构：一选（接受头环，回复全部生命）→ 二选（三种故事，三种奖励）→ 结束文案。
/// 文本在 <c>localization/zhs/events.json</c>，键前缀为 <c>MOENEGIMOD-AGLAEA_RIPPLE_EVENT</c>。
/// </summary>
public sealed class AglaeaRippleEvent : CustomEventModel
{
    /// <summary>
    /// 不通过 BaseLib 的事件字典注册。
    ///
    /// BaseLib 的 AddEvent 按 <c>Acts.Length == 0</c> 把这个事件归类为「共享事件」，
    /// 会进入所有章节的事件池（实测第一幕第一个问号就出了）。
    /// 而 <c>Acts</c> 里又不能调用 <c>ModelDb.Act&lt;AglaeaZandarAct&gt;()</c>：
    /// 事件在 ModelDb 初始化期间就会被构造，此时章节还没注册，会抛 KeyNotFoundException。
    ///
    /// 所以干脆 autoAdd: false，章节归属完全交给 <c>Act4FixedRippleEventPatch</c> 固定抽取。
    /// 模型本身仍会被 ModelDb 正常注册，<c>ModelDb.Event&lt;AglaeaRippleEvent&gt;()</c> 可用。
    /// </summary>
    public AglaeaRippleEvent() : base(autoAdd: false)
    {
    }

    public override ActModel[] Acts => System.Array.Empty<ActModel>();

    /// <summary>事件全屏插画（默认布局；3440×1616，视觉重心在左，右侧留给文字）。</summary>
    public override string? CustomInitialPortraitPath =>
        "res://Aglaea/images/events/aglaea_ripple_event.png";

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new[]
        {
            new EventOption(this, AcceptGift, $"{Id.Entry}.pages.INITIAL.options.ACCEPT"),
        };
    }

    /// <summary>一选：收下麦穗头环，回复全部生命值。</summary>
    private async Task AcceptGift()
    {
        var player = Owner!;
        await CreatureCmd.Heal(player.Creature, player.Creature.MaxHp - player.Creature.CurrentHp);

        SetEventState(
            L10NLookup($"{Id.Entry}.pages.STORY.description"),
            new[]
            {
                new EventOption(this, AdventureStory, $"{Id.Entry}.pages.STORY.options.ADVENTURE"),
                new EventOption(this, FableStory, $"{Id.Entry}.pages.STORY.options.FABLE"),
                new EventOption(this, ReunionStory, $"{Id.Entry}.pages.STORY.options.REUNION"),
            });
    }

    private Task AdventureStory() => OfferRareCards();

    private Task FableStory() => OfferRareRelic();

    private Task ReunionStory() => GainMaxHp();

    /// <summary>二选 A：2 次稀有卡牌奖励（每次 3 张，全部为稀有）。</summary>
    private async Task OfferRareCards()
    {
        var player = Owner!;
        var options = new CardCreationOptions(
                new[] { player.Character.CardPool },
                CardCreationSource.Other,
                CardRarityOddsType.Uniform,
                c => c.Rarity == CardRarity.Rare)
            .WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications);

        await RewardsCmd.OfferCustom(player, new List<Reward>
        {
            new CardReward(options, 3, player),
            new CardReward(options, 3, player),
        });

        SetEventFinished(L10NLookup($"{Id.Entry}.pages.END.description"));
    }

    /// <summary>二选 B：1 个稀有遗物奖励。</summary>
    private async Task OfferRareRelic()
    {
        var player = Owner!;
        await RewardsCmd.OfferCustom(player, new List<Reward>
        {
            new RelicReward(RelicRarity.Rare, player),
        });

        SetEventFinished(L10NLookup($"{Id.Entry}.pages.END.description"));
    }

    /// <summary>二选 C：获得 24 点最大生命值。</summary>
    private async Task GainMaxHp()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, 24m);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.END.description"));
    }
}

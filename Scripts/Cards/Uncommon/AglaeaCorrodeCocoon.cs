using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 蚀茧：消耗抽牌堆中的所有若虫。每消耗 1 张召唤 1 次衣匠。消耗。升级后去除消耗。
/// （v0.1.3 改动：攻击牌 → 技能牌；效果由「每张造成伤害」改为「每张召唤衣匠」。）
/// </summary>
public class AglaeaCorrodeCocoon() : AglaeaCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<AglaeaChrysalis>(),
        TipTailor
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Player;

        // 消耗抽牌堆中的所有若虫；每消耗一张召唤一次衣匠。
        var chrysalises = CardPile.GetCards(player, PileType.Draw)
            .Where(c => c is AglaeaChrysalis)
            .ToList();

        foreach (var chrysalis in chrysalises)
        {
            await CardCmd.Exhaust(choiceContext, chrysalis);
            await TailorCmd.Summon(choiceContext, player, this);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

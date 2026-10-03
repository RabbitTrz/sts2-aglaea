using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 政务处理（多人 · 罕见 · 技能）：在其他玩家的抽牌堆中放入 2 张若虫。消耗。
/// 升级：去除消耗。
/// </summary>
public class AglaeaPaperwork() : AglaeaCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AllAllies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        TipChrysalis
    ];

    private const int _chrysalisCount = 2;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只影响「其他」玩家，不含自己。
        // 写法对齐原版 Rally.cs（同为 TargetType.AllAllies + MultiplayerOnly）。
        var others = cardPlay.Player.Creature.CombatState
            .GetTeammatesOf(cardPlay.Player.Creature)
            .Where(c => c.IsAlive && c.Player != null && c.Player != cardPlay.Player)
            .ToList();

        foreach (var teammate in others)
        {
            await TailorCmd.AddChrysalisToDrawPile(
                choiceContext, teammate.Player!, this, _chrysalisCount);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

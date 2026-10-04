using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 列阵：召唤衣匠直至达到最大数量。消耗。升级：移除消耗。
/// </summary>
public class AglaeaArray() : AglaeaCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int missing = TailorCmd.GetMaxTailors(cardPlay.Player) - TailorCmd.TailorCount(cardPlay.Player);
        for (int i = 0; i < missing; i++)
        {
            await TailorCmd.Summon(choiceContext, cardPlay.Player, this);
        }

        // v0.1.3：再回满所有衣匠的生命值。
        foreach (var tailor in TailorCmd.AliveTailors(cardPlay.Player))
        {
            if (tailor.CurrentHp < tailor.MaxHp)
            {
                await CreatureCmd.Heal(tailor, tailor.MaxHp - tailor.CurrentHp);
            }
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

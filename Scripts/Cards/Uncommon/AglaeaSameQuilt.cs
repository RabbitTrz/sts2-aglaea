using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.GameInfo.Objects;
using MoeNegiMod.Aglaea.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 同衾：将手牌中所有非攻击牌变化为连携打击。升级后保留。
/// </summary>
public class AglaeaSameQuilt() : AglaeaCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<AglaeaLinkedStrike>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Player;

        // 快照当前手牌中所有非攻击牌。
        var nonAttacks = CardPile.GetCards(player, PileType.Hand)
            .Where(c => c.Type != CardType.Attack)
            .ToList();

        foreach (var card in nonAttacks)
        {
            await CardCmd.TransformTo<AglaeaLinkedStrike>(card);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}

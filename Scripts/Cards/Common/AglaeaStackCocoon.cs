using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 叠茧：获得 5 点[gold]格挡[/gold]。将抽牌堆中的所有若虫置于抽牌堆顶。升级后 8 点[gold]格挡[/gold]。
/// </summary>
public class AglaeaStackCocoon() : AglaeaCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<AglaeaChrysalis>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);

        // 将抽牌堆中的所有若虫置于堆顶。
        var drawPile = PileType.Draw.GetPile(cardPlay.Player);
        var chrysalises = drawPile.Cards
            .Where(c => c is AglaeaChrysalis)
            .ToList();

        foreach (var chrysalis in chrysalises)
        {
            drawPile.MoveToTopInternal(chrysalis);
        }

        if (chrysalises.Count > 0)
        {
            drawPile.InvokeContentsChanged();
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 金丝成障：存在衣匠时，你受到的伤害减少 1 点。升级：获得固有。
/// </summary>
public class AglaeaGoldBarrier() : AglaeaCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        TipTailor
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 效果本身不叠层（PowerStackType.Single），层数固定传 1。
        await PowerCmd.Apply<GoldBarrierPower>(
            choiceContext, cardPlay.Player.Creature, 1, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

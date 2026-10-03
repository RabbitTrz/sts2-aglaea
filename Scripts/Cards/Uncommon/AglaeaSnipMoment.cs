using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 裁断之时：给予间隙织线，并获得 12 点[gold]格挡[/gold]。升级后 15 点[gold]格挡[/gold]。
/// </summary>
public class AglaeaSnipMoment() : AglaeaCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(12, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeaveLinePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 给予间隙织线。
        if (cardPlay.Target != null)
        {
            await PowerCmd.Apply<WeaveLinePower>(
                choiceContext, cardPlay.Target, 1, cardPlay.Player.Creature, this);
        }

        // 获得[gold]格挡[/gold]。
        await CreatureCmd.GainBlock(
            cardPlay.Player.Creature, DynamicVars.Block.IntValue, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

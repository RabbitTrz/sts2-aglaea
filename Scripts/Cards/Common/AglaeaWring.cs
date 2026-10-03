using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 绞丝：移除所有「金丝」。每移除 1 层，获得 3 点[gold]格挡[/gold]。升级后每层获得 5 点。
/// </summary>
public class AglaeaWring() : AglaeaCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = cardPlay.Player.Creature;
        var goldenThread = creature.GetPower<GoldenThreadPower>();
        if (goldenThread == null)
        {
            return;
        }

        int removed = goldenThread.Amount;
        await PowerCmd.Remove<GoldenThreadPower>(creature);

        int block = removed * DynamicVars.Block.IntValue;
        await CreatureCmd.GainBlock(creature, block, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}

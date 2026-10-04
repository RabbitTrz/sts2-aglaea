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
/// 绞丝：移除所有「至高之姿」。每移除 1 层，获得 7 点[gold]格挡[/gold]。升级后每层获得 9 点。
/// （v0.1.3 改动：原先移除的是「金丝」，现改为「至高之姿」。）
///
/// 注意：移除至高之姿会连带触发它自身的 AfterRemoved（清空金丝），这是既有设计。
/// </summary>
public class AglaeaWring() : AglaeaCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = cardPlay.Player.Creature;
        var pose = creature.GetPower<SupremePosePower>();
        if (pose == null)
        {
            return;
        }

        int removed = pose.Amount;
        await PowerCmd.Remove<SupremePosePower>(creature);

        int block = removed * DynamicVars.Block.IntValue;
        await CreatureCmd.GainBlock(creature, block, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}

/* 测试牌「命运眼睑」已整体注释移除，代码保留以备后用。
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 命运眼睑：本回合每消耗 1 层「至高之姿」，获得 1 层「至高之姿」。消耗。
/// 升级无变化（费用保持 3）。
/// </summary>
public class AglaeaFateEyelid() : AglaeaCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FateEyelidPower>(
            choiceContext, cardPlay.Player.Creature, 1, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 命运眼睑无升级变化（费用保持 3）。保留空实现以符合测试需要。
    }
}
*/

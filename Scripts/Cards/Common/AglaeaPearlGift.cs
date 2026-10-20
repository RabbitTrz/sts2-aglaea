using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Character;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 华珠之礼：获得 3 层「至高之姿」。升级后获得 5 层。
/// </summary>
public class AglaeaPearlGift() : AglaeaCard(3,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SupremePosePower>("Pose", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 至高之姿的登场演出：仅在层数为 0 且本场战斗还没播过时触发（不阻塞结算）。
        AglaeaVfx.PlaySupremeIntroOnce(cardPlay.Player.Creature);

        await PowerCmd.Apply<SupremePosePower>(
            choiceContext, cardPlay.Player.Creature, DynamicVars["Pose"].IntValue, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Pose"].UpgradeValueBy(2m);
    }
}

using BaseLib.Utils;
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
/// 苦难织者：获得 15 层至高之姿。打出后放回手牌。升级：获得保留。
/// （v0.1.4 改动：7 费 → 6 费；10 层 → 15 层。回手牌不会形成无限循环。）
/// </summary>
public class AglaeaSorrowWeaver() : AglaeaCard(6,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SupremePosePower>("Pose", 15m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 至高之姿的登场演出：仅在层数为 0 且本场战斗还没播过时触发（不阻塞结算）。
        AglaeaVfx.PlaySupremeIntroOnce(cardPlay.Player.Creature);

        await PowerCmd.Apply<SupremePosePower>(
            choiceContext, cardPlay.Player.Creature, DynamicVars["Pose"].IntValue, cardPlay.Player.Creature, this);

        // 打出后放回手牌（与「回缝」同一套写法，已实机验证过）。
        await CardPileCmd.Add(this, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}

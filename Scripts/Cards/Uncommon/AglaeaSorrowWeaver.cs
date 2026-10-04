using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 苦难织者：获得 10 层至高之姿。打出后放回手牌。升级：获得保留。
/// （v0.1.3 改动：10 费 → 7 费；20 层 → 10 层；去掉「消耗」；改为打出后回手牌。
///   7 费是资源门，回手牌不会形成无限循环。）
/// </summary>
public class AglaeaSorrowWeaver() : AglaeaCard(7,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SupremePosePower>("Pose", 10m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
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

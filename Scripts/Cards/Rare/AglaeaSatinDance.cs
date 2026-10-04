using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 缎舞（X 费）：若存在衣匠，使所有衣匠额外攻击 X+1 次。升级：X+2 次。
/// （v0.1.3 改动：基础次数由 X 改为 X+1，升级由 X+1 改为 X+2。）
/// </summary>
public class AglaeaSatinDance() : AglaeaCard(-1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>(),
        TipExtraAttack
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue() + 1;
        if (IsUpgraded)
        {
            x++;
        }

        for (int i = 0; i < x; i++)
        {
            await TailorCmd.AttackAll(choiceContext, cardPlay.Player, 3, extraAttack: true);
        }
    }
}

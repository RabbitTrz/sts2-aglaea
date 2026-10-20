using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MoeNegiMod.Aglaea.Character;
using MoeNegiMod.Aglaea.Powers;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 披姿（X 费）：获得 X 层至高之姿。升级：X+1 层。
/// </summary>
public class AglaeaCladForm() : AglaeaCard(-1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 至高之姿的登场演出：仅在层数为 0 且本场战斗还没播过时触发（不阻塞结算）。
        AglaeaVfx.PlaySupremeIntroOnce(cardPlay.Player.Creature);

        int x = ResolveEnergyXValue();
        if (IsUpgraded)
        {
            x++;
        }

        await PowerCmd.Apply<SupremePosePower>(
            choiceContext, cardPlay.Player.Creature, x, cardPlay.Player.Creature, this);
    }
}

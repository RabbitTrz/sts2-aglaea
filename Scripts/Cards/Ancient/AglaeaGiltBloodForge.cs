using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Powers;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 金血浇铸（先古）：每回合开始时，失去等同于层数的生命，获得 3 倍层数的至高之姿。
/// 升级：费用 −1。
///
/// 每次打出为能力累积 1 层；打第 2 张即为 2 层（每回合失 2 得 6）。
/// 循环写成循环调用，可正确累层。
/// 由「尘封魔典」发放（该遗物从角色卡池随机挑 Ancient 牌，本牌自动进入候选）。
/// </summary>
public class AglaeaGiltBloodForge() : AglaeaCard(2,
    CardType.Power, CardRarity.Ancient,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<GiltBloodForgePower>(
            choiceContext, cardPlay.Player.Creature, 1,
            cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

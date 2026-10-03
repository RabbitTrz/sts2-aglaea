using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 万帷网（多人 · 罕见 · 技能）：所有玩家各召唤 1 名衣匠。
/// 升级：费用 −1。
/// </summary>
public class AglaeaMyriadNet() : AglaeaCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AllAllies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        TipTailor
    ];

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // GetTeammatesOf 已包含出牌者本人（源码注释：including the creature itself），
        // 因此这里就是「所有玩家」，不需要额外补自己。
        // 写法对齐原版 Rally.cs（同为 TargetType.AllAllies + MultiplayerOnly）。
        var teammates = cardPlay.Player.Creature.CombatState
            .GetTeammatesOf(cardPlay.Player.Creature)
            .Where(c => c.IsAlive && c.Player != null)
            .ToList();

        foreach (var teammate in teammates)
        {
            await TailorCmd.Summon(choiceContext, teammate.Player!, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

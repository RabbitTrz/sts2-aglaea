using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Powers;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 领袖魅力（多人 · 稀有 · 能力）：所有玩家每打出 10 张攻击牌，你获得 1 层至高之姿。
/// 升级：获得固有（Innate）。（v0.1.4：原本是「需要牌数 −3」，实现复杂，改为固有。）
/// </summary>
public class AglaeaLeadershipCharm() : AglaeaCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose
    ];

    private const int _attacksRequired = 10;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 每次打出都新建一份独立实例（LeadershipCharmPower 是 Instanced）：
        // 每份各自按自己的门槛累计，叠层 = 产出频率翻倍。
        // 因此这里无需查已有实例，交给 PowerCmd 即可。
        await PowerCmd.Apply<LeadershipCharmPower>(
            choiceContext, cardPlay.Player.Creature, _attacksRequired, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

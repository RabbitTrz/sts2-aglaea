using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 作茧自缚：若存在衣匠，则造成 3 点伤害并使最左侧的衣匠自毁。升级后 6 点。
/// </summary>
public class AglaeaCocoonSelfTrap() : AglaeaCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var tailors = TailorCmd.AliveTailors(cardPlay.Player);

        // 只有存在衣匠时才造成伤害；否则无事发生。
        if (tailors.Count == 0)
        {
            return;
        }

        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        // 最左侧的衣匠自毁（列表第一个存活的）。
        await CreatureCmd.Kill(tailors[0]);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

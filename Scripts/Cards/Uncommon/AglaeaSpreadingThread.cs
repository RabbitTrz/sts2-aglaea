using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 传线连绵：间隙织线状态下的敌人在死亡时，使随机一名敌人获得间隙织线，
/// 并使其受到 10 点伤害。升级后 15 点伤害。
/// </summary>
public class AglaeaSpreadingThread() : AglaeaCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        TipWeaveLine
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SpreadingThreadPower>("Damage", 10m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SpreadingThreadPower>(
            choiceContext, cardPlay.Player.Creature,
            DynamicVars["Damage"].IntValue, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy(5m);
    }
}

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 战甲（状态牌）：0 费，可打出。保留、消耗。获得 1 费，获得 8 点[gold]格挡[/gold]。
/// 由天谴斗士的【战甲】效果塞进玩家手牌——虽然是「敌人塞给你的」，但打出来是纯收益。
/// </summary>
[Pool(typeof(StatusCardPool))]
public class AglaeaWarplate() : AglaeaCard(0,
    CardType.Status, CardRarity.Status,
    TargetType.Self)
{
    // v0.1.4 补：打出后消耗（2026-10-04 用户补填）。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        new BlockVar(8, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, cardPlay.Player);
        await CommonActions.CardBlock(this, cardPlay);
    }
}

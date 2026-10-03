using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 金星行列：使衣匠的最大生命值加 3。升级后加 4。
/// </summary>
public class AglaeaGoldStarRow() : AglaeaCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<TailorMaxHpBonusPower>("Bonus", 3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorMaxHpBonusPower>(),
        TipTailor
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int bonus = DynamicVars["Bonus"].IntValue;

        // 对当前场上所有衣匠直接增加最大生命。
        foreach (var tailor in TailorCmd.AliveTailors(cardPlay.Player))
        {
            await CreatureCmd.GainMaxHp(tailor, bonus);
        }

        // 挂载 power，使后续召唤的衣匠也获得加成。
        await PowerCmd.Apply<TailorMaxHpBonusPower>(
            choiceContext, cardPlay.Player.Creature,
            bonus, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

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
/// 锻泪匠躯：衣匠死亡时，对所有敌人造成 10 点伤害（视为衣匠攻击）。升级后 15 点。
/// （v0.1.3 改动：原先的效果是「衣匠受到伤害时由所有衣匠均摊」。）
/// </summary>
public class AglaeaForgedTearBody() : AglaeaCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        TipTailor
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<ForgedTearBodyPower>("Damage", 10m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ForgedTearBodyPower>(
            choiceContext, cardPlay.Player.Creature,
            DynamicVars["Damage"].IntValue, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy(5m);
    }
}

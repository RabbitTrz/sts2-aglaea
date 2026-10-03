using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 枯草之盈：造成 20 点伤害。本回合每抽到 1 张牌，降低 1 费。
/// 升级后降低 1 费。
/// </summary>
public class AglaeaWitheredPlenty() : AglaeaCard(4,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
    }

    // 本回合内每抽到 1 张牌（任意牌），此牌在本回合内降低 1 费。
    // 回合开始时的初始抽牌（fromHandDraw）不计入。
    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (fromHandDraw)
        {
            return Task.CompletedTask;
        }

        if (card.Owner?.Creature == Owner.Creature && card != this)
        {
            EnergyCost.AddThisTurn(-1);
        }
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 予你盛装：造成 3 点伤害，将 2 张连携打击放入手牌。升级：改为放入 2 张连携打击+。
/// （v0.1.3 改动：技能牌 → 攻击牌；加 3 点伤害；张数 3 → 2；去掉「消耗」。）
/// </summary>
public class AglaeaBestowGarb() : AglaeaCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<AglaeaLinkedStrike>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        if (!IsUpgraded)
        {
            await CardPileCmd.AddToCombatAndPreview<AglaeaLinkedStrike>(
                cardPlay.Player.Creature, PileType.Hand, DynamicVars.Cards.IntValue, cardPlay.Player);
            return;
        }

        // 升级：放入升过级的连携打击+。
        var combat = cardPlay.Player.Creature.CombatState;
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var card = combat.CreateCard<AglaeaLinkedStrike>(cardPlay.Player);
            CardCmd.Upgrade(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, cardPlay.Player);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级效果由 OnPlay 分支实现，这里无需改动数值。
    }
}

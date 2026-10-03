using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 筛缕：造成 6 点伤害，抽 4 张牌，并丢弃其中的非攻击牌。升级后 9 点伤害。
/// </summary>
public class AglaeaSiftThread() : AglaeaCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new CardsVar(4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        // 抽 4 张牌。
        var drawn = await CardPileCmd.Draw(
            choiceContext, DynamicVars.Cards.IntValue, cardPlay.Player);

        // 丢弃其中所有非攻击牌。
        var nonAttacks = drawn.Where(c => c.Type != CardType.Attack).ToList();
        if (nonAttacks.Count > 0)
        {
            await CardCmd.Discard(choiceContext, nonAttacks);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

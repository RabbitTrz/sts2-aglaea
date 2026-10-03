using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 千针刺芒：造成 6 点伤害 X 次。X 大于等于 3 时，额外造成 1 次伤害。
/// 升级后 7 点伤害， X 大于等于 3 时额外 2 次。
/// </summary>
public class AglaeaThousandNeedles() : AglaeaCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.RandomEnemy)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new RepeatVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();

        // 基础 X 次。
        for (int i = 0; i < x; i++)
        {
            await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
        }

        // X 大于等于 3 时，额外造成 Repeat 次。
        if (x >= 3)
        {
            for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
            {
                await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}

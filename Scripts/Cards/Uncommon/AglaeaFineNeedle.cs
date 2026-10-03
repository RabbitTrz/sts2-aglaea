using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Monsters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 密针：造成 4 点伤害 3 次。每当有衣匠死亡，此牌在本战斗内降低 1 费。
/// 升级后 5 点伤害 3 次。
/// </summary>
public class AglaeaFineNeedle() : AglaeaCard(3,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        TipTailor
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move),
        new RepeatVar(3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
        }
    }

    // 衣匠死亡时，此牌在本场战斗中永久降低 1 费。
    public override Task AfterDeath(
        PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented)
        {
            return Task.CompletedTask;
        }
        // AfterDeath 也会在牌库中的持久化副本上触发（Hook 遍历 RunState 的牌库），
        // 若不排除会导致降费永久留在牌库卡上、跨战斗累积。仅战斗内实例生效。
        if (!IsInCombat)
        {
            return Task.CompletedTask;
        }
        if (creature.Monster is AglaeaTailorMonster)
        {
            EnergyCost.AddThisCombat(-1);
        }
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}

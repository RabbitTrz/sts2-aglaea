using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 蚀茧：消耗抽牌堆中的所有若虫。每消耗一张造成 10 点伤害。升级后每张 15 点。
/// </summary>
public class AglaeaCorrodeCocoon() : AglaeaCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<AglaeaChrysalis>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Player;
        var target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        // 消耗抽牌堆中的所有若虫。
        var chrysalises = CardPile.GetCards(player, PileType.Draw)
            .Where(c => c is AglaeaChrysalis)
            .ToList();

        foreach (var chrysalis in chrysalises)
        {
            await CardCmd.Exhaust(choiceContext, chrysalis);
        }

        // 每消耗一张造成一次伤害。
        int damage = DynamicVars.Damage.IntValue;
        for (int i = 0; i < chrysalises.Count; i++)
        {
            await CreatureCmd.Damage(
                choiceContext, target, damage, ValueProp.Move, player.Creature);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}

using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 断念：消耗手牌中所有攻击牌。每消耗一张，使全体敌人本回合减少 2 点力量。
/// 升级后每张 -3 点力量。
/// </summary>
public class AglaeaBreakThought() : AglaeaCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<PiercingWailPower>("StrengthLoss", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Player;

        // 快照手牌中的攻击牌。
        var attacks = CardPile.GetCards(player, PileType.Hand)
            .Where(c => c.Type == CardType.Attack)
            .ToList();

        int exhausted = 0;
        foreach (var card in attacks)
        {
            await CardCmd.Exhaust(choiceContext, card);
            exhausted++;
        }

        if (exhausted == 0)
        {
            return;
        }

        // 每个敌人失去 exhausted * StrengthLoss 点力量。
        var enemies = player.Creature.CombatState.Enemies.Where(e => e.IsAlive);
        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<PiercingWailPower>(
                choiceContext, enemy, DynamicVars["StrengthLoss"].BaseValue * exhausted, player.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthLoss"].UpgradeValueBy(1m);
    }
}

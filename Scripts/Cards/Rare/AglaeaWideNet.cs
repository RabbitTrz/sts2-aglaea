using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 广织成网：衣匠的攻击会对所有敌人造成伤害。若存在衣匠，使随机衣匠攻击 1 次。
/// 升级后攻击 2 次。
/// </summary>
public class AglaeaWideNet() : AglaeaCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        TipTailor,
        TipExtraAttack
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new RepeatVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Player;
        var creature = player.Creature;

        await PowerCmd.Apply<WideNetPower>(choiceContext, creature, 1, creature, this);

        // 若存在衣匠，使随机衣匠攻击 N 次。
        if (TailorCmd.TailorCount(player) > 0)
        {
            int attacks = DynamicVars.Repeat.IntValue;
            for (int i = 0; i < attacks; i++)
            {
                await TailorCmd.AttackOnce(choiceContext, player, TailorCmd.TailorAttackDamage, extraAttack: true);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}

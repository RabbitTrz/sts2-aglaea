using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 作茧自缚：造成 3 点伤害。若存在衣匠，使生命值最低的衣匠恢复 2 点生命值。升级后 6 点伤害。
/// </summary>
public class AglaeaCocoonSelfTrap() : AglaeaCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    /// <summary>治疗量。固定 2，不随升级变化（升级只加伤害）。</summary>
    private const decimal HealAmount = 2m;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        // 使生命值最低的衣匠恢复 2 点生命。没有衣匠时只是不治疗，伤害照常结算。
        var lowest = TailorCmd.AliveTailors(cardPlay.Player)
            .OrderBy(c => c.CurrentHp)
            .FirstOrDefault();
        if (lowest != null)
        {
            await CreatureCmd.Heal(lowest, HealAmount);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

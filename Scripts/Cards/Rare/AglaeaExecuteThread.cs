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
/// 诛线：造成 20 点伤害，使衣匠的最大生命值提升 1 点。
/// 无条件生效、不带消耗；加成只在本场战斗内有效，不跨战斗继承。升级后提升 2 点。
/// </summary>
public class AglaeaExecuteThread() : AglaeaCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(20, ValueProp.Move),
        new PowerVar<TailorMaxHpBonusPower>("Bonus", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorMaxHpBonusPower>(),
        TipTailor
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        int bonus = DynamicVars["Bonus"].IntValue;

        // 本场立即生效：对已在场的衣匠直接增加最大生命（与「金星行列」同机制）。
        foreach (var tailor in TailorCmd.AliveTailors(cardPlay.Player))
        {
            await CreatureCmd.GainMaxHp(tailor, bonus);
        }

        // 挂载 power，使本场后续召唤的衣匠也获得加成。随战斗结束清理，不跨战斗继承。
        await PowerCmd.Apply<TailorMaxHpBonusPower>(
            choiceContext, cardPlay.Player.Creature, bonus, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}

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
/// 执行者（先古）：固有。造成 10 点伤害，给予间隙织线。若存在衣匠，使随机衣匠攻击 2 次。
/// 升级：伤害 +5，衣匠攻击次数 +1。
///
/// 由「古老牙齿」把起始牌【裁剪】转化而来（见 <see cref="AglaeaArchaicToothPatch"/>）。
/// </summary>
public class AglaeaExecutor() : AglaeaCard(0,
    CardType.Attack, CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move),
        new RepeatVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeaveLinePower>(),
        HoverTipFactory.FromPower<TailorSwarmPower>(),
        TipExtraAttack
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
        await PowerCmd.Apply<WeaveLinePower>(
            choiceContext, cardPlay.Target, 1, cardPlay.Player.Creature, this, false);

        // 使随机衣匠攻击若干次（无衣匠时 AttackOnce 内部直接返回）。
        // 与「热狂圆舞」同词条，写法保持一致。
        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            await TailorCmd.AttackOnce(
                choiceContext, cardPlay.Player, TailorCmd.TailorAttackDamage, extraAttack: true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}

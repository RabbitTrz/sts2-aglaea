using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 浪漫合奏（多人 · 稀有 · 攻击）：造成 6 点伤害。
/// 每当其他玩家打出「除浪漫合奏本身以外」的攻击牌时，无论这张牌处于何处都会回到你的手牌。
/// 升级：伤害 +3。
/// </summary>
public class AglaeaRomanticEnsemble() : AglaeaCard(0,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 挂载回收 power（已存在则刷新层数）。打出后本牌进入弃牌堆，
        // 由该 power 在其他玩家打出攻击牌时把它收回手牌。
        await PowerCmd.Apply<RomanticEnsemblePower>(
            choiceContext, cardPlay.Player.Creature, 1, cardPlay.Player.Creature, this);

        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

/* 测试牌「播种若虫」已整体注释移除，代码保留以备后用。
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Aglaea.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 播种若虫（普通攻击牌）：造成伤害，并将一张若虫放入抽牌堆。
/// 用于直接测试若虫「进抽牌堆 → 抽到自动打出」的机制，绕开衣匠死亡路径的不确定性。
/// </summary>
public class AglaeaChrysalisSow() : AglaeaCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<AglaeaChrysalis>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        // 将一张若虫放入抽牌堆（首次会挂载若虫驱动 power）。
        await TailorCmd.AddChrysalisToDrawPile(choiceContext, cardPlay.Player, this, 1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
*/

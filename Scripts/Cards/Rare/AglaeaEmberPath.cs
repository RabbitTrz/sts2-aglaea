using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 逐火之路：战斗结束后，获得 {Amount} 个遗物「火种」。升级：固有。
/// </summary>
public class AglaeaEmberPath() : AglaeaCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{

    /// <summary>
    /// 每场战斗发放的火种数量。卡面用 {Amount} 读取它，升级走 UpgradeValueBy
    /// 而不是在 OnPlay 里写 <c>IsUpgraded ? 2 : 1</c> —— 后者卡面永远显示 1，
    /// 因为升级改不到这个临时局部变量。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<EmberPathPower>(
            choiceContext, cardPlay.Player.Creature,
            DynamicVars["Amount"].IntValue, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}


using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 沐浴黄金：获得 7 层金丝。下一次衣匠攻击后，移除 7 层金丝。
/// 升级后 10 层。
/// </summary>
public class AglaeaBatheGold() : AglaeaCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<BatheGoldPower>("Gold", 7m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<GoldenThreadPower>(),
        TipTailor
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 先获得金丝。变量用数组索引器访问 PowerVar，避免读取成 GoldVar 抛 InvalidCastException。
        await PowerCmd.Apply<GoldenThreadPower>(
            choiceContext, cardPlay.Player.Creature, DynamicVars["Gold"].IntValue, cardPlay.Player.Creature, this);

        // 再挂载「下次攻击后移除」标记。
        await PowerCmd.Apply<BatheGoldPower>(
            choiceContext, cardPlay.Player.Creature, DynamicVars["Gold"].IntValue, cardPlay.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Gold"].UpgradeValueBy(3m);
    }
}

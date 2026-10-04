using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 二律背反（状态牌）：0 费，可打出。获得 1 费，受到 3 点伤害（可被格挡）。
/// 由赞达尔的【智识的二律背反】塞进玩家手牌。
///
/// 自伤走 <c>Move | Unpowered</c>（与凡庸/灼伤等原版自伤牌一致）：可被格挡、
/// 不被力量加成，也不会触发「本回合是否受到过伤害」之外的强度类计算。
/// </summary>
[Pool(typeof(StatusCardPool))]
public class AglaeaAntinomy() : AglaeaCard(0,
    CardType.Status, CardRarity.Status,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        new DamageVar(3, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, cardPlay.Player);

        await CreatureCmd.Damage(
            choiceContext, cardPlay.Player.Creature,
            DynamicVars.Damage.IntValue, ValueProp.Move | ValueProp.Unpowered, null);
    }
}

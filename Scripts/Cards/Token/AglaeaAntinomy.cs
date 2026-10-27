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
/// 二律背反（状态牌）：0 费，可打出。获得 1 费，受到 6 点伤害（可被格挡）。消耗。
/// 由赞达尔的【智识的二律背反】塞进玩家手牌。
///
/// 自伤固定 6 点，走 <c>Move | Unpowered</c>（与灼伤 Burn 等原版自伤牌一致）：
/// 可被格挡、不被力量加成、也不吃虚弱减伤。注意 DamageVar 与运行时必须用同一组属性
/// （2026-10-05 修复：此前 DamageVar 少了 Unpowered，卡面会显示被虚弱削减后的伤害）。
/// </summary>
[Pool(typeof(StatusCardPool))]
public class AglaeaAntinomy() : AglaeaCard(0,
    CardType.Status, CardRarity.Status,
    TargetType.Self)
{
    // v0.1.4 补：打出后消耗（2026-10-04 用户补填）。
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        // 固定 3 点：必须带 Unpowered（与灼伤 Burn 的写法一致），
        // 否则卡面预览会把玩家的力量/虚弱算进来，显示成浮动值。
        new DamageVar(6, ValueProp.Move | ValueProp.Unpowered)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, cardPlay.Player);

        // 伤害属性统一由 DamageVar 提供（对齐原版自伤牌 Burn 的写法）。
        await CreatureCmd.Damage(
            choiceContext, cardPlay.Player.Creature, DynamicVars.Damage, this, cardPlay);
    }
}

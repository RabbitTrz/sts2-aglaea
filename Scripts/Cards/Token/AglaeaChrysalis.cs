using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 若虫（无色 Token 牌）：3 费，获得 1 点能量，抽 1 张牌。消耗。
/// 由衣匠死亡 / 召唤满员时放入抽牌堆（见 TailorSwarmPower 与 TailorCmd）。
/// 「被抽到时自动打出」由 <see cref="Powers.ChrysalisSwarmPower"/> 驱动（挂在玩家身上），
/// 该 power 监听抽卡并自动打出若虫，避免在卡牌构造器里订阅 Drawn 事件引入重入风险。
/// 升级后：获得 2 点能量。
///
/// 注意：必须注册进 TokenCardPool（游戏本体的无色 Token 池，与小刀/燃料同级），
/// 否则战斗内 CreateCard 会因卡片不属于任何卡池而抛 "not in any card pool" 异常。
/// </summary>
[Pool(typeof(TokenCardPool))]
public class AglaeaChrysalis() : AglaeaCard(3,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        new CardsVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMe(Owner))
        {
            VfxCmd.PlayFullScreenInCombat("vfx/vfx_adrenaline", Owner.Creature);
        }
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}

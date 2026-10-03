using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

// ============================================================
// 复缕（暂时停用 2026-09-27）
//
// 原因：机制上有洞 —— 0 费 + 打出后复制品进手牌 + 每次打出攻击牌都会触发衣匠攻击，
// 三者叠加可以无限刷衣匠攻击。新的「打出后回手、复制品进弃牌堆」方案也还是不够优雅，
// 待重新设计后再启用。设计表的 #28 行同步作废。
//
// 启用时：解开下面的块注释即可，卡牌由 BaseLib 自动注册，无需改别的地方。
// 本地化文案（Aglaea/localization/zhs/cards.json 里的 MOENEGIMOD-AGLAEA_RE_THREAD.*）
// 保持原样，未做改动。
// ============================================================

/// <summary>
/// 复缕：造成 3 点伤害 2 次。
/// 升级后增加 1 次。
/// </summary>
public class AglaeaReThread() : AglaeaCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, ValueProp.Move), new RepeatVar(2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}


// ============================================================
// 纠缠（卡牌形态，2026-10-05 停用）
//
// 用户改设计：【纠缠】不再做成卡牌，改为玩家身上的 Power ——
// 「抽到的下 n 张牌在本场战斗中的耗能 +1」，见 Scripts/Power/EntanglePower.cs。
// 本卡牌连同它依赖的「抽到第二张就锁死出牌」机制（EntangleLockPower）一起注释停用。
//
// 本地化文案保留（MOENEGIMOD-AGLAEA_ENTANGLE.*），需要恢复时解开下面的块注释。
// ============================================================
/*
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 纠缠（状态牌）：不能被打出。
/// 这张牌在手牌中时，如果再次抽到纠缠，则本回合无法再出牌，同时消耗这两张纠缠。
///
/// 「本回合无法再出牌」由 <see cref="EntangleLockPower"/> 实现（与凡庸同效）。
/// 由赞达尔的【求解：第一因之果】与天谴斗士的【震荡】塞进玩家牌堆。
/// </summary>
[Pool(typeof(StatusCardPool))]
public class AglaeaEntangle() : AglaeaCard(-1,
    CardType.Status, CardRarity.Status,
    TargetType.None)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    /// <summary>
    /// 抽到本牌时检查：手上若已有另一张纠缠，则锁死出牌并消耗两张。
    /// </summary>
    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this || Owner == null)
        {
            return;
        }

        // 手里已有的【另一张】纠缠。
        var others = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c is AglaeaEntangle && c != this)
            .ToList();
        if (others.Count == 0)
        {
            return;
        }

        // 本回合无法再出牌（多次触发只挂一层，Single 不叠）。
        await PowerCmd.Apply<EntangleLockPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, null);

        // 消耗这两张纠缠。
        await CardCmd.Exhaust(choiceContext, this);
        foreach (var other in others)
        {
            await CardCmd.Exhaust(choiceContext, other);
        }
    }
}
*/

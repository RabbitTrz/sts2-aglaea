using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 纠缠（玩家身上的 Debuff，Counter 递减）：
/// 抽到的下 Amount 张牌，在本场战斗中的耗能 +1（每抽到 1 张减 1 层）。
///
/// 2026-10-05 由卡牌改为 Power（用户改设计）；由赞达尔施放【求解：第一因之果】时
/// 按 CALL 层数给予所有玩家（每层 CALL → 每人 1 层纠缠）。
/// </summary>
public sealed class EntanglePower : AglaeaPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>抽到的牌（只算该玩家自己抽的）本场耗能 +1，然后计数 -1。</summary>
    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (Owner.Player == null || card.Owner != Owner.Player)
        {
            return;
        }

        card.EnergyCost.AddThisCombat(1);
        await PowerCmd.Decrement(this);
    }
}

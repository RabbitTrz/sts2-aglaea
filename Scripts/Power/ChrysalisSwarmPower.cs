using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MoeNegiMod.Aglaea.Cards;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 若虫驱动（玩家身上的隐藏 Buff）：让「若虫」牌在被抽到时自动打出。
/// 参考原版 HellraiserPower.AfterCardDrawnEarly 的自动打出模式。
/// 该 power 常驻玩家身上，随战斗开始挂载。
/// </summary>
public sealed class ChrysalisSwarmPower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    private sealed class Data
    {
        // 防止自动打出过程中再次触发自身（重入保护）。
        public readonly HashSet<CardModel> AutoPlayingCards = new();
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

    // 抽到若虫时自动打出（AfterCardDrawnEarly 比 AfterCardDrawn 更早，保证在进入手牌前处理）。
    public override async Task AfterCardDrawnEarly(
        PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card is not AglaeaChrysalis)
        {
            return;
        }

        if (card.Owner?.Creature != Owner)
        {
            return;
        }

        var data = GetInternalData<Data>();
        if (data.AutoPlayingCards.Contains(card))
        {
            return;
        }

        data.AutoPlayingCards.Add(card);
        try
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
        finally
        {
            data.AutoPlayingCards.Remove(card);
        }
    }
}

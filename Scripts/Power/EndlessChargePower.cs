using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 蓄势绵延（玩家身上的 Buff）：每消耗 2 层「至高之姿」，获得 1 层「至高之姿」。
/// 内部计数累积至高之姿消耗层数，每满 2 层返还 1 层（余数保留）。
/// </summary>
public sealed class EndlessChargePower : AglaeaPower, ISupremePoseConsumeListener
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    private const int ConsumedPerPose = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private sealed class Data
    {
        public int Accumulated;
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

    public async Task OnSupremePoseConsumed(int layers)
    {
        if (layers <= 0)
        {
            return;
        }

        var data = GetInternalData<Data>();
        data.Accumulated += layers;

        while (data.Accumulated >= ConsumedPerPose)
        {
            data.Accumulated -= ConsumedPerPose;
            await PowerCmd.Apply<SupremePosePower>(
                new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
        }
    }
}

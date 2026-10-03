using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MoeNegiMod.Aglaea.Relics;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 逐火之路（玩家身上的隐藏 Buff）：战斗结束后，为每 1 层生成 1 个「火种」战利品。
/// 战利品需要玩家主动点击才会领取（与静默猎手的「狩猎」、亡灵契约师的「禁忌魔典」一致），
/// 跳过则什么也不获得。层数 = 升级前 1、升级后 2。
/// </summary>
public sealed class EmberPathPower : AglaeaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner.Player;
        if (player == null)
        {
            return;
        }

        // 每层生成 1 个战利品，内容为「火种」遗物。
        //
        // 每个战利品都用【新造的】可变实例：RelicCmd.Obtain 不去重，点击时会把
        // 这个实例直接塞进遗物列表并调 AfterObtained，由它负责「已有则 +1 层、
        // 没有则层数置 1」以及把多余实例合并掉。
        for (int i = 0; i < Amount; i++)
        {
            var relic = ModelDb.Relic<AglaeaEmberSeed>().ToMutable();
            room.AddExtraReward(player, new RelicReward(relic, player));
        }

        // 结算后必须移除自身：否则本场打出的「逐火之路」会一直挂着，
        // 之后每场战斗结束时都再白送一次战利品。
        // 移除后即回到「本场战斗中打出，才在本场结束获得战利品」的设计语义。
        await PowerCmd.Remove(this);
    }
}

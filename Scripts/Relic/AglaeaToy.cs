using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Commands;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

public class AglaeaToy : AglaeaRelics
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    // 战斗开始时召唤一名衣匠。
    public override async Task BeforeCombatStart()
    {
        await TailorCmd.Summon(new BlockingPlayerChoiceContext(), Owner, this);
    }
}
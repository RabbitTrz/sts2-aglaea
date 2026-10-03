using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MoeNegiMod.Aglaea.Monsters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 浴光回春（玩家身上的 Buff）：回合开始时，所有存活的衣匠回复满生命值。
/// </summary>
public sealed class SunbathPower : AglaeaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TailorSwarmPower>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        foreach (var pet in Owner.Pets.Where(c => c.IsAlive && c.Monster is AglaeaTailorMonster))
        {
            decimal missing = pet.MaxHp - pet.CurrentHp;
            if (missing > 0m)
            {
                await CreatureCmd.Heal(pet, missing);
            }
        }
    }
}

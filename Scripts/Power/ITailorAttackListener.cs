using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 衣匠攻击监听接口。实现此接口的 power（挂在玩家身上）会在每次衣匠攻击前/后被 TailorCmd 回调。
/// 供「回纺为守」「沐浴黄金」「广织成网」「锻泪匠躯」等能力牌使用。
/// </summary>
public interface ITailorAttackListener
{
    Task BeforeTailorAttack(
        PlayerChoiceContext choiceContext, Player player, Creature tailor, Creature target);

    Task AfterTailorAttack(
        PlayerChoiceContext choiceContext, Player player, Creature tailor, Creature target, int damage);
}

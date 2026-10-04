using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Cards;

/// <summary>
/// 火种归还：保留。消耗。移除自身的至高之姿，将力量与敏捷变为 0。
/// 每清除 1 层至高之姿，清除 1 个最后附着的负面效果。
/// 升级：固有。
///
/// 「最后附着的负面效果」= 玩家身上按附着顺序排列的 Debuff 列表中的最后一个。
/// <see cref="MegaCrit.Sts2.Core.Entities.Creatures.Creature.Powers"/> 保持附着顺序，
/// 因此取反向枚举里的第一个 Debuff 即为「最后附着」。
/// </summary>
public class AglaeaEmberReturn() : AglaeaCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        .. TipPose,
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = cardPlay.Player.Creature;

        // 走「直接移除」入口：会通知静息蓄势按层数给力量。
        int poseLayers = await SupremePoseCmd.RemoveAllDirectly(choiceContext, cardPlay.Player, this);

        // 力量与敏捷清零。
        await ZeroOut<StrengthPower>(creature);
        await ZeroOut<DexterityPower>(creature);

        // 每清除 1 层至高之姿，移除 1 个最后附着的负面效果。
        for (int i = 0; i < poseLayers; i++)
        {
            var debuff = creature.Powers
                .Where(p => p.TypeForCurrentAmount == PowerType.Debuff
                            && p is not SupremePosePower)
                .LastOrDefault();
            if (debuff == null)
            {
                break;
            }

            await PowerCmd.Remove(debuff);
        }
    }

    /// <summary>把该 power 的层数归零（移除而不返还，负值同样清零）。</summary>
    private static async Task ZeroOut<T>(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature)
        where T : PowerModel
    {
        var power = creature.GetPower<T>();
        if (power == null)
        {
            return;
        }

        await PowerCmd.Remove(power);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

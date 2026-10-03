using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Aglaea.Commands;
using MoeNegiMod.Aglaea.Powers;
using System.Threading.Tasks;

namespace MoeNegiMod.Aglaea.Relics;

/// <summary>
/// 熔光手链（阿格莱雅的先古升级起始遗物）：战斗开始时召唤 1 名衣匠，且衣匠最大生命值 +2。
///
/// 由「欧洛巴斯之触」把起始遗物 <see cref="AglaeaToy"/>（黄金手链）替换而来，
/// 替换映射由 <see cref="AglaeaOrobasPatch"/> 注入。
/// </summary>
public class AglaeaMoltenBracelet : AglaeaRelics
{
    private const int _tailorMaxHpBonus = 2;

    // 与黄金手链同为起始遗物，使 TouchOfOrobas 的 GetStarterRelic 能识别。
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeCombatStart()
    {
        await TailorCmd.Summon(new BlockingPlayerChoiceContext(), Owner, this);

        // 对刚召唤出的衣匠直接加最大生命。
        foreach (var tailor in TailorCmd.AliveTailors(Owner))
        {
            await CreatureCmd.GainMaxHp(tailor, _tailorMaxHpBonus);
        }

        // 挂载 power，使本场后续召唤的衣匠也获得加成。
        await PowerCmd.Apply<TailorMaxHpBonusPower>(
            new BlockingPlayerChoiceContext(), Owner.Creature, _tailorMaxHpBonus, Owner.Creature, null);
    }
}

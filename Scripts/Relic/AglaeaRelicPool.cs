using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MoeNegiMod.Aglaea.Character;
using MoeNegiMod.Aglaea.Relics;
using System;
using System.Collections.Generic;

namespace MoeNegiMod.Aglaea.Character;

public partial class AglaeaRelicPool : CustomRelicPoolModel
{
    public override string EnergyColorName => Aglaea.CharacterId;

    public override Color LabOutlineColor => Aglaea.Color;

    /// <summary>
    /// 三个专属遗物（反重力皮靴 / 火力风暴潮 / 光速螺旋桨）也加入本池，
    /// 这样它们会进入 <c>ModelDb.AllRelics</c>，并出现在图鉴的「事件」分类里。
    ///
    /// 它们不会因此变成随机掉落：稀有度是 Event，原版随机遗物奖励只抽
    /// Common/Uncommon/Rare；商店也因 IsAllowedInShops = false 不会出售。
    /// </summary>
    protected override IEnumerable<RelicModel> GenerateAllRelics()
    {
        yield return ModelDb.Relic<AglaeaAntiGravityBoots>();
        yield return ModelDb.Relic<AglaeaFirestormTide>();
        yield return ModelDb.Relic<AglaeaLightSpeedPropeller>();
    }
}
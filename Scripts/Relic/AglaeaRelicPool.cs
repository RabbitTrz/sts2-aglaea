using BaseLib.Abstracts;
using Godot;
using MoeNegiMod.Aglaea.Character;
using System;

namespace MoeNegiMod.Aglaea.Character;

public partial class AglaeaRelicPool : CustomRelicPoolModel
{
    public override string EnergyColorName => Aglaea.CharacterId;

    public override Color LabOutlineColor => Aglaea.Color;
}
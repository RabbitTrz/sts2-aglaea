using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MoeNegiMod.Aglaea.Extensions;

namespace MoeNegiMod.Aglaea.Powers;

public abstract class AglaeaPower : CustomPowerModel
{
	public override string CustomPackedIconPath
	{
		get
		{
			var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
			return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
		}
	}

	public override string CustomBigIconPath
	{
		get
		{
			var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
			return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
		}
	}
}

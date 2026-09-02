using System;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Hazard;

public abstract class HazardDefinition : TheLastStand.Framework.Serialization.Definition
{
	[Flags]
	public enum E_HazardType
	{
		None = 0,
		Fog = 1,
		LightFog = 2
	}

	public abstract E_HazardType HazardType { get; }

	protected HazardDefinition(XContainer container)
		: base(container)
	{
	}
}

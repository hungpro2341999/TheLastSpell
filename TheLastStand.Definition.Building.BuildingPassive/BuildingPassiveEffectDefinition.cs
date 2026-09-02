using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.BuildingPassive;

public abstract class BuildingPassiveEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public BuildingPassiveEffectDefinition(XContainer container)
		: base(container)
	{
	}
}

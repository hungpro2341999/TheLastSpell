using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class DestroyBuildingDefinition : BuildingPassiveEffectDefinition
{
	public DestroyBuildingDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}

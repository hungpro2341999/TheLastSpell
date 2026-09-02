using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingAction;

public class RevealDangerIndicatorsBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public RevealDangerIndicatorsBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}
}

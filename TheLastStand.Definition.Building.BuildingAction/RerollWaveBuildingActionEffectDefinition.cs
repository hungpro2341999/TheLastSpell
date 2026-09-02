using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingAction;

public class RerollWaveBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	public RerollWaveBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}
}

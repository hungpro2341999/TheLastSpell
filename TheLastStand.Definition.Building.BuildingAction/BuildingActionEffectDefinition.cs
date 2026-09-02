using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.BuildingAction;

public abstract class BuildingActionEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_BuildingActionTargeting
	{
		All,
		Single
	}

	public BuildingActionDefinition BuildingActionDefinitionContainer { get; }

	public virtual string ActionEstimationIconId { get; } = string.Empty;

	public BuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer)
	{
		BuildingActionDefinitionContainer = buildingActionDefinitionContainer;
	}

	public override void Deserialize(XContainer xContainer)
	{
	}
}

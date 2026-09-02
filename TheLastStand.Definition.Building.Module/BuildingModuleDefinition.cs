using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.Module;

public abstract class BuildingModuleDefinition : TheLastStand.Framework.Serialization.Definition
{
	public readonly BuildingDefinition BuildingDefinition;

	protected BuildingModuleDefinition(BuildingDefinition buildingDefinition, XContainer moduleDefinitionContainer)
		: base(null)
	{
		BuildingDefinition = buildingDefinition;
		Deserialize(moduleDefinitionContainer);
	}
}

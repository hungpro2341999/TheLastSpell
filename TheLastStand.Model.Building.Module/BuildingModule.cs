using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;

namespace TheLastStand.Model.Building.Module;

public abstract class BuildingModule
{
	public BuildingModuleController BuildingModuleController { get; private set; }

	public BuildingModuleDefinition BuildingModuleDefinition { get; private set; }

	public Building BuildingParent { get; private set; }

	public BuildingModule(Building buildingParent, BuildingModuleDefinition buildingModuleDefinition, BuildingModuleController buildingModuleController)
	{
		BuildingParent = buildingParent;
		BuildingModuleController = buildingModuleController;
		BuildingModuleDefinition = buildingModuleDefinition;
	}
}

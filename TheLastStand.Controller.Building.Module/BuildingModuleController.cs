using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.Module;

public abstract class BuildingModuleController
{
	public BuildingController BuildingControllerParent { get; private set; }

	public BuildingModule BuildingModule { get; private set; }

	public BuildingModuleController(BuildingController buildingControllerParent, BuildingModuleDefinition buildingModuleDefinition)
	{
		BuildingControllerParent = buildingControllerParent;
		BuildingModule = CreateModel(buildingControllerParent.Building, buildingModuleDefinition);
	}

	protected abstract BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition);
}

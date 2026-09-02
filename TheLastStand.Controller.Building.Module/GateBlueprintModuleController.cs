using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.Module;

public class GateBlueprintModuleController : BlueprintModuleController
{
	public GateBlueprintModule GateBlueprintModule { get; }

	public GateBlueprintModuleController(BuildingController buildingControllerParent, BlueprintModuleDefinition blueprintModuleDefinition)
		: base(buildingControllerParent, blueprintModuleDefinition)
	{
		GateBlueprintModule = base.BuildingModule as GateBlueprintModule;
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new GateBlueprintModule(building, buildingModuleDefinition as BlueprintModuleDefinition, this);
	}
}

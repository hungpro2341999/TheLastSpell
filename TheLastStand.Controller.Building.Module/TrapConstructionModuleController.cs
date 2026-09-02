using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.Module;

public class TrapConstructionModuleController : ConstructionModuleController
{
	public TrapConstructionModule TrapConstructionModule { get; }

	public TrapConstructionModuleController(BuildingController buildingControllerParent, ConstructionModuleDefinition constructionModuleDefinition)
		: base(buildingControllerParent, constructionModuleDefinition)
	{
		TrapConstructionModule = base.BuildingModule as TrapConstructionModule;
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new TrapConstructionModule(building, buildingModuleDefinition as ConstructionModuleDefinition, this);
	}
}

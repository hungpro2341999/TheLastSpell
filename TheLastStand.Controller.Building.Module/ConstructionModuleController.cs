using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.Module;

public class ConstructionModuleController : BuildingModuleController
{
	public ConstructionModule ConstructionModule { get; }

	public ConstructionModuleController(BuildingController buildingControllerParent, ConstructionModuleDefinition constructionModuleDefinition)
		: base(buildingControllerParent, constructionModuleDefinition)
	{
		ConstructionModule = base.BuildingModule as ConstructionModule;
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new ConstructionModule(building, buildingModuleDefinition as ConstructionModuleDefinition, this);
	}
}

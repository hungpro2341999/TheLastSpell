using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.Module;

public class TrapConstructionModuleController : ConstructionModuleController
{
	#region Properties
	/// <summary>
	/// Model dữ liệu xây dựng cạm bẫy (TrapConstructionModule).
	/// </summary>
	public TrapConstructionModule TrapConstructionModule { get; }
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller xây dựng cạm bẫy.
	/// </summary>
	public TrapConstructionModuleController(BuildingController buildingControllerParent, ConstructionModuleDefinition constructionModuleDefinition)
		: base(buildingControllerParent, constructionModuleDefinition)
	{
		TrapConstructionModule = base.BuildingModule as TrapConstructionModule;
	}

	/// <summary>
	/// Khởi tạo Model TrapConstructionModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new TrapConstructionModule(building, buildingModuleDefinition as ConstructionModuleDefinition, this);
	}
	#endregion
}

using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.Module;

public class ConstructionModuleController : BuildingModuleController
{
	#region Properties
	/// <summary>
	/// Model dữ liệu xây dựng của công trình.
	/// </summary>
	public ConstructionModule ConstructionModule { get; }
	#endregion

	#region Initialization
	/// <summary>
	/// Khởi tạo Controller xử lý dữ liệu xây dựng của công trình.
	/// </summary>
	public ConstructionModuleController(BuildingController buildingControllerParent, ConstructionModuleDefinition constructionModuleDefinition)
		: base(buildingControllerParent, constructionModuleDefinition)
	{
		ConstructionModule = base.BuildingModule as ConstructionModule;
	}
	#endregion

	#region Model Creation
	/// <summary>
	/// Khởi tạo Model ConstructionModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new ConstructionModule(building, buildingModuleDefinition as ConstructionModuleDefinition, this);
	}
	#endregion
}

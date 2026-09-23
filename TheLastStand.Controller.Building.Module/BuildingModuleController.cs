using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.Module;

public abstract class BuildingModuleController
{
	#region Properties
	/// <summary>
	/// BuildingController cha quản lý module này.
	/// </summary>
	public BuildingController BuildingControllerParent { get; private set; }

	/// <summary>
	/// Model dữ liệu đại diện cho module của công trình.
	/// </summary>
	public BuildingModule BuildingModule { get; private set; }
	#endregion

	#region Initialization
	/// <summary>
	/// Khởi tạo Controller cho module của công trình và tự động tạo Model tương ứng.
	/// </summary>
	public BuildingModuleController(BuildingController buildingControllerParent, BuildingModuleDefinition buildingModuleDefinition)
	{
		BuildingControllerParent = buildingControllerParent;
		BuildingModule = CreateModel(buildingControllerParent.Building, buildingModuleDefinition);
	}
	#endregion

	#region Model Creation
	/// <summary>
	/// Phương thức trừu tượng khởi tạo Model module cụ thể cho công trình.
	/// </summary>
	protected abstract BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition);
	#endregion
}

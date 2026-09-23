using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.Module;

public class GateBlueprintModuleController : BlueprintModuleController
{
	#region Properties
	/// <summary>
	/// Model bản vẽ dành riêng cho Cổng thành (GateBlueprintModule).
	/// </summary>
	public GateBlueprintModule GateBlueprintModule { get; }
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller bản vẽ cổng thành.
	/// </summary>
	public GateBlueprintModuleController(BuildingController buildingControllerParent, BlueprintModuleDefinition blueprintModuleDefinition)
		: base(buildingControllerParent, blueprintModuleDefinition)
	{
		GateBlueprintModule = base.BuildingModule as GateBlueprintModule;
	}

	/// <summary>
	/// Khởi tạo Model GateBlueprintModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new GateBlueprintModule(building, buildingModuleDefinition as BlueprintModuleDefinition, this);
	}
	#endregion
}

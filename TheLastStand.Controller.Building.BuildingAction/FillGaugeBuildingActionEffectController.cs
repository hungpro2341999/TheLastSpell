using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller xử lý hiệu ứng "Nạp thanh tiến độ sản xuất" (Fill Gauge Effect) của công trình.
/// Khi kích hoạt, hành động này sẽ cộng trực tiếp một lượng điểm sản xuất (Production Units)
/// vào thanh tiến trình sản xuất (Production Gauge) của công trình để nhanh chóng đạt mốc sản sinh tài nguyên/vật phẩm.
/// </summary>
public class FillGaugeBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu nạp thanh tiến trình sản xuất.
	/// </summary>
	public FillGaugeBuildingActionEffect FillGaugeBuildingActionEffect => base.BuildingActionEffect as FillGaugeBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng nạp tiến độ công trình.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu hiệu ứng (số lượng điểm nạp, v.v.).</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public FillGaugeBuildingActionEffectController(FillGaugeBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new FillGaugeBuildingActionEffect(definition, this, productionBuilding);
	}

	#endregion

	#region Tile Targeting Validation

	/// <summary>
	/// Kiểm tra tính hợp lệ trên ô Tile chỉ định.
	/// Hiệu ứng nạp thanh tiến độ là tự thân cho công trình nên luôn trả về true.
	/// </summary>
	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	#endregion

	#region Action Execution Logic

	/// <summary>
	/// Thực thi nạp điểm sản xuất: Gọi ProductionModuleController của công trình để tăng lượng Production Units.
	/// </summary>
	public override void ExecuteActionEffect()
	{
		TPSingleton<BuildingManager>.Instance.Log($"Filling building gauge for {base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id} by {FillGaugeBuildingActionEffect.FillGaugeBuildingActionDefinition.Amount} units.");
		// Bơm trực tiếp số điểm sản xuất được chỉ định trong file Definition vào tiến trình công trình
		base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingController.ProductionModuleController.AddProductionUnits(FillGaugeBuildingActionEffect.FillGaugeBuildingActionDefinition.Amount);
	}

	#endregion
}


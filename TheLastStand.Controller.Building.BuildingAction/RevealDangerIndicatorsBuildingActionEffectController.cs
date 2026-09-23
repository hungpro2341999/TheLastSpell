using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller xử lý hiệu ứng "Hiển thị chỉ số nguy hiểm / Tỷ lệ địch chi tiết" (Reveal Danger Indicators Effect) từ công trình (ví dụ: Tháp trinh sát / Vọng gác).
/// Kích hoạt hiển thị mũi tên chi tiết cảnh báo đợt xuất hiện quái vật (chi tiết loại quái, số lượng, hướng tràn vào) trên bản đồ chiến trường.
/// </summary>
public class RevealDangerIndicatorsBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu hiệu ứng hiển thị chỉ báo nguy hiểm.
	/// </summary>
	public RevealDangerIndicatorsBuildingActionEffect RevealWaveEnemiesRatioBuildingActionEffect => base.BuildingActionEffect as RevealDangerIndicatorsBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng hiển thị chỉ báo nguy hiểm của công trình.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public RevealDangerIndicatorsBuildingActionEffectController(RevealDangerIndicatorsBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new RevealDangerIndicatorsBuildingActionEffect(definition, this, productionBuilding);
	}

	#endregion

	#region Tile Targeting Validation

	/// <summary>
	/// Kiểm tra tính hợp lệ trên ô Tile chỉ định. Luôn trả về true.
	/// </summary>
	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	#endregion

	#region Action Execution Logic

	/// <summary>
	/// Thực thi bật các mũi tên cảnh báo chi tiết về các đợt quái thông qua SpawnWaveManager.
	/// </summary>
	public override void ExecuteActionEffect()
	{
		// Bật hiển thị các mũi tên phân tích chi tiết về tương quan lực lượng và số lượng địch tại các hướng tấn công
		TPSingleton<SpawnWaveManager>.Instance.SetDetailedSpawnWaveArrows(state: true);
	}

	#endregion
}


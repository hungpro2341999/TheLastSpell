using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller xử lý hiệu ứng "Quay lại / Đổi đợt quái" (Reroll Wave Effect) từ công trình (ví dụ: Đài thiên văn, Tháp quan sát).
/// Cho phép người chơi tạo ngẫu nhiên lại thành phần, hướng tấn công hoặc phân bổ của đợt quái sắp tới trong đêm tiếp theo.
/// </summary>
public class RerollWaveBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu hiệu ứng reroll đợt quái.
	/// </summary>
	public RerollWaveBuildingActionEffect RerollWaveBuildingActionEffect => base.BuildingActionEffect as RerollWaveBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng reroll wave của công trình.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu hiệu ứng.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public RerollWaveBuildingActionEffectController(RerollWaveBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new RerollWaveBuildingActionEffect(definition, this, productionBuilding);
	}

	#endregion

	#region Tile Targeting Validation

	/// <summary>
	/// Kiểm tra tính hợp lệ trên ô Tile. Luôn trả về true.
	/// </summary>
	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	#endregion

	#region Action Execution Logic

	/// <summary>
	/// Thực thi việc reroll đợt quái:
	/// - Tạo lại thông tin đợt xuất hiện quái với cờ isReroll = true.
	/// - Cập nhật lại giao diện hiển thị đợt quái (SpawnWaveView.Refresh).
	/// </summary>
	public override void ExecuteActionEffect()
	{
		// Kích hoạt sinh lại ngẫu nhiên đợt xuất hiện quái vật cho đêm sắp tới
		SpawnWaveManager.GenerateSpawnWave(isReroll: true);
		// Cập nhật lại toàn bộ giao diện thông tin đợt quái trên bản đồ
		SpawnWaveManager.SpawnWaveView.Refresh();
	}

	#endregion
}


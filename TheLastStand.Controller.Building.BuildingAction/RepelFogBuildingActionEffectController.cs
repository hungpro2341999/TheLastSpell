using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Camera;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller xử lý hiệu ứng "Đẩy Lùi Sương Mù" (Repel Fog Effect) từ công trình (ví dụ: Tháp đèn, Cột ánh sáng, Công trình phòng thủ sương mù).
/// Tạm khóa thao tác camera của người chơi, lia máy quay đến khu vực sương mù gần nhất đang có đợt quái,
/// giảm mật độ/khoảng cách của sương mù (Fog Density) và cập nhật lại tầm nhìn cũng như HUD đợt quái.
/// </summary>
public class RepelFogBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu đẩy lùi sương mù.
	/// </summary>
	public RepelFogBuildingActionEffect RepelFogBuildingActionEffect => base.BuildingActionEffect as RepelFogBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng đẩy lùi sương mù.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu cấu hình (lượng sương mù bị đẩy lùi).</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	public RepelFogBuildingActionEffectController(RepelFogBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new RepelFogBuildingActionEffect(definition, this, productionBuilding);
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
	/// Thực thi hiệu ứng đẩy lùi sương mù:
	/// - Khóa thao tác rê chuột / zoom của người chơi.
	/// - Bắt đầu Coroutine chuyển camera đến vùng sương mù gần nhất có đợt quái trước khi đẩy lùi.
	/// </summary>
	public override void ExecuteActionEffect()
	{
		// Tạm thời vô hiệu hóa quyền điều khiển camera để người chơi tập trung vào cutscene đẩy lùi sương
		ACameraView.AllowUserPan = false;
		ACameraView.AllowUserZoom = false;
		TPSingleton<FogManager>.Instance.StartCoroutine(TPSingleton<FogManager>.Instance.MoveCameraToNearestFogWithWave(RepelFogAfterCameraMove, 1f));
	}

	/// <summary>
	/// Callback được gọi sau khi camera đã di chuyển xong tới vị trí rìa sương mù:
	/// - Giảm mật độ sương mù (FogController.DecreaseDensity).
	/// - Đánh dấu trạng thái HasBeenRepelled trong FogManager.
	/// - Làm mới hiển thị các mũi tên/chỉ số đợt quái (SpawnWaveView.Refresh).
	/// - Mở lại quyền điều khiển camera cho người chơi.
	/// </summary>
	private void RepelFogAfterCameraMove()
	{
		// Giảm mật độ sương mù theo lượng được định nghĩa trong file cấu hình
		FogController.DecreaseDensity(refreshFog: true, RepelFogBuildingActionEffect.RepelFogBuildingActionEffectDefinition.Amount);
		TPSingleton<FogManager>.Instance.Fog.HasBeenRepelled = true;
		
		// Làm mới giao diện đợt quái (do tầm nhìn sương mù đã bị đẩy lùi xa hơn)
		SpawnWaveManager.SpawnWaveView.Refresh();
		
		// Mở khóa lại tương tác camera cho người chơi
		ACameraView.AllowUserPan = true;
		ACameraView.AllowUserZoom = true;
	}

	#endregion
}


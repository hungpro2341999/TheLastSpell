using System.Collections;
using TPLib;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại tự phá hủy công trình (Self-Destruct / Sacrifice).
/// Hiển thị hiệu ứng kỹ năng, hoạt ảnh công trình sụp đổ (Die Animation), 
/// sau đó xóa bỏ công trình khỏi TileMap và khôi phục tàn tích cũ nếu có.
/// </summary>
public class DestroyBuildingController : BuildingPassiveEffectController
{
	#region Constructors

	/// <summary>
	/// Khởi tạo DestroyBuildingController với module nội tại và định nghĩa tự hủy.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình.</param>
	/// <param name="destroyBuildingDefinition">Định nghĩa cấu hình phá hủy công trình.</param>
	public DestroyBuildingController(PassivesModule buildingPassivesModule, DestroyBuildingDefinition destroyBuildingDefinition)
	{
		base.BuildingPassiveEffect = new DestroyBuilding(buildingPassivesModule, destroyBuildingDefinition, this);
	}

	#endregion

	#region Passive Effect Execution

	/// <summary>
	/// Thực thi hiệu ứng tự hủy: Khởi động Coroutine chờ chạy xong hiệu ứng thị giác và tiến hành dỡ bỏ công trình.
	/// </summary>
	public override void Apply()
	{
		TPSingleton<BuildingManager>.Instance.StartCoroutine(WaitDisplayEffectsAndDestroyBuilding());
	}

	/// <summary>
	/// Coroutine xử lý chuỗi hành động: Chờ hiệu ứng skill hiển thị -> Chạy hoạt ảnh sụp đổ -> 
	/// Xóa công trình tại ô Tile gốc -> Khôi phục công trình dự phòng nếu cần thiết.
	/// </summary>
	private IEnumerator WaitDisplayEffectsAndDestroyBuilding()
	{
		yield return base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingView.DisplaySkillEffects(0f);
		yield return base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingView.PlayDieAnimCoroutine();
		BuildingManager.DestroyBuilding(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OriginTile);
		TPSingleton<BuildingManager>.Instance.RestoreBuildingIfNeeded(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OriginTile);
	}

	#endregion
}

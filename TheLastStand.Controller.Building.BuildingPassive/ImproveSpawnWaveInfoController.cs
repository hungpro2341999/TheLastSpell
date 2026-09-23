using TPLib;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại cải thiện thông tin đợt tấn công của quái (Improve Spawn Wave Info).
/// Điển hình sử dụng cho công trình Nhà Tiên Tri (The Seer) giúp dự báo chi tiết hướng và thành phần quân địch.
/// </summary>
public class ImproveSpawnWaveInfoController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ dữ liệu hiệu ứng nâng cấp thông tin đợt tấn công.
	/// </summary>
	public ImproveSpawnWaveInfo ImproveSpawnWaveInfo => base.BuildingPassiveEffect as ImproveSpawnWaveInfo;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo ImproveSpawnWaveInfoController với module nội tại và định nghĩa cấu hình.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình.</param>
	/// <param name="buildingDefinition">Định nghĩa cấu hình hiệu ứng cải thiện thông tin wave.</param>
	public ImproveSpawnWaveInfoController(PassivesModule buildingPassivesModule, ImproveSpawnWaveInfoDefinition buildingDefinition)
	{
		base.BuildingPassiveEffect = new ImproveSpawnWaveInfo(buildingPassivesModule, buildingDefinition, this);
	}

	#endregion

	#region Passive Effect Lifecycle

	/// <summary>
	/// Áp dụng hiệu ứng: Thông báo cho SpawnWaveManager rằng công trình Nhà Tiên Tri đã được xây dựng và làm mới giao diện hiển thị wave.
	/// </summary>
	public override void Apply()
	{
		TPSingleton<SpawnWaveManager>.Instance.OnSeerBuiltOrDestroyed(built: true);
		SpawnWaveManager.CurrentSpawnWave?.SpawnWaveView.Refresh();
	}

	/// <summary>
	/// Tăng cường thông tin dự báo làn sóng khi công trình được nâng cấp.
	/// </summary>
	/// <param name="bonus">Giá trị độ chi tiết / thông tin cộng thêm.</param>
	public override void ImproveEffect(int bonus)
	{
		ImproveSpawnWaveInfo.UpgradedBonusValue += bonus;
		SpawnWaveManager.CurrentSpawnWave?.SpawnWaveView.Refresh();
	}

	/// <summary>
	/// Hủy áp dụng hiệu ứng: Thông báo cho SpawnWaveManager rằng công trình Nhà Tiên Tri đã bị phá hủy hoặc dỡ bỏ.
	/// </summary>
	public override void Unapply()
	{
		TPSingleton<SpawnWaveManager>.Instance.OnSeerBuiltOrDestroyed(built: false);
	}

	#endregion
}

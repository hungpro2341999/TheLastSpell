using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại biến đổi công trình (Transform Building).
/// Thay thế công trình hiện tại thành một công trình ngẫu nhiên khác từ danh sách định nghĩa,
/// đồng thời phát hiệu ứng khói biến đổi/phá hủy nếu được cấu hình.
/// </summary>
public class TransformBuildingController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ dữ liệu hiệu ứng biến đổi công trình.
	/// </summary>
	public TransformBuilding TransformBuilding => base.BuildingPassiveEffect as TransformBuilding;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo TransformBuildingController với module nội tại và định nghĩa biến đổi.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình.</param>
	/// <param name="transformBuildingDefinition">Định nghĩa cấu hình danh sách công trình mục tiêu và thuộc tính biến đổi.</param>
	public TransformBuildingController(PassivesModule buildingPassivesModule, TransformBuildingDefinition transformBuildingDefinition)
	{
		base.BuildingPassiveEffect = new TransformBuilding(buildingPassivesModule, transformBuildingDefinition, this);
	}

	#endregion

	#region Passive Effect Execution

	/// <summary>
	/// Thực thi biến đổi công trình: Lấy ID công trình ngẫu nhiên, gọi BuildingManager.ReplaceBuilding để thay thế 
	/// và kích hoạt hiệu ứng làn khói tan biến (Destruction Smoke) nếu có.
	/// </summary>
	public override void Apply()
	{
		TheLastStand.Model.Building.Building buildingParent = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent;

		// Thay thế công trình tại ô tile gốc bằng công trình mới được chọn ngẫu nhiên
		TheLastStand.Model.Building.Building building = BuildingManager.ReplaceBuilding(buildingParent.OriginTile, buildingParent, BuildingDatabase.BuildingDefinitions[TransformBuilding.TransformBuildingDefinition.GetRandomBuildingId()], ignoreBuilding: true, TransformBuilding.TransformBuildingDefinition.Instantaneous);

		// Chạy hiệu ứng làn khói nếu có cấu hình
		if (TransformBuilding.TransformBuildingDefinition.PlayDestructionSmoke)
		{
			building.BuildingView.StartCoroutine(building.BuildingView.PlayDestructionSmokeCoroutine());
		}
	}

	#endregion
}

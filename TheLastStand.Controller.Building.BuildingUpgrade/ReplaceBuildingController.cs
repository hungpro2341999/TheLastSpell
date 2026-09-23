using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller xử lý hiệu ứng nâng cấp thay thế công trình hiện tại bằng một công trình mới (Replace Building).
/// </summary>
public class ReplaceBuildingController : BuildingUpgradeEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu nâng cấp thay thế công trình.
	/// </summary>
	public ReplaceBuilding ReplaceBuilding => base.BuildingUpgradeEffect as ReplaceBuilding;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp thay thế công trình.
	/// </summary>
	public ReplaceBuildingController(ReplaceBuildingDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ReplaceBuilding(definition, this, buildingUpgrade);
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Thay thế công trình cũ tại ô bản đồ bằng công trình mới quy định trong ReplaceBuildingDefinition.
	/// </summary>
	public override void TriggerEffect(bool onLoad = false)
	{
		BuildingManager.ReplaceBuilding(base.BuildingUpgradeEffect.BuildingUpgrade.Building.OriginTile, base.BuildingUpgradeEffect.BuildingUpgrade.Building, BuildingDatabase.BuildingDefinitions[ReplaceBuilding.ReplaceBuildingDefinition.NewBuildingId]);
	}

	#endregion
}


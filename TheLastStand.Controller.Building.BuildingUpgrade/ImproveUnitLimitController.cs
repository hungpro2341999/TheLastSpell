using TPLib;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller xử lý hiệu ứng nâng cấp giới hạn số lượng tướng/đơn vị (Playable Units) có thể chiêu mộ.
/// </summary>
public class ImproveUnitLimitController : BuildingUpgradeEffectController
{
	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp giới hạn đơn vị.
	/// </summary>
	public ImproveUnitLimitController(ImproveUnitLimitDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveUnitLimit(definition, this, buildingUpgrade);
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Cộng thêm giới hạn tuyển dụng tướng trong PlayableUnitManager.
	/// </summary>
	public override void TriggerEffect(bool onLoad = false)
	{
		TPSingleton<PlayableUnitManager>.Instance.Recruitment.UnitLimitBonus++;
	}

	#endregion
}


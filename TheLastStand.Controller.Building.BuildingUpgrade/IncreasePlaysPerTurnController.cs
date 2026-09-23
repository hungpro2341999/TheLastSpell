using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller xử lý hiệu ứng nâng cấp tăng số lần hành động/lượt đánh (Plays/Goals) mỗi lượt của công trình phòng thủ/chiến đấu.
/// </summary>
public class IncreasePlaysPerTurnController : BuildingUpgradeEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu nâng cấp tăng lượt hành động.
	/// </summary>
	public IncreasePlaysPerTurn IncreasePlaysPerTurn => base.BuildingUpgradeEffect as IncreasePlaysPerTurn;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp tăng số lượt hành động mỗi lượt.
	/// </summary>
	public IncreasePlaysPerTurnController(IncreasePlaysPerTurnDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new IncreasePlaysPerTurn(definition, this, buildingUpgrade);
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Kích hoạt tăng số lượng mục tiêu/hành động cần tính toán (NumberOfGoalsToCompute) trong BattleModule của công trình.
	/// </summary>
	public override void TriggerEffect(bool onLoad = false)
	{
		TheLastStand.Model.Building.Building building = base.BuildingUpgradeEffect.BuildingUpgrade.Building;
		if (building.BattleModule != null)
		{
			building.BattleModule.NumberOfGoalsToCompute += IncreasePlaysPerTurn.IncreasePlaysPerTurnDefinition.Value;
		}
	}

	#endregion
}


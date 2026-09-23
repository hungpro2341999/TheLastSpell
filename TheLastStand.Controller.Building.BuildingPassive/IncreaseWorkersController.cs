using TPLib;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại tăng số lượng công nhân tối đa (Increase Workers).
/// Cung cấp thêm lượt công nhân hoạt động trong ngày cho người chơi và hoàn tác lại khi công trình bị dỡ bỏ.
/// </summary>
public class IncreaseWorkersController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ dữ liệu hiệu ứng tăng công nhân.
	/// </summary>
	public IncreaseWorkers IncreaseWorkers => base.BuildingPassiveEffect as IncreaseWorkers;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo IncreaseWorkersController với module nội tại và định nghĩa số lượng công nhân.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình.</param>
	/// <param name="increaseWorkersDefinition">Định nghĩa cấu hình số lượng công nhân tăng thêm.</param>
	public IncreaseWorkersController(PassivesModule buildingPassivesModule, IncreaseWorkersDefinition increaseWorkersDefinition)
	{
		base.BuildingPassiveEffect = new IncreaseWorkers(buildingPassivesModule, increaseWorkersDefinition, this);
	}

	#endregion

	#region Passive Effect Lifecycle & Upgrades

	/// <summary>
	/// Áp dụng hiệu ứng: Tăng giới hạn số lượng công nhân tối đa vào ResourceManager.
	/// </summary>
	public override void Apply()
	{
		TPSingleton<ResourceManager>.Instance.IncreaseMaxWorkers(IncreaseWorkers.IncreaseWorkersDefinition.Value.EvalToInt() + IncreaseWorkers.UpgradedBonusValue);
	}

	/// <summary>
	/// Tăng thêm công nhân khi công trình được nâng cấp trong trận đấu.
	/// </summary>
	/// <param name="bonus">Số lượng công nhân cộng thêm.</param>
	public override void ImproveEffect(int bonus)
	{
		IncreaseWorkers.UpgradedBonusValue += bonus;
		TPSingleton<ResourceManager>.Instance.IncreaseMaxWorkers(bonus);
	}

	/// <summary>
	/// Hoàn tác hiệu ứng: Giảm bớt số lượng công nhân tối đa khi công trình bị phá hủy hoặc dỡ bỏ.
	/// </summary>
	public override void Unapply()
	{
		TPSingleton<ResourceManager>.Instance.DecreaseMaxWorkers(IncreaseWorkers.IncreaseWorkersDefinition.Value.EvalToInt() + IncreaseWorkers.UpgradedBonusValue);
	}

	#endregion
}

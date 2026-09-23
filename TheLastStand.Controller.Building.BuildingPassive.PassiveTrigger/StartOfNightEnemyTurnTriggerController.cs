using TPLib;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Manager;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt khi bắt đầu lượt đi của kẻ địch vào ban đêm (Start of Night Enemy Turn).
/// Kiểm tra xem giờ đêm hiện tại có phải là giờ đầu tiên (CurrentNightHour == 1) hay không.
/// </summary>
public class StartOfNightEnemyTurnTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt khi bắt đầu lượt quái đêm.
	/// </summary>
	public StartOfNightEnemyTurnTrigger StartOfNightEnemyTurnTrigger => base.PassiveTrigger as StartOfNightEnemyTurnTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo StartOfNightEnemyTurnTriggerController với định nghĩa cấu hình tương ứng.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public StartOfNightEnemyTurnTriggerController(StartOfNightEnemyTurnTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new StartOfNightEnemyTurnTrigger(definition, this);
	}

	#endregion

	#region Trigger Evaluation

	/// <summary>
	/// Kiểm tra điều kiện kích hoạt: Chỉ kích hoạt tại giờ đêm đầu tiên (CurrentNightHour == 1).
	/// </summary>
	/// <param name="OnLoad">Cờ đánh dấu trạng thái nạp dữ liệu lưu.</param>
	/// <returns>True nếu đang ở giờ đêm đầu tiên; ngược lại False.</returns>
	public override bool UpdateAndCheckTrigger(bool OnLoad)
	{
		return TPSingleton<GameManager>.Instance.Game.CurrentNightHour == 1;
	}

	#endregion
}

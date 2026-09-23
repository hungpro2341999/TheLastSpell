using TPLib;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Manager;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt khi bắt đầu lượt đi của phe người chơi vào ban đêm (Start of Night Playable Turn).
/// Kiểm tra xem giờ đêm hiện tại có phải là giờ đầu tiên (CurrentNightHour == 1) hay không.
/// </summary>
public class StartOfNightPlayableTurnTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt khi bắt đầu lượt phe người chơi ban đêm.
	/// </summary>
	public StartOfNightPlayableTurnTrigger StartOfNightPlayableTurnTrigger => base.PassiveTrigger as StartOfNightPlayableTurnTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo StartOfNightPlayableTurnTriggerController với định nghĩa cấu hình tương ứng.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public StartOfNightPlayableTurnTriggerController(StartOfNightPlayableTurnTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new StartOfNightPlayableTurnTrigger(definition, this);
	}

	#endregion

	#region Trigger Evaluation

	/// <summary>
	/// Kiểm tra điều kiện kích hoạt: Chỉ kích hoạt tại giờ đêm đầu tiên của phe người chơi (CurrentNightHour == 1).
	/// </summary>
	/// <param name="OnLoad">Cờ đánh dấu trạng thái nạp dữ liệu lưu.</param>
	/// <returns>True nếu đang ở giờ đêm đầu tiên; ngược lại False.</returns>
	public override bool UpdateAndCheckTrigger(bool OnLoad)
	{
		return TPSingleton<GameManager>.Instance.Game.CurrentNightHour == 1;
	}

	#endregion
}

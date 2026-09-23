using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt sau mỗi X lượt đi trong đêm (After X Night Turns).
/// Tích lũy bộ đếm số lượt ban đêm và kích hoạt hiệu ứng khi đạt đủ số lượt quy định.
/// </summary>
public class AfterXNightTurnsTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt sau X lượt đêm.
	/// </summary>
	public AfterXNightTurnsTrigger AfterXNightTurnsTrigger => base.PassiveTrigger as AfterXNightTurnsTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo AfterXNightTurnsTriggerController từ dữ liệu lưu trữ (Save Deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa của trigger từ tệp lưu.</param>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public AfterXNightTurnsTriggerController(SerializedAfterXNightTurnsTrigger container, AfterXNightTurnsTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXNightTurnsTrigger(container, definition, this);
	}

	/// <summary>
	/// Khởi tạo mới AfterXNightTurnsTriggerController trong trận đấu.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public AfterXNightTurnsTriggerController(AfterXNightTurnsTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXNightTurnsTrigger(definition, this);
	}

	#endregion

	#region Trigger Evaluation

	/// <summary>
	/// Tăng bộ đếm số lượt đêm trôi qua và so khớp với ngưỡng NumberOfNightTurns trong cấu hình.
	/// </summary>
	/// <param name="onLoad">Cờ đánh dấu đang trong quá trình nạp lại game.</param>
	/// <returns>True nếu bộ đếm lượt đêm đạt đúng ngưỡng cấu hình; ngược lại False.</returns>
	public override bool UpdateAndCheckTrigger(bool onLoad)
	{
		AfterXNightTurnsTrigger.NightTurnsBuffer++;
		return AfterXNightTurnsTrigger.NightTurnsBuffer == AfterXNightTurnsTrigger.AfterXNightTurnsTriggerDefinition.NumberOfNightTurns;
	}

	#endregion
}

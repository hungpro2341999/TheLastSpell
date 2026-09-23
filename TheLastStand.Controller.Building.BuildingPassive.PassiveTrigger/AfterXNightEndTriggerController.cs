using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt sau mỗi X đêm phòng thủ thành công (After X Night Ends).
/// Tích lũy bộ đếm số đêm kết thúc và kích hoạt hiệu ứng khi đạt đủ số lượng định nghĩa.
/// </summary>
public class AfterXNightEndTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt sau X đêm kết thúc.
	/// </summary>
	public AfterXNightEndTrigger AfterXNightEndTrigger => base.PassiveTrigger as AfterXNightEndTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo AfterXNightEndTriggerController từ dữ liệu lưu trữ (Save Deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa của trigger từ tệp lưu.</param>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public AfterXNightEndTriggerController(SerializedAfterXNightEndTrigger container, AfterXNightEndTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXNightEndTrigger(container, definition, this);
	}

	/// <summary>
	/// Khởi tạo mới AfterXNightEndTriggerController trong trận đấu.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public AfterXNightEndTriggerController(AfterXNightEndTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXNightEndTrigger(definition, this);
	}

	#endregion

	#region Trigger Evaluation

	/// <summary>
	/// Tăng bộ đếm số đêm hoàn tất và so khớp với ngưỡng NumberOfNightEnd trong cấu hình.
	/// </summary>
	/// <param name="onLoad">Cờ đánh dấu đang trong quá trình nạp lại game.</param>
	/// <returns>True nếu bộ đếm đạt đúng ngưỡng quy định; ngược lại False.</returns>
	public override bool UpdateAndCheckTrigger(bool onLoad)
	{
		AfterXNightEndTrigger.NightEndBuffer++;
		return AfterXNightEndTrigger.NightEndBuffer == AfterXNightEndTrigger.AfterXNightEndTriggerDefinition.NumberOfNightEnd;
	}

	#endregion
}

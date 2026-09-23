using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt sau mỗi X pha sản xuất ban ngày (After X Production Phases).
/// Tích lũy bộ đếm số pha sản xuất trôi qua và kích hoạt hiệu ứng khi đạt đủ số lượng định nghĩa.
/// </summary>
public class AfterXProductionPhasesTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt sau X pha sản xuất.
	/// </summary>
	public AfterXProductionPhasesTrigger AfterXProductionPhasesTrigger => base.PassiveTrigger as AfterXProductionPhasesTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo AfterXProductionPhasesTriggerController từ dữ liệu lưu trữ (Save Deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa của trigger từ tệp lưu.</param>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public AfterXProductionPhasesTriggerController(SerializedAfterXProductionPhasesTrigger container, AfterXProductionPhasesTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXProductionPhasesTrigger(container, definition, this);
	}

	/// <summary>
	/// Khởi tạo mới AfterXProductionPhasesTriggerController trong trận đấu.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public AfterXProductionPhasesTriggerController(AfterXProductionPhasesTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new AfterXProductionPhasesTrigger(definition, this);
	}

	#endregion

	#region Trigger Evaluation

	/// <summary>
	/// Tăng bộ đếm số pha sản xuất và so khớp với ngưỡng NumberOfProductionPhases trong cấu hình.
	/// </summary>
	/// <param name="onLoad">Cờ đánh dấu đang trong quá trình nạp lại game.</param>
	/// <returns>True nếu bộ đếm pha sản xuất đạt đúng ngưỡng cấu hình; ngược lại False.</returns>
	public override bool UpdateAndCheckTrigger(bool onLoad)
	{
		AfterXProductionPhasesTrigger.ProductionPhasesBuffer++;
		return AfterXProductionPhasesTrigger.ProductionPhasesBuffer == AfterXProductionPhasesTrigger.AfterXProductionPhasesTriggerDefinition.NumberOfProductionPhases;
	}

	#endregion
}

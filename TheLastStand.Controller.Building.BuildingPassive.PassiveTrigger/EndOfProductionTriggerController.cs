using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt vào thời điểm kết thúc pha sản xuất ban ngày (End of Production Phase).
/// </summary>
public class EndOfProductionTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt khi kết thúc pha sản xuất.
	/// </summary>
	public EndOfProductionTrigger EndOfProductionTrigger => base.PassiveTrigger as EndOfProductionTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo EndOfProductionTriggerController với định nghĩa cấu hình tương ứng.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public EndOfProductionTriggerController(EndOfProductionTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new EndOfProductionTrigger(definition, this);
	}

	#endregion
}

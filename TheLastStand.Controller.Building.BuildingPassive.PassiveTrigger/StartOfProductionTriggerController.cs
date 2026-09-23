using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt vào thời điểm bắt đầu pha sản xuất ban ngày (Start of Production Phase).
/// Thường dùng cho các hiệu ứng trao tài nguyên, phục hồi công nhân hoặc làm mới thanh năng lượng mỗi buổi sáng.
/// </summary>
public class StartOfProductionTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt khi bắt đầu pha sản xuất.
	/// </summary>
	public StartOfProductionTrigger StartOfProductionTrigger => base.PassiveTrigger as StartOfProductionTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo StartOfProductionTriggerController với định nghĩa cấu hình tương ứng.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public StartOfProductionTriggerController(StartOfProductionTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new StartOfProductionTrigger(definition, this);
	}

	#endregion
}

using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt khi công trình bị phá hủy / sụp đổ hoàn toàn (On Death).
/// </summary>
public class OnDeathTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt khi công trình bị phá hủy.
	/// </summary>
	public OnDeathTrigger OnDeathTrigger => base.PassiveTrigger as OnDeathTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo OnDeathTriggerController với định nghĩa cấu hình tương ứng.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public OnDeathTriggerController(OnDeathTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new OnDeathTrigger(definition, this);
	}

	#endregion
}

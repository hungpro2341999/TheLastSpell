using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt khi công trình bị dập tắt (On Extinguish - ví dụ ngọn lửa, đuốc chòi, brazier).
/// Kế thừa cơ chế hoạt động tương tự như trigger khi công trình bị phá hủy (OnDeath).
/// </summary>
public class OnExtinguishTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt khi công trình bị dập tắt (kế thừa kiểu OnDeathTrigger).
	/// </summary>
	public OnDeathTrigger OnDeathTrigger => base.PassiveTrigger as OnDeathTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo OnExtinguishTriggerController với định nghĩa cấu hình dập tắt công trình.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger dập tắt.</param>
	public OnExtinguishTriggerController(OnExtinguishTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new OnExtinguishTrigger(definition, this);
	}

	#endregion
}

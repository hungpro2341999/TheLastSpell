using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt ngay tại thời điểm công trình vừa được thi công / xây dựng xong (On Construction).
/// </summary>
public class OnConstructionTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt khi xây dựng xong công trình.
	/// </summary>
	public OnConstructionTrigger OnConstructionTrigger => base.PassiveTrigger as OnConstructionTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo OnConstructionTriggerController với định nghĩa cấu hình tương ứng.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public OnConstructionTriggerController(OnConstructionTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new OnConstructionTrigger(definition, this);
	}

	#endregion
}

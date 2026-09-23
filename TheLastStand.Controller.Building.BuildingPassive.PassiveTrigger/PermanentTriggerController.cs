using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Bộ điều khiển trigger kích hoạt hiệu ứng dạng vĩnh viễn / duy trì liên tục (Permanent Trigger).
/// Kiểm tra điều kiện nạp lại game (OnLoad) để xác định có cần tái áp dụng hiệu ứng khi tải lại dữ liệu lưu hay không.
/// </summary>
public class PermanentTriggerController : PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của trigger kích hoạt dạng vĩnh viễn.
	/// </summary>
	public PermanentTrigger PermanentTrigger => base.PassiveTrigger as PermanentTrigger;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo PermanentTriggerController với định nghĩa cấu hình tương ứng.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger vĩnh viễn.</param>
	public PermanentTriggerController(PermanentTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new PermanentTrigger(definition, this);
	}

	#endregion

	#region Trigger Evaluation

	/// <summary>
	/// Kiểm tra điều kiện kích hoạt: Nếu không phải lúc nạp game (OnLoad), hoặc cấu hình cho phép TriggerOnLoad thì áp dụng.
	/// </summary>
	/// <param name="OnLoad">Cờ đánh dấu có đang trong quá trình nạp lại game hay không.</param>
	/// <returns>True nếu hiệu ứng vĩnh viễn được phép kích hoạt/tái áp dụng; ngược lại False.</returns>
	public override bool UpdateAndCheckTrigger(bool OnLoad)
	{
		if (!OnLoad || PermanentTrigger.PermanentTriggerDeginition.TriggerOnLoad)
		{
			return base.UpdateAndCheckTrigger(OnLoad);
		}
		return false;
	}

	#endregion
}

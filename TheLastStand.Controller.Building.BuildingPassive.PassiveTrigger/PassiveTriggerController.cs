using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

/// <summary>
/// Lớp cơ sở trừu tượng (Abstract Base Class) cho các bộ điều khiển điều kiện kích hoạt hiệu ứng nội tại (Passive Trigger).
/// Định nghĩa cơ chế kiểm tra và cập nhật bộ đếm/trạng thái điều kiện trước khi hiệu ứng nội tại được áp dụng.
/// </summary>
public abstract class PassiveTriggerController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ dữ liệu của điều kiện kích hoạt nội tại.
	/// </summary>
	public TheLastStand.Model.Building.BuildingPassive.PassiveTrigger.PassiveTrigger PassiveTrigger { get; protected set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo PassiveTriggerController với định nghĩa cấu hình trigger cơ sở.
	/// </summary>
	/// <param name="definition">Định nghĩa cấu hình của trigger.</param>
	public PassiveTriggerController(PassiveTriggerDefinition definition)
	{
	}

	#endregion

	#region Trigger Evaluation

	/// <summary>
	/// Cập nhật bộ đếm (nếu có) và kiểm tra xem điều kiện kích hoạt có được thỏa mãn hay không.
	/// </summary>
	/// <param name="OnLoad">Cờ đánh dấu sự kiện có đang được gọi trong quá trình nạp lại game hay không.</param>
	/// <returns>True nếu điều kiện kích hoạt hợp lệ; ngược lại trả về False.</returns>
	public virtual bool UpdateAndCheckTrigger(bool OnLoad)
	{
		return true;
	}

	#endregion
}

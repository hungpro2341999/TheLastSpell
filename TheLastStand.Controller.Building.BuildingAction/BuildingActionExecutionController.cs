using TheLastStand.Model.Building.BuildingAction;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller điều phối phiên thực thi hành động công trình (Building Action Execution).
/// Đóng vai trò khởi tạo và liên kết vòng đời của model BuildingActionExecution.
/// </summary>
public class BuildingActionExecutionController
{
	#region Properties & Model

	/// <summary>
	/// Model lưu trữ ngữ cảnh và trạng thái của lần thực thi hành động.
	/// </summary>
	public BuildingActionExecution BuildingActionExecution { get; private set; }

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller và thiết lập đối tượng BuildingActionExecution liên kết.
	/// </summary>
	public BuildingActionExecutionController()
	{
		BuildingActionExecution = new BuildingActionExecution(this);
	}

	#endregion
}


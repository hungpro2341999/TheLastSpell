using System.Collections.Generic;
using TheLastStand.Model;
using TheLastStand.Model.Skill;

namespace TheLastStand.Controller;

/// <summary>
/// Interface quy định hành vi AI (Behavior Tree / Goal-oriented Action) của kẻ địch hoặc thực thể tự động.
/// </summary>
public interface IBehaviorController
{
	#region Goal Computing & Management

	/// <summary>
	/// Xóa bỏ mục tiêu hành vi hiện tại của AI.
	/// </summary>
	void ClearCurrentGoal();

	/// <summary>
	/// Tính toán các mục tiêu (Goals) tiếp theo mà AI muốn thực hiện dựa trên trạng thái chiến trường.
	/// </summary>
	/// <param name="alreadyTargetedTiles">Danh sách các ô đã bị kẻ địch khác nhắm tới để tránh trùng lặp.</param>
	void ComputeCurrentGoals(Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedTiles = null);

	#endregion

	#region Goal Execution

	/// <summary>
	/// Thực thi toàn bộ danh sách mục tiêu đã được tính toán trong lượt này.
	/// </summary>
	void ExecuteAllGoals();

	/// <summary>
	/// Thực thi một mục tiêu hành vi cụ thể (di chuyển, dùng kỹ năng, tấn công...).
	/// </summary>
	/// <param name="computedGoal">Mục tiêu đã được tính toán.</param>
	/// <returns>True nếu thực thi thành công, ngược lại False.</returns>
	bool ExecuteGoal(ComputedGoal computedGoal);

	#endregion

	#region Death Rattle Logic

	/// <summary>
	/// Chuẩn bị kích hoạt hiệu ứng trăn trối (Death Rattle) khi đối tượng sắp chết.
	/// </summary>
	void PrepareForDeathRattle();

	/// <summary>
	/// Thực thi kỹ năng hoặc hiệu ứng trăn trối khi đối tượng bị tiêu diệt.
	/// </summary>
	void ExecuteDeathRattle();

	#endregion
}

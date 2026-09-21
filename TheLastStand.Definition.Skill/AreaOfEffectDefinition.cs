using System.Collections.Generic;
using UnityEngine;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Định nghĩa vùng ảnh hưởng (Area of Effect - AoE) của một kỹ năng.
/// Mô tả hình dạng, tâm điểm và số lượng ô mục tiêu mà kỹ năng tác động khi được sử dụng.
/// Pattern là lưới ký tự 2D: 'X' = ô bị ảnh hưởng, 'e' = ô hiệu ứng bao quanh (surrounding),
/// 'M' = ô di chuyển (maneuver), '_' = ô trống.
/// </summary>
public class AreaOfEffectDefinition
{
	#region Properties

	/// <summary>
	/// Tọa độ tâm điểm (Origin) của vùng AoE trong lưới Pattern.
	/// Đây là ô mà người chơi chọn làm mục tiêu khi sử dụng kỹ năng.
	/// </summary>
	public Vector2Int Origin { get; set; }

	/// <summary>
	/// Ma trận 2D ký tự mô tả hình dạng vùng ảnh hưởng.
	/// Mỗi List&lt;char&gt; là một hàng, mỗi char biểu thị loại ô:
	/// 'X' = ô chịu hiệu ứng chính, 'e' = ô hiệu ứng bao quanh,
	/// 'M' = ô maneuver (di chuyển sau khi tấn công), '_' = ô trống.
	/// </summary>
	public List<List<char>> Pattern { get; set; }

	/// <summary>
	/// True nếu kỹ năng chỉ nhắm vào đúng 1 ô (AffectedTilesCount == 1).
	/// Dùng để tối ưu hiển thị và logic xử lý cho kỹ năng đơn mục tiêu.
	/// </summary>
	public bool IsSingleTarget { get; set; }

	#endregion Properties
}

using TheLastStand.View.Skill.SkillAction;

namespace TheLastStand.Controller.Skill.SkillAction;

/// <summary>
/// Giao diện (Interface) cho các bộ điều khiển kỹ năng có tương tác hoặc hiển thị hiệu ứng lên mục tiêu.
/// </summary>
public interface IEffectTargetSkillActionController
{
	/// <summary>
	/// Thêm một hiệu ứng hiển thị hình ảnh vào danh sách chờ.
	/// </summary>
	void AddEffectDisplay(IDisplayableEffect displayableEffect);

	/// <summary>
	/// Kích hoạt trình diễn các hiệu ứng với độ trễ tùy chọn.
	/// </summary>
	void DisplayEffects(float delay = 0f);

	/// <summary>
	/// Lấy tổng số lượng hiệu ứng đang chờ hiển thị.
	/// </summary>
	int GetEffectsCount();
}

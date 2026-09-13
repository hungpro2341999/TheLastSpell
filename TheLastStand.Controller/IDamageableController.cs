using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Perk;

namespace TheLastStand.Controller;

/// <summary>
/// Interface cho các Controller của mọi thực thể có thể nhận sát thương (Hero, Quái vật, Công trình, Chướng ngại vật...).
/// </summary>
public interface IDamageableController : IEffectTargetSkillActionController
{
	#region Properties

	/// <summary>
	/// Tham chiếu tới Model của thực thể chịu sát thương.
	/// </summary>
	IDamageable Damageable { get; }

	#endregion

	#region Health & Armor Modification

	/// <summary>
	/// Hồi máu cho thực thể.
	/// </summary>
	/// <param name="amount">Lượng máu hồi phục.</param>
	/// <param name="refreshHud">Có cập nhật thanh máu trên HUD ngay lập tức không.</param>
	/// <returns>Lượng máu thực tế hồi được.</returns>
	float GainHealth(float amount, bool refreshHud = true);

	/// <summary>
	/// Trừ điểm giáp (Armor) của thực thể khi bị tấn công hoặc chịu sát thương.
	/// </summary>
	/// <param name="amount">Lượng giáp bị trừ.</param>
	/// <param name="attacker">Thực thể tấn công gây mất giáp.</param>
	/// <param name="refreshHud">Có cập nhật thanh giáp trên HUD ngay lập tức không.</param>
	void LoseArmor(float amount, ISkillCaster attacker = null, bool refreshHud = true);

	/// <summary>
	/// Trừ điểm máu (Health) của thực thể khi nhận sát thương trực tiếp hoặc xuyên giáp.
	/// </summary>
	/// <param name="amount">Lượng máu bị trừ.</param>
	/// <param name="attacker">Thực thể tấn công gây sát thương.</param>
	/// <param name="refreshHud">Có cập nhật thanh máu trên HUD ngay lập tức không.</param>
	/// <param name="skillName">Tên kỹ năng gây ra sát thương.</param>
	void LoseHealth(float amount, ISkillCaster attacker = null, bool refreshHud = true, string skillName = null);

	#endregion

	#region Combat Callbacks

	/// <summary>
	/// Kích hoạt khi thực thể vừa bị trúng đòn tấn công.
	/// </summary>
	/// <param name="attacker">Thực thể tấn công.</param>
	void OnHit(ISkillCaster attacker);

	/// <summary>
	/// Kích hoạt khi dữ liệu đòn tấn công vừa được tính toán xong (trước khi trừ máu) để xử lý các hiệu ứng Perk/Phòng thủ.
	/// </summary>
	/// <param name="perkDataContainer">Dữ liệu bổng lộc/hiệu ứng đính kèm đòn đánh.</param>
	void OnAttackDataComputed(PerkDataContainer perkDataContainer);

	#endregion
}

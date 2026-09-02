using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Perk;

namespace TheLastStand.Controller;

public interface IDamageableController : IEffectTargetSkillActionController
{
	IDamageable Damageable { get; }

	float GainHealth(float amount, bool refreshHud = true);

	void LoseArmor(float amount, ISkillCaster attacker = null, bool refreshHud = true);

	void LoseHealth(float amount, ISkillCaster attacker = null, bool refreshHud = true, string skillName = null);

	void OnHit(ISkillCaster attacker);

	void OnAttackDataComputed(PerkDataContainer perkDataContainer);
}

using TheLastStand.Model;
using TheLastStand.View.Skill.SkillAction;
using UnityEngine;

namespace TheLastStand.View;

public interface IDamageableView
{
	AttackFeedback AttackFeedback { get; }

	IDamageable Damageable { get; }

	IDamageableHUD DamageableHUD { get; }

	GainArmorFeedback GainArmorFeedback { get; }

	HealFeedback HealFeedback { get; }

	Transform HudFollowTarget { get; }

	GameObject GameObject { get; }

	void PlayDieAnim();

	void PlayTakeDamageAnim();
}

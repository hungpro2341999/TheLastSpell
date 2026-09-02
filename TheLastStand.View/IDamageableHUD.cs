using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using UnityEngine;

namespace TheLastStand.View;

public interface IDamageableHUD
{
	bool HealthDisplayed { get; set; }

	bool Highlight { get; set; }

	Transform Transform { get; }

	void DisplayHealthIfNeeded();

	void PlayDamageAnim(AttackSkillActionExecutionTileData attackData);

	void PlayHealthGainAnim(float healthGain, float healthAfterGain);

	void PlayArmorGainAnim(float armorGain, float armorAfterGain);
}

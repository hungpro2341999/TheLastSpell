using TheLastStand.View.Skill.SkillAction;

namespace TheLastStand.Controller.Skill.SkillAction;

public interface IEffectTargetSkillActionController
{
	void AddEffectDisplay(IDisplayableEffect displayableEffect);

	void DisplayEffects(float delay = 0f);

	int GetEffectsCount();
}

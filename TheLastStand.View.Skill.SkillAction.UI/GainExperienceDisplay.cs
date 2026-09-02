using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class GainExperienceDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/GainExperienceDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI expGainLbl;

	public override void Init(int expGain)
	{
		base.Init();
		expGainLbl.text = $"+{expGain} XP";
	}
}

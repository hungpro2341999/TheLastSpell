using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class GainOverallUsesDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/GainOverallUsesDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI overallUsesGainValue;

	public override void Init(int usesGain)
	{
		base.Init();
		overallUsesGainValue.text = string.Format("{0}{1} <sprite name=UsePerNight>", (usesGain > 0) ? "+" : "", usesGain);
	}
}

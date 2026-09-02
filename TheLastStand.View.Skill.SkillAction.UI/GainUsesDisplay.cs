using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class GainUsesDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/GainUsesDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI usesGainValue;

	public override void Init(int usesGain)
	{
		base.Init();
		usesGainValue.text = string.Format("{0}{1} <size=16><sprite name=TrapUse></size>", (usesGain > 0) ? "+" : "", usesGain);
	}
}

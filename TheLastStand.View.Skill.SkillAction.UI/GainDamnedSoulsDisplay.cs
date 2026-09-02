using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class GainDamnedSoulsDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/GainDamnedSoulsDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI damnedSoulsGainValue;

	public override void Init(int damnedSoulsGain)
	{
		base.Init();
		damnedSoulsGainValue.text = string.Format("{0}{1} <sprite name=\"DamnedSouls\">", (damnedSoulsGain > 0) ? "+" : "", damnedSoulsGain);
	}
}

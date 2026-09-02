using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class GainDamageDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/GainDamageDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI damageGainDisplay;

	public override void Init(int damageGain)
	{
		base.Init();
		damageGainDisplay.text = ((damageGain > 0) ? "+" : string.Empty);
		damageGainDisplay.text += $"{damageGain}%";
	}
}

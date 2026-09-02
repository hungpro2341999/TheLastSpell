using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class PerkActivationDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string PerkEffectDisplayName = "PerkActivationDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI label;

	public void Init(string perkName, bool isActivated)
	{
		base.Init();
		label.text = (isActivated ? "<style=Good>+" : "<style=Bad>-") + " " + perkName + "</style>";
	}
}

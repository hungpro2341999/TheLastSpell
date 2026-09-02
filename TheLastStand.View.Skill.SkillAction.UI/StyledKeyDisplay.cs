using TMPro;
using TPLib.Localization;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class StyledKeyDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string StyledKeyDisplayPrefabPath = "Prefab/Displayable Effect/UI Effect Displays/StyledKeyDisplay";

		public const string LoseChargeStyle = "Discharge";

		public const string LoseChargeKey = "StatusName_Discharge";
	}

	[SerializeField]
	private TextMeshProUGUI valueLabel;

	public void Init(string style, string key)
	{
		base.Init();
		valueLabel.text = "<style=" + style + ">" + Localizer.Get(key) + "</style>";
	}

	public void Init(Status.E_StatusType statusType)
	{
		string text = statusType.ToString();
		Init(text, "StatusName_" + text);
	}
}

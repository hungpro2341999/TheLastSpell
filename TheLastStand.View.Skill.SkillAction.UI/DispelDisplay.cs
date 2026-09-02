using TMPro;
using TPLib.Localization;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class DispelDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DispelName = "Dispel";

		public const string DispelSkillEffectLoc = "SkillEffectName_Dispel";

		public const string RemoveStatusSkillEffectLoc = "SkillEffectName_RemoveStatus";

		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/DispelDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI dispelText;

	public void Init(Status.E_StatusType status)
	{
		base.Init();
		string key = (Status.E_StatusType.RemovablePositive.HasFlag(status) ? "SkillEffectName_RemoveStatus" : "SkillEffectName_Dispel");
		dispelText.text = "<style=Dispel>" + Localizer.Get(key) + " <sprite name=" + RemoveStatusDefinition.GetRemovedStatusIconName(status) + "></style>";
	}
}

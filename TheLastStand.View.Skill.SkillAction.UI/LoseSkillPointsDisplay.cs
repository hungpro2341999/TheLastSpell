using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class LoseSkillPointsDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/LoseSkillPointsDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI lossText;

	public override void Init(int loss)
	{
		base.Init(loss);
		lossText.text = $"-{loss} use";
	}
}

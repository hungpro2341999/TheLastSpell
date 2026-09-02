using TMPro;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class BuffDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string BuffDisplayPrefab = "Prefab/Displayable Effect/UI Effect Displays/BuffDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI buffText;

	public void Init(UnitStatDefinition.E_Stat stat, int value)
	{
		base.Init(value);
		buffText.text = string.Format("<style=Buff></style> <style=GoodNb>+{0}{1}</style> <style={2}></style>", value, stat.ShownAsPercentage() ? "%" : string.Empty, stat);
	}
}

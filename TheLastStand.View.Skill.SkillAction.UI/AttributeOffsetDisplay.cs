using TMPro;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class AttributeOffsetDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string AttributeOffsetDisplayPrefab = "Prefab/Displayable Effect/UI Effect Displays/AttributeOffsetDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI attributeDisplay;

	public void Init(UnitStatDefinition.E_Stat stat, int attributeValue)
	{
		base.Init();
		string valueStylized = stat.GetValueStylized(attributeValue);
		string stylizedCustomContent = stat.GetStylizedCustomContent(string.Empty);
		attributeDisplay.text = valueStylized + " " + stylizedCustomContent;
	}
}

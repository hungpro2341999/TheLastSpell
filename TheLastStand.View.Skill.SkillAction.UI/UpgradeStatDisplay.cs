using TMPro;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class UpgradeStatDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/UpgradeStatDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI upgradeStatValue;

	public void Init(UnitStatDefinition.E_Stat stat, int value)
	{
		base.Init(value);
		string text = string.Format("{0}{1}{2}", (value >= 0) ? "+" : "", value, stat.ShownAsPercentage() ? "%" : string.Empty);
		text = "<style=" + ((value >= 0) ? "GoodNb" : "BadNb") + ">" + text + "</style>";
		upgradeStatValue.text = $"<size=16><sprite name=UpgradeStat></size> {text} <style={stat}></style>";
	}
}

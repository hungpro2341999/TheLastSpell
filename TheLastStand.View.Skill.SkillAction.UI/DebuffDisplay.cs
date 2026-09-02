using TMPro;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class DebuffDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/DebuffDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI debuffText;

	public void Init(UnitStatDefinition.E_Stat stat, int value)
	{
		base.Init(value);
		debuffText.text = $"<style=Debuff></style> <style=BadNb>{value}</style> <style={stat}></style>";
	}
}

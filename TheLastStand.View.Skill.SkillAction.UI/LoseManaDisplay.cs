using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class LoseManaDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string LoseManaDisplayPrefab = "Prefab/Displayable Effect/UI Effect Displays/LoseManaDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI manaGainDisplay;

	private int manaLoss;

	public override void Init(int value)
	{
		base.Init(value);
		manaLoss = value;
		RefreshText();
	}

	public void AddManaLoss(int value)
	{
		manaLoss += value;
		RefreshText();
	}

	private void RefreshText()
	{
		manaGainDisplay.text = $"-{Mathf.Abs(manaLoss)}";
	}
}

using TMPro;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Unit.Stat;

public class UnitStatWithRegenDisplay : UnitStatDisplay
{
	[SerializeField]
	[Tooltip("Used to show the regen for health or mana")]
	private TextMeshProUGUI regenStatValueText;

	private UnitStatDefinition regenStatDefinition;

	public UnitStatDefinition RegenStatDefinition
	{
		get
		{
			return regenStatDefinition;
		}
		set
		{
			if (regenStatDefinition != value)
			{
				regenStatDefinition = value;
				fullRefreshNeeded = true;
			}
		}
	}

	protected override void RefreshValues()
	{
		base.RefreshValues();
		if (RegenStatDefinition != null && regenStatValueText != null && TileObjectSelectionManager.SelectedUnit.UnitStatsController.UnitStats.Stats.ContainsKey(RegenStatDefinition.Id))
		{
			regenStatValueText.text = $"+{TileObjectSelectionManager.SelectedUnit.GetClampedStatValue(RegenStatDefinition.Id)}";
		}
	}
}

using TheLastStand.Definition.Unit.Trait;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Unit.Trait;

public class TraitTooltip : TooltipBase
{
	[SerializeField]
	private UnitTraitDisplay unitTraitTooltipDisplay;

	public void SetContent(UnitTraitDefinition unitTraitDefinition)
	{
		unitTraitTooltipDisplay.UnitTraitDefinition = unitTraitDefinition;
	}

	protected override bool CanBeDisplayed()
	{
		return unitTraitTooltipDisplay.UnitTraitDefinition != null;
	}

	protected override void RefreshContent()
	{
		unitTraitTooltipDisplay.Refresh();
	}
}

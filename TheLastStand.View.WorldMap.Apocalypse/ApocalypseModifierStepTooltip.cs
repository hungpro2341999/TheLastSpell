using TMPro;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseModifierStepTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI modifierStepEffectsText;

	private ApocalypseModifierStepDefinition currentModifierStepDefinition;

	public void Init(ApocalypseModifierStepDefinition modifierStepDefinition)
	{
		currentModifierStepDefinition = modifierStepDefinition;
	}

	protected override bool CanBeDisplayed()
	{
		return currentModifierStepDefinition != null;
	}

	protected override void RefreshContent()
	{
		if (currentModifierStepDefinition != null)
		{
			modifierStepEffectsText.text = currentModifierStepDefinition.GetLocalizedDescription();
		}
	}
}

using System.Collections.Generic;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Manager;

namespace TheLastStand.View.Tooltip;

public class ApocalypseEffectsTooltipDisplayer : ObjectDisableTooltipDisplayer
{
	private uint? damnedSoulsPercentageModifier;

	private List<ApocalypseModifierStepDefinition> apocalypseModifierStepDefinitions;

	private int maximumModifiersDisplayed = 14;

	private bool? isApocalypseUnlocked;

	private ApocalypseEffectsTooltip ApocalypseEffectsTooltip => targetTooltip as ApocalypseEffectsTooltip;

	public void SetDamnedSoulsPercentageModifier(uint? value)
	{
		damnedSoulsPercentageModifier = value;
	}

	public void SetApocalypseModifierStepDefinitions(List<ApocalypseModifierStepDefinition> stepDefinitions)
	{
		apocalypseModifierStepDefinitions = stepDefinitions;
	}

	public void SetIsApocalypseSystemUnlocked(bool? isUnlocked)
	{
		isApocalypseUnlocked = isUnlocked;
	}

	public void SetMaximumModifiersDisplayed(int newMaximum = -1)
	{
		if (newMaximum == -1)
		{
			maximumModifiersDisplayed = 14;
		}
		else
		{
			maximumModifiersDisplayed = newMaximum;
		}
	}

	private void Start()
	{
		ApocalypseEffectsTooltip.SetDamnedSoulsPercentageModifier(damnedSoulsPercentageModifier);
		ApocalypseEffectsTooltip.SetApocalypseModifierStepDefinitions(ApocalypseManager.CurrentApocalypseModifierStepDefinitions);
		ApocalypseEffectsTooltip.SetIsApocalypseSystemUnlocked(isApocalypseUnlocked);
		ApocalypseEffectsTooltip.SetMaximumModifiersDisplayed(maximumModifiersDisplayed);
	}

	public override void DisplayTooltip()
	{
		if (apocalypseModifierStepDefinitions != null)
		{
			ApocalypseEffectsTooltip.SetDamnedSoulsPercentageModifier(damnedSoulsPercentageModifier);
			ApocalypseEffectsTooltip.SetApocalypseModifierStepDefinitions(apocalypseModifierStepDefinitions);
			ApocalypseEffectsTooltip.SetIsApocalypseSystemUnlocked(isApocalypseUnlocked);
			ApocalypseEffectsTooltip.SetMaximumModifiersDisplayed(maximumModifiersDisplayed);
		}
		ApocalypseEffectsTooltip.FollowElement.ChangeTarget(base.transform);
		base.DisplayTooltip();
	}
}

using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.View.Apocalypse;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseSelectionPreview : MonoBehaviour
{
	[SerializeField]
	private ApocalypseHeader apocalypseHeader;

	[SerializeField]
	private Button editButton;

	[SerializeField]
	private Selectable levelViewSelectable;

	[SerializeField]
	private Selectable editButtonSelectable;

	public Selectable EditButtonSelectable => editButtonSelectable;

	public void ContinueAnimations()
	{
		apocalypseHeader.ContinueAnimations();
	}

	public void PauseAnimations()
	{
		apocalypseHeader.PauseAnimations();
	}

	public void Refresh()
	{
		apocalypseHeader.RefreshApocalypseLevel(useTween: false);
		apocalypseHeader.RefreshRewardsFlames();
		apocalypseHeader.ApocalypseEffectsTooltip.SetApocalypseModifierStepDefinitions(ApocalypseManager.CurrentApocalypseModifierStepDefinitions);
	}

	private void InitJoystickNavigation()
	{
		levelViewSelectable.SetSelectOnUp(EditButtonSelectable);
		if (!(apocalypseHeader.ApocalypseGaugeDisplay != null))
		{
			return;
		}
		int rewardAtLevelTooltipDisplayersNb = apocalypseHeader.ApocalypseGaugeDisplay.RewardAtLevelTooltipDisplayersNb;
		if (rewardAtLevelTooltipDisplayersNb == 0)
		{
			return;
		}
		bool flag = false;
		for (int num = rewardAtLevelTooltipDisplayersNb - 1; num >= 0; num--)
		{
			ApocalypseRewardAtLevelTooltipDisplayer rewardTooltipDisplayerAtIndex = apocalypseHeader.ApocalypseGaugeDisplay.GetRewardTooltipDisplayerAtIndex(num);
			if (rewardTooltipDisplayerAtIndex != null)
			{
				rewardTooltipDisplayerAtIndex.JoystickSelectable.SetSelectOnUp(editButton);
				if (levelViewSelectable != null && !flag)
				{
					flag = true;
					levelViewSelectable.SetSelectOnLeft(rewardTooltipDisplayerAtIndex.JoystickSelectable);
					rewardTooltipDisplayerAtIndex.JoystickSelectable.SetSelectOnRight(levelViewSelectable);
				}
			}
		}
	}

	private void Start()
	{
		apocalypseHeader.ApocalypseEffectsTooltip.SetMaximumModifiersDisplayed(10);
		InitJoystickNavigation();
	}
}

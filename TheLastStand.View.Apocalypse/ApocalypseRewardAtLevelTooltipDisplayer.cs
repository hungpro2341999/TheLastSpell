using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPLib.Localization;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Manager;
using TheLastStand.View.HUD;
using TheLastStand.View.Tooltip;
using TheLastStand.View.WorldMap.Apocalypse;
using UnityEngine;

namespace TheLastStand.View.Apocalypse;

public class ApocalypseRewardAtLevelTooltipDisplayer : ObjectDisableTooltipDisplayer
{
	private static class Constants
	{
		public const string AnimatorParamBoolIsLit = "IsLit";
	}

	[SerializeField]
	private GameObject completionFeedbackContainer;

	[SerializeField]
	private Animator flamesAnimator;

	[SerializeField]
	private ApocalypseFlameKindleFeedback flameKindleFeedback;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	private int[] apocalypseLevels;

	private List<ApocalypseRewardTextFormatting> rewardsTextFormatting = new List<ApocalypseRewardTextFormatting>();

	private static readonly int AnimatorIsLitStateHash = Animator.StringToHash("IsLit");

	private bool haveRewardBeenUnlocked;

	private ApocalypseGaugeDisplay gaugeDisplay;

	private bool isLit;

	private ApocalypseLevelRewardTooltip ApocalypseLevelRewardTooltip => targetTooltip as ApocalypseLevelRewardTooltip;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public RectTransform RectTransform => rectTransform;

	public void ContinueAnimations()
	{
		if (flamesAnimator != null)
		{
			flamesAnimator.speed = 1f;
		}
		if (flameKindleFeedback != null)
		{
			flameKindleFeedback.ContinueAnimations();
		}
	}

	public override void DisplayTooltip()
	{
		ApocalypseLevelRewardTooltip.FollowElement.ChangeTarget(base.transform);
		ApocalypseLevelRewardTooltip.Init(apocalypseLevels, rewardsTextFormatting.ToList());
		base.DisplayTooltip();
	}

	public void Init(ApocalypseGaugeDisplay gaugeDisplay, ApocalypseLevelRewardTooltip targetedTooltip = null, params int[] targetedApocalypseLevels)
	{
		if (targetedApocalypseLevels != null && targetedApocalypseLevels.Length != 0)
		{
			if (targetedTooltip != null)
			{
				targetTooltip = targetedTooltip;
			}
			apocalypseLevels = targetedApocalypseLevels;
			RefreshRewardsText();
			haveRewardBeenUnlocked = ApocalypseManager.GetHighestApocalypseLevelReached() >= apocalypseLevels[^1];
			if (completionFeedbackContainer != null)
			{
				completionFeedbackContainer.SetActive(haveRewardBeenUnlocked);
			}
			RefreshIcon();
			UnsubscribeGaugeDisplayEvent();
			this.gaugeDisplay = gaugeDisplay;
			SubscribeGaugeDisplayEvent();
		}
	}

	public void PauseAnimations()
	{
		if (flamesAnimator != null)
		{
			flamesAnimator.speed = 0f;
		}
		if (flameKindleFeedback != null)
		{
			flameKindleFeedback.PauseAnimations();
		}
	}

	public void RefreshIcon()
	{
		RefreshIcon(-1, playKindleFeedback: false, forceRefresh: true);
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshRewardsText));
	}

	private void CheckRefreshFlame(int apocalypseLevel)
	{
		RefreshIcon(apocalypseLevel, playKindleFeedback: true, forceRefresh: false);
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshRewardsText));
		UnsubscribeGaugeDisplayEvent();
	}

	private void RefreshIcon(int apocalypseLevelToCheck, bool playKindleFeedback, bool forceRefresh)
	{
		if (flamesAnimator == null || apocalypseLevels == null || apocalypseLevels.Length == 0)
		{
			return;
		}
		if (apocalypseLevelToCheck == -1)
		{
			apocalypseLevelToCheck = ((ApocalypseManager.CurrentApocalypse != null) ? ApocalypseManager.CurrentApocalypse.CurrentLevel : 0);
		}
		bool flag = apocalypseLevelToCheck >= apocalypseLevels[^1];
		if (flag != isLit || forceRefresh)
		{
			flamesAnimator.SetBool(AnimatorIsLitStateHash, flag);
			isLit = flag;
			if (isLit && playKindleFeedback && flameKindleFeedback != null)
			{
				flameKindleFeedback.PlayFeedback();
			}
		}
	}

	private void RefreshRewardsText()
	{
		if (apocalypseLevels == null)
		{
			return;
		}
		rewardsTextFormatting.Clear();
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ApocalypseTierDefinition orderedTierDefinition in ApocalypseDatabase.OrderedTierDefinitions)
		{
			if (orderedTierDefinition.ApocalypseLevelCompletedToUnlock == -1 || Array.IndexOf(apocalypseLevels, orderedTierDefinition.ApocalypseLevelCompletedToUnlock) == -1)
			{
				continue;
			}
			num += ApocalypseDatabase.GetModifiersNbForTier(orderedTierDefinition.Id);
			List<ApocalypseModifierDefinition> modifiersDefinitionsForTier = ApocalypseDatabase.GetModifiersDefinitionsForTier(orderedTierDefinition.Id);
			if (modifiersDefinitionsForTier == null || modifiersDefinitionsForTier.Count <= 0)
			{
				continue;
			}
			int count = modifiersDefinitionsForTier.Count;
			for (int i = 0; i < count; i++)
			{
				stringBuilder.Append(" <style=keyword>• " + modifiersDefinitionsForTier[i].GetLocalizedTitle() + "</style>");
				if (i + 1 < count)
				{
					stringBuilder.AppendLine();
				}
			}
		}
		if (num > 0)
		{
			string textKey = (ApocalypseLevelRewardTooltip.IsGameOverVersion ? "ApocalypseLevelReward_VictoryScreenUnlockedModifiers" : "ApocalypseLevelReward_NewModifiers");
			rewardsTextFormatting.Add(new ApocalypseRewardTextFormatting(textKey, num, stringBuilder.ToString(), string.Join(", ", apocalypseLevels)));
		}
	}

	private void SubscribeGaugeDisplayEvent()
	{
		if (gaugeDisplay != null)
		{
			gaugeDisplay.OnGaugeApocalypseLevelChanged += CheckRefreshFlame;
		}
	}

	private void UnsubscribeGaugeDisplayEvent()
	{
		if (gaugeDisplay != null)
		{
			gaugeDisplay.OnGaugeApocalypseLevelChanged -= CheckRefreshFlame;
		}
	}
}

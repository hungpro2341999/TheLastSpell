using System;
using System.Collections.Generic;
using DG.Tweening;
using TPLib;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.View.Apocalypse;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseGaugeDisplay : MonoBehaviour
{
	private static class Constants
	{
		public static readonly int PlayFeedback = Animator.StringToHash("PlayFeedback");
	}

	[SerializeField]
	protected bool canLaunchOverMaxAnimation;

	[SerializeField]
	protected bool canKindleTooltipDisplayerAnimations;

	[SerializeField]
	protected Image foregroundOverMaxImage;

	[SerializeField]
	protected Slider slider;

	[SerializeField]
	protected Animator[] sliderAnimators;

	[SerializeField]
	private Animator gaugeFullFeedbackAnimator;

	[SerializeField]
	private RectTransform rewardTooltipDisplayersContainer;

	[SerializeField]
	private ApocalypseRewardAtLevelTooltipDisplayer rewardTooltipDisplayerPrefab;

	[SerializeField]
	private ApocalypseLevelRewardTooltip apocalypseLevelRewardTooltip;

	[SerializeField]
	private float apocalypseLevelRewardTooltipDisplayYOffset;

	[SerializeField]
	private float sliderTweenDuration = 1f;

	[SerializeField]
	private Ease sliderTweenEasing = Ease.Linear;

	private List<ApocalypseRewardAtLevelTooltipDisplayer> rewardAtLevelTooltipDisplayers = new List<ApocalypseRewardAtLevelTooltipDisplayer>();

	private uint sliderMaxApocalypseLevelDisplayed;

	private int targetedApocalypseLevel;

	private float targetedSliderValue;

	private float currentSliderValue;

	private Tween sliderTween;

	private bool maxApocalypseLevelAnimationDisplayed;

	public ApocalypseLevelRewardTooltip ApocalypseLevelRewardTooltip => apocalypseLevelRewardTooltip;

	public int RewardAtLevelTooltipDisplayersNb => rewardAtLevelTooltipDisplayers.Count;

	public event Action<int> OnGaugeApocalypseLevelChanged = delegate
	{
	};

	public void ContinueAnimations()
	{
		Animator[] array = sliderAnimators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		foreach (ApocalypseRewardAtLevelTooltipDisplayer rewardAtLevelTooltipDisplayer in rewardAtLevelTooltipDisplayers)
		{
			rewardAtLevelTooltipDisplayer.ContinueAnimations();
		}
	}

	public ApocalypseRewardAtLevelTooltipDisplayer GetRewardTooltipDisplayerAtIndex(int index)
	{
		if (index < 0 || index >= rewardAtLevelTooltipDisplayers.Count)
		{
			return null;
		}
		if (!rewardAtLevelTooltipDisplayers[index].gameObject.activeSelf)
		{
			return null;
		}
		return rewardAtLevelTooltipDisplayers[index];
	}

	public void InitApocalypseRewardsInSlider()
	{
		List<int> apocalypseLevelsForRewards = ApocalypseDatabase.GetApocalypseLevelsForRewards();
		sliderMaxApocalypseLevelDisplayed = ApocalypseDatabase.ConfigurationDefinition.GaugeDisplayMaxApocalypseLevel;
		int num = apocalypseLevelsForRewards.Count - rewardAtLevelTooltipDisplayers.Count;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				ApocalypseRewardAtLevelTooltipDisplayer item = UnityEngine.Object.Instantiate(rewardTooltipDisplayerPrefab, rewardTooltipDisplayersContainer);
				rewardAtLevelTooltipDisplayers.Add(item);
			}
		}
		int count = apocalypseLevelsForRewards.Count;
		int num2 = (int)rewardTooltipDisplayersContainer.rect.width;
		for (int j = 0; j < rewardAtLevelTooltipDisplayers.Count; j++)
		{
			ApocalypseRewardAtLevelTooltipDisplayer apocalypseRewardAtLevelTooltipDisplayer = rewardAtLevelTooltipDisplayers[j];
			if (j >= count)
			{
				apocalypseRewardAtLevelTooltipDisplayer.gameObject.SetActive(value: false);
				continue;
			}
			int num3 = apocalypseLevelsForRewards[j];
			if (num3 > sliderMaxApocalypseLevelDisplayed)
			{
				TPSingleton<ApocalypseManager>.Instance.LogError($"A reward with an apocalypse level of {num3} has been found, but the gauge maximum level displayed is {sliderMaxApocalypseLevelDisplayed}, this shouldn't happen ! Hiding the reward...", CLogLevel.MAJOR);
				apocalypseRewardAtLevelTooltipDisplayer.gameObject.SetActive(value: false);
				continue;
			}
			apocalypseRewardAtLevelTooltipDisplayer.gameObject.SetActive(value: true);
			apocalypseRewardAtLevelTooltipDisplayer.Init(canKindleTooltipDisplayerAnimations ? this : null, apocalypseLevelRewardTooltip, num3);
			float f = (float)(num2 * num3) / (float)sliderMaxApocalypseLevelDisplayed;
			apocalypseRewardAtLevelTooltipDisplayer.RectTransform.anchoredPosition = new Vector2(Mathf.FloorToInt(f), apocalypseLevelRewardTooltipDisplayYOffset);
		}
		RefreshJoystickNavigation(clearSelectablesNavigation: true);
	}

	public void PauseAnimations()
	{
		Animator[] array = sliderAnimators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		foreach (ApocalypseRewardAtLevelTooltipDisplayer rewardAtLevelTooltipDisplayer in rewardAtLevelTooltipDisplayers)
		{
			rewardAtLevelTooltipDisplayer.PauseAnimations();
		}
	}

	public void RefreshJoystickNavigation(bool clearSelectablesNavigation)
	{
		int count = rewardAtLevelTooltipDisplayers.Count;
		ApocalypseRewardAtLevelTooltipDisplayer apocalypseRewardAtLevelTooltipDisplayer = null;
		if (clearSelectablesNavigation)
		{
			for (int i = 0; i < count; i++)
			{
				apocalypseRewardAtLevelTooltipDisplayer = rewardAtLevelTooltipDisplayers[i];
				apocalypseRewardAtLevelTooltipDisplayer.JoystickSelectable.ClearNavigation();
			}
		}
		for (int j = 0; j < rewardAtLevelTooltipDisplayers.Count; j++)
		{
			apocalypseRewardAtLevelTooltipDisplayer = rewardAtLevelTooltipDisplayers[j];
			if (apocalypseRewardAtLevelTooltipDisplayer.gameObject.activeSelf)
			{
				ApocalypseRewardAtLevelTooltipDisplayer rewardTooltipDisplayerAtIndex = GetRewardTooltipDisplayerAtIndex(j - 1);
				ApocalypseRewardAtLevelTooltipDisplayer rewardTooltipDisplayerAtIndex2 = GetRewardTooltipDisplayerAtIndex(j + 1);
				if (rewardTooltipDisplayerAtIndex != null)
				{
					apocalypseRewardAtLevelTooltipDisplayer.JoystickSelectable.SetSelectOnLeft(rewardTooltipDisplayerAtIndex.JoystickSelectable);
				}
				if (rewardTooltipDisplayerAtIndex2 != null)
				{
					apocalypseRewardAtLevelTooltipDisplayer.JoystickSelectable.SetSelectOnRight(rewardTooltipDisplayerAtIndex2.JoystickSelectable);
				}
			}
		}
	}

	public void RefreshRewardsFlames()
	{
		if (rewardAtLevelTooltipDisplayers.Count <= 0)
		{
			return;
		}
		foreach (ApocalypseRewardAtLevelTooltipDisplayer rewardAtLevelTooltipDisplayer in rewardAtLevelTooltipDisplayers)
		{
			if (rewardAtLevelTooltipDisplayer.gameObject.activeSelf)
			{
				rewardAtLevelTooltipDisplayer.RefreshIcon();
			}
		}
	}

	public void SetSliderApocalypseValue(int apocalypseLevel, bool useTween = false)
	{
		bool foregroundOverMaxImageVisible = false;
		targetedApocalypseLevel = apocalypseLevel;
		if (apocalypseLevel > sliderMaxApocalypseLevelDisplayed)
		{
			targetedSliderValue = 1f;
			foregroundOverMaxImageVisible = true;
		}
		else
		{
			targetedSliderValue = (float)apocalypseLevel / (float)sliderMaxApocalypseLevelDisplayed;
		}
		if (useTween)
		{
			currentSliderValue = slider.value;
			if (!TPHelpers.IsApproxEqual(targetedSliderValue, currentSliderValue))
			{
				sliderTween?.Kill();
				sliderTween = DOTween.To(() => currentSliderValue, delegate(float x)
				{
					currentSliderValue = x;
				}, targetedSliderValue, sliderTweenDuration).SetEase(sliderTweenEasing).OnUpdate(OnUpdateSliderTween);
			}
		}
		else
		{
			maxApocalypseLevelAnimationDisplayed = false;
			currentSliderValue = targetedSliderValue;
			slider.value = targetedSliderValue;
			SetForegroundOverMaxImageVisible(foregroundOverMaxImageVisible);
			this.OnGaugeApocalypseLevelChanged(targetedApocalypseLevel);
		}
	}

	private void OnUpdateSliderTween()
	{
		slider.value = currentSliderValue;
		bool flag = TPHelpers.IsApproxEqual(slider.value, 1f);
		SetForegroundOverMaxImageVisible(flag && targetedApocalypseLevel > sliderMaxApocalypseLevelDisplayed);
		if (!flag)
		{
			maxApocalypseLevelAnimationDisplayed = false;
		}
		if (flag && !maxApocalypseLevelAnimationDisplayed && canLaunchOverMaxAnimation)
		{
			ShowFullGaugeFeedback();
			maxApocalypseLevelAnimationDisplayed = true;
		}
		this.OnGaugeApocalypseLevelChanged(Mathf.FloorToInt(slider.value * (float)sliderMaxApocalypseLevelDisplayed));
	}

	private void SetForegroundOverMaxImageVisible(bool isVisible)
	{
		if (foregroundOverMaxImage.gameObject.activeSelf != isVisible)
		{
			foregroundOverMaxImage.gameObject.SetActive(isVisible);
		}
	}

	private void ShowFullGaugeFeedback()
	{
		gaugeFullFeedbackAnimator.SetTrigger(Constants.PlayFeedback);
		if (TPSingleton<ApocalypseSelectionPanel>.Instance != null)
		{
			TPSingleton<ApocalypseSelectionPanel>.Instance.PlayGaugeFullSfx();
		}
	}
}

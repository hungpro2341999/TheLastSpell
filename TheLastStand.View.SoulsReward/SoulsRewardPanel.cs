using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Database;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Trophy;
using TheLastStand.View.HUD;
using TheLastStand.View.Trophy;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.SoulsReward;

public class SoulsRewardPanel : MonoBehaviour
{
	public static class Constants
	{
		public const string SoulGainAnimatorFadeIn = "FadeIn";

		public const string SoulGainAnimatorFadeOut = "FadeOut";
	}

	[SerializeField]
	private CanvasGroup soulsRewardCanvasGroup;

	[SerializeField]
	private RectTransform trophyParent;

	[SerializeField]
	private TrophyDisplay trophyPrefab;

	[SerializeField]
	private RectTransform trophyBoardMask;

	[SerializeField]
	private ScrollRect trophiesScrollRect;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitBetweenEachTrophyAppear = 0.6f;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitAfterShowingAllTrophy = 0.5f;

	[SerializeField]
	private GameObject trophyLeftButton;

	[SerializeField]
	private GameObject trophyRightButton;

	[SerializeField]
	private RectTransform scrollViewport;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer layoutNavigationInitializer;

	[SerializeField]
	private HUDJoystickTarget joystickParent;

	[SerializeField]
	private RectTransform damnedSoulsNightTotalMask;

	[SerializeField]
	private RectTransform damnedSoulsNightTotalRectTransform;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsNightTotalText;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsNightTotalTransparentText;

	[SerializeField]
	[Range(0f, 2f)]
	private float damnedSoulsNightPunchDuration = 0.2f;

	[SerializeField]
	[Range(1f, 3f)]
	private float damnedSoulsNightPunchStrength = 1.2f;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsTotalText;

	[SerializeField]
	[Range(0f, 5f)]
	private float damnedSoulsTotalBlinkDuration = 0.4f;

	[SerializeField]
	[Range(0f, 1f)]
	private float damnedSoulsTotalBlinkFadeTo = 0.6f;

	[SerializeField]
	[Range(1f, 5000f)]
	private float damnedSoulsTransferSoulsPerSecond = 500f;

	[SerializeField]
	private Ease damnedSoulsTransferEasing = Ease.Linear;

	[SerializeField]
	[Range(0f, 2f)]
	private float damnedSoulsTotalPunchDuration = 0.2f;

	[SerializeField]
	[Range(1f, 3f)]
	private float damnedSoulsTotalPunchStrength = 1.2f;

	[SerializeField]
	private Animator soulGainAnimator;

	[SerializeField]
	private Image soulGainImage;

	[SerializeField]
	private float soulGainFadeInDuration = 0.5f;

	[SerializeField]
	private float soulGainFadeOutDuration = 0.5f;

	[SerializeField]
	private float soulGainIdleLoopDurationMultiplier = 1f;

	[SerializeField]
	private float soulGainIdleLoopDurationMax = 7f;

	[SerializeField]
	private AudioSource trophyAudioSource;

	[SerializeField]
	private List<AudioClip> trophyAudioClips;

	[SerializeField]
	private AudioSource soulsBumpAudioSource;

	[SerializeField]
	private AudioSource soulsRiseAudioSource;

	[SerializeField]
	private AudioSource soulsHitAudioSource;

	[SerializeField]
	[Min(0f)]
	private float soulsRiseFadeOutDuration = 0.5f;

	[SerializeField]
	private Ease soulsRiseFadeOutEase = Ease.InOutSine;

	private Tween damnedSoulsBlinkTween;

	private Tween damnedSoulsNightTotalBlinkTween;

	private int maxAmountOfSoulGainIdleLoops;

	private float soulGainByIdleLoop;

	private float soulGainByFade;

	private List<TrophyDisplay> trophyDisplays = new List<TrophyDisplay>();

	private bool canContinue;

	private int nextTrophySourceIndex;

	private float soulsRiseAudioSourceTargetVolume;

	private void Awake()
	{
		soulGainByIdleLoop = soulGainIdleLoopDurationMultiplier * damnedSoulsTransferSoulsPerSecond;
		soulGainByFade = (soulGainFadeInDuration + soulGainFadeOutDuration) * damnedSoulsTransferSoulsPerSecond;
		maxAmountOfSoulGainIdleLoops = Mathf.FloorToInt((soulsRiseAudioSource.clip.length - 0.5f - soulGainFadeInDuration - soulGainFadeOutDuration) / soulGainIdleLoopDurationMultiplier);
		soulsRiseAudioSourceTargetVolume = soulsRiseAudioSource.volume;
	}

	public void RefreshPosition()
	{
		damnedSoulsNightTotalMask.localPosition = new Vector3(0f, damnedSoulsNightTotalMask.localPosition.y, 0f);
		damnedSoulsNightTotalRectTransform.localPosition = new Vector3(0f, damnedSoulsNightTotalRectTransform.localPosition.y, 0f);
	}

	public void Display(bool firstTimeOpenedThisNight)
	{
		trophiesScrollRect.enabled = true;
		if (firstTimeOpenedThisNight)
		{
			soulsRewardCanvasGroup.alpha = 0f;
			soulsRewardCanvasGroup.blocksRaycasts = false;
			trophyLeftButton.SetActive(value: false);
			trophyRightButton.SetActive(value: false);
			return;
		}
		if (ApplicationManager.Application.RunsCompleted != 0)
		{
			soulsRewardCanvasGroup.alpha = 1f;
			soulsRewardCanvasGroup.blocksRaycasts = true;
		}
		if (InputManager.IsLastControllerJoystick && trophyDisplays.Count > 0)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
		}
	}

	public void ClearTrophies()
	{
		for (int i = 0; i < trophyDisplays.Count; i++)
		{
			trophyDisplays[i].gameObject.SetActive(value: false);
		}
		trophyDisplays.Clear();
		joystickTarget.ClearSelectables();
	}

	public Selectable GetFirstSelectableTrophy()
	{
		return trophyDisplays.FirstOrDefault()?.Selectable;
	}

	public void Hide()
	{
		soulsRewardCanvasGroup.alpha = 0f;
		soulsRewardCanvasGroup.blocksRaycasts = false;
		trophiesScrollRect.enabled = false;
	}

	public void MoveLeft()
	{
		trophyParent.localPosition = new Vector3(trophyParent.localPosition.x + 125f, trophyParent.localPosition.y, trophyParent.localPosition.z);
	}

	public void MoveRight()
	{
		trophyParent.localPosition = new Vector3(trophyParent.localPosition.x - 125f, trophyParent.localPosition.y, trophyParent.localPosition.z);
	}

	public IEnumerator ShowTrophiesPanel(bool isDefeat)
	{
		damnedSoulsTotalText.text = $"{Mathf.Max(0f, ApplicationManager.Application.DamnedSouls - TPSingleton<TrophyManager>.Instance.ComputedDamnedSoulsEarnedThisNight)}";
		soulsRewardCanvasGroup.DOFade(1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId("SoulsRewardFadeIn", this);
		soulsRewardCanvasGroup.blocksRaycasts = true;
		int nightTotal = 0;
		TrophyDisplay pooledComponent = ObjectPooler.GetPooledComponent("TrophyDisplays", trophyPrefab, trophyParent);
		pooledComponent.Init(trophyParent, scrollViewport);
		string text = Localizer.Get("TrophyName_" + TrophyDatabase.DefaultTrophyDefinition.Id);
		string description = "TrophyDescription_" + TrophyDatabase.DefaultTrophyDefinition.Id;
		pooledComponent.Refresh(text, TPSingleton<TrophyManager>.Instance.DamnedSoulsEarnedThisNightWithMultiplier, description, TrophyDatabase.DefaultTrophyDefinition.IgnoreGem, TrophyDatabase.DefaultTrophyDefinition.BackgroundPath);
		pooledComponent.Show();
		trophyDisplays.Add(pooledComponent);
		soulsBumpAudioSource.Play();
		nightTotal += (int)TPSingleton<TrophyManager>.Instance.DamnedSoulsEarnedThisNightWithMultiplier;
		damnedSoulsNightTotalText.text = $"{nightTotal}";
		damnedSoulsNightTotalTransparentText.text = $"{nightTotal}";
		damnedSoulsNightTotalText.rectTransform.DOPunchScale(Vector3.one * damnedSoulsNightPunchStrength, damnedSoulsNightPunchDuration, 1, 0.1f).SetFullId("NightTotalPunchScale", this).OnComplete(delegate
		{
			canContinue = true;
		});
		damnedSoulsNightTotalTransparentText.rectTransform.DOPunchScale(Vector3.one * damnedSoulsNightPunchStrength, damnedSoulsNightPunchDuration, 1, 0.1f).SetFullId("NightTotalPunchScale", this).OnComplete(delegate
		{
			canContinue = true;
		});
		while (!canContinue)
		{
			yield return null;
		}
		canContinue = false;
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBetweenEachTrophyAppear);
		LayoutRebuilder.ForceRebuildLayoutImmediate(trophyParent);
		ListExtensions.Shuffle(trophyAudioClips);
		List<TheLastStand.Model.Trophy.Trophy> trophies = TPSingleton<TrophyManager>.Instance.GetSuccessfulTrophies(isDefeat);
		int trophyIndex = 0;
		int trophyCount = trophies.Count;
		while (trophyIndex < trophyCount)
		{
			TheLastStand.Model.Trophy.Trophy trophy = trophies[trophyIndex];
			TrophyDisplay pooledComponent2 = ObjectPooler.GetPooledComponent("TrophyDisplays", trophyPrefab, trophyParent);
			pooledComponent2.Init(trophyParent, scrollViewport);
			pooledComponent2.Refresh(trophy);
			pooledComponent2.Show();
			trophyDisplays.Add(pooledComponent2);
			trophyAudioSource.PlayOneShot(trophyAudioClips[nextTrophySourceIndex++ % trophyAudioClips.Count]);
			nightTotal += (int)trophy.TrophyDefinition.DamnedSoulsEarned;
			damnedSoulsNightTotalText.text = $"{nightTotal}";
			damnedSoulsNightTotalTransparentText.text = $"{nightTotal}";
			damnedSoulsNightTotalText.rectTransform.DOPunchScale(Vector3.one * damnedSoulsNightPunchStrength, damnedSoulsNightPunchDuration, 1, 0.1f).SetFullId("NightTotalPunchScale", this).OnComplete(delegate
			{
				canContinue = true;
			});
			damnedSoulsNightTotalTransparentText.rectTransform.DOPunchScale(Vector3.one * damnedSoulsNightPunchStrength, damnedSoulsNightPunchDuration, 1, 0.1f).SetFullId("NightTotalTransparentPunchScale", this).OnComplete(delegate
			{
				canContinue = true;
			});
			while (!canContinue)
			{
				yield return null;
			}
			canContinue = false;
			if (trophyIndex != trophyCount - 1)
			{
				yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBetweenEachTrophyAppear);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(trophyParent);
			if (trophyParent.sizeDelta.x > trophyBoardMask.sizeDelta.x - 100f)
			{
				Vector3 localPosition = trophyParent.localPosition;
				localPosition.x -= 1000f;
				trophyParent.localPosition = localPosition;
			}
			int num = trophyIndex + 1;
			trophyIndex = num;
		}
		layoutNavigationInitializer.InitNavigation();
		joystickTarget.AddSelectables(trophyDisplays.Select((TrophyDisplay x) => x.Selectable));
		if (InputManager.IsLastControllerJoystick && trophyDisplays.Count > 0)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickParent.GetSelectionInfo());
		}
		trophyLeftButton.SetActive(trophyParent.sizeDelta.x > trophyBoardMask.sizeDelta.x);
		trophyRightButton.SetActive(trophyParent.sizeDelta.x > trophyBoardMask.sizeDelta.x);
		yield return PlaySoulsTransferAnim(nightTotal);
	}

	public IEnumerator PlaySoulsTransferAnim(int nightTotal)
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterShowingAllTrophy);
		soulsRiseAudioSource.Play();
		soulsRiseAudioSource.volume = soulsRiseAudioSourceTargetVolume;
		float num = soulGainFadeInDuration + soulGainFadeOutDuration;
		int a = Mathf.RoundToInt(Mathf.Max((float)nightTotal - soulGainByFade, 0f) / soulGainByIdleLoop);
		a = Mathf.Min(a, maxAmountOfSoulGainIdleLoops);
		num += (float)a * soulGainIdleLoopDurationMultiplier;
		soulGainImage.enabled = true;
		soulGainAnimator.SetTrigger("FadeIn");
		DOTween.To(() => nightTotal, delegate(int x)
		{
			damnedSoulsTotalText.text = $"{ApplicationManager.Application.DamnedSouls - x}";
		}, 0, PlayableUnitManager.DebugForceSkipNightReport ? 0f : num).SetEase(damnedSoulsTransferEasing).SetFullId("TransferDamnedSouls", this);
		DOTween.To(() => damnedSoulsNightTotalMask.localPosition.x, delegate(float x)
		{
			damnedSoulsNightTotalMask.localPosition = new Vector3(x, damnedSoulsNightTotalMask.localPosition.y, 0f);
			damnedSoulsNightTotalRectTransform.localPosition = new Vector3(0f - x, damnedSoulsNightTotalRectTransform.localPosition.y, 0f);
		}, damnedSoulsNightTotalMask.rect.width, PlayableUnitManager.DebugForceSkipNightReport ? 0f : num).SetEase(Ease.Linear);
		damnedSoulsBlinkTween = damnedSoulsTotalText.DOFade(damnedSoulsTotalBlinkFadeTo, damnedSoulsTotalBlinkDuration).SetLoops(-1, LoopType.Yoyo).SetFullId("DamnedSoulsBlink", this);
		damnedSoulsNightTotalBlinkTween = damnedSoulsNightTotalText.DOFade(damnedSoulsTotalBlinkFadeTo, damnedSoulsTotalBlinkDuration).SetLoops(-1, LoopType.Yoyo).SetFullId("DamnedSoulsNightTotalBlink", this);
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : (soulGainFadeInDuration + (float)a * soulGainIdleLoopDurationMultiplier));
		soulGainAnimator.SetTrigger("FadeOut");
		soulsHitAudioSource.Play();
		soulsRiseAudioSource.DOFade(0f, soulsRiseFadeOutDuration).SetEase(soulsRiseFadeOutEase).OnComplete(soulsRiseAudioSource.Stop);
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : soulGainFadeOutDuration);
		soulGainImage.enabled = false;
		damnedSoulsBlinkTween.Kill();
		damnedSoulsNightTotalBlinkTween.Kill();
		damnedSoulsNightTotalText.alpha = 1f;
		damnedSoulsTotalText.alpha = 1f;
		damnedSoulsTotalText.rectTransform.DOPunchScale(Vector3.one * damnedSoulsTotalPunchStrength, damnedSoulsTotalPunchDuration, 1, 0.1f).SetFullId("DamnedSoulsTotalPunchScale", this);
	}
}

using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Panic;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Panic;
using TheLastStand.View.NightReport;
using UnityEngine;

namespace TheLastStand.View.Panic;

public class PanicRewardIndicator : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 10f)]
	private float timeToMoveFromMaxToMin = 3f;

	[SerializeField]
	private Ease moveEasing = Ease.OutCubic;

	[SerializeField]
	[Range(0f, 5f)]
	private float movePauseAtEachPanicLevel = 0.7f;

	[SerializeField]
	[Range(0f, 5f)]
	private float blinkDuration = 0.4f;

	[SerializeField]
	[Range(0f, 1f)]
	private float blinkFadeTo = 0.7f;

	[SerializeField]
	[Range(0f, 2f)]
	private float punchScaleDuration = 0.2f;

	[SerializeField]
	[Range(1f, 3f)]
	private float punchScaleStrength = 1.1f;

	[SerializeField]
	private RectTransform handleRect;

	[SerializeField]
	private TextMeshProUGUI goldRewardText;

	[SerializeField]
	private TextMeshProUGUI materialRewardText;

	[SerializeField]
	private TextMeshProUGUI itemRewardText;

	[SerializeField]
	private CanvasGroup rewardTextsCanvasGroup;

	[SerializeField]
	private AudioClip[] panicStepsMoveAudioClip;

	[SerializeField]
	private AudioClip[] panicStepsCoinAudioClip;

	private Tween goldTween;

	private Tween itemsTween;

	private Tween materialTween;

	private Tween blinkTween;

	private int lastGold;

	private int lastItemsCount;

	private int lastMaterials;

	public bool IsMoving { get; set; }

	public void Init(TheLastStand.Model.Panic.Panic panic)
	{
		handleRect.anchorMin = new Vector2(1f, handleRect.anchorMin.y);
		handleRect.anchorMax = new Vector2(1f, handleRect.anchorMax.y);
		lastGold = 0;
		lastItemsCount = 0;
		lastMaterials = 0;
		RefreshValues(panic, forceRefresh: true);
	}

	public void Refresh(TheLastStand.Model.Panic.Panic panic, bool instant = false)
	{
		IsMoving = true;
		float num = panic.Value / panic.PanicDefinition.ValueMax;
		float num2 = 1f;
		Sequence s = DOTween.Sequence().SetFullId("MoveIndicator", this);
		for (int panicLevel = panic.PanicDefinition.PanicLevelDefinitions.Length - 1; panicLevel >= 0; panicLevel--)
		{
			float num3 = ((num * 100f >= panic.PanicDefinition.PanicLevelDefinitions[panicLevel].PanicValueNeeded) ? num : (panic.PanicDefinition.PanicLevelDefinitions[panicLevel].PanicValueNeeded / 100f));
			int currentLevel = panicLevel;
			s.Append(DOTween.To(() => handleRect.anchorMin.x, delegate(float x)
			{
				handleRect.anchorMin = new Vector2(x, handleRect.anchorMin.y);
				handleRect.anchorMax = new Vector2(x, handleRect.anchorMax.y);
			}, num3, instant ? 0f : (timeToMoveFromMaxToMin * (num2 - num3))).SetEase(moveEasing).SetFullId("MoveIndicatorToNextLevel", this)
				.OnPlay(delegate
				{
					PlayMoveSound(currentLevel);
				})
				.OnComplete(delegate
				{
					RefreshValues(panic, panicLevel == panic.PanicDefinition.PanicLevelDefinitions.Length - 1);
				})).OnComplete(delegate
			{
				FinishTween();
			});
			s.AppendInterval(instant ? 0f : movePauseAtEachPanicLevel);
			num2 = num3;
			if (num * 100f >= panic.PanicDefinition.PanicLevelDefinitions[panicLevel].PanicValueNeeded)
			{
				break;
			}
		}
		blinkTween?.Kill();
		rewardTextsCanvasGroup.alpha = 1f;
		blinkTween = rewardTextsCanvasGroup.DOFade(blinkFadeTo, blinkDuration).SetLoops(-1, LoopType.Yoyo).SetFullId("BlinkRewards", this);
	}

	private void FinishTween()
	{
		IsMoving = false;
		blinkTween?.Kill();
		rewardTextsCanvasGroup.alpha = 1f;
	}

	private void PlayMoveSound(int stepIndex)
	{
		if (panicStepsMoveAudioClip[stepIndex] != null)
		{
			TPSingleton<NightReportPanel>.Instance.PlayAudioClip(panicStepsMoveAudioClip[stepIndex]);
		}
	}

	private void RefreshValues(TheLastStand.Model.Panic.Panic panic, bool forceRefresh = false)
	{
		int num = 0;
		for (int num2 = panic.PanicDefinition.PanicLevelDefinitions.Length - 1; num2 >= 0; num2--)
		{
			if (handleRect.anchorMin.x >= panic.PanicDefinition.PanicLevelDefinitions[num2].PanicValueNeeded * 0.01f)
			{
				num = num2;
				break;
			}
		}
		string text = null;
		int num3 = -1;
		if (panic.PanicDefinition.PanicLevelDefinitions[num].PanicRewardDefinition.ItemsListsPerDay != null)
		{
			foreach (KeyValuePair<int, PanicRewardDefinition.DayGenerationDatas> item in panic.PanicDefinition.PanicLevelDefinitions[num].PanicRewardDefinition.ItemsListsPerDay)
			{
				if (text == null || (item.Key > num3 && item.Key <= TPSingleton<GameManager>.Instance.Game.DayNumber))
				{
					text = item.Value.ItemsListId;
				}
			}
		}
		bool flag = false;
		int num4 = panic.PanicDefinition.PanicLevelDefinitions[num].PanicRewardDefinition.Gold.EvalToInt(panic.PanicEvalGoldContext);
		int num5 = panic.PanicDefinition.PanicLevelDefinitions[num].PanicRewardDefinition.Materials.EvalToInt(panic.PanicEvalMaterialContext);
		int num6 = ((text != null) ? 1 : 0);
		if (num4 != lastGold || forceRefresh)
		{
			flag = true;
			lastGold = num4;
			goldRewardText.text = $"+{num4} <style=Gold></style>";
			goldTween?.Kill();
			goldRewardText.rectTransform.localScale = Vector3.one;
			goldTween = goldRewardText.rectTransform.DOPunchScale(Vector3.one * punchScaleStrength, punchScaleDuration, 1, 0.1f).SetFullId("GoldPunchScale", this);
		}
		if (num5 != lastMaterials || forceRefresh)
		{
			flag = true;
			lastMaterials = num5;
			materialRewardText.text = $"+{num5} <style=Materials></style>";
			materialTween?.Kill();
			materialRewardText.rectTransform.localScale = Vector3.one;
			materialTween = materialRewardText.rectTransform.DOPunchScale(Vector3.one * punchScaleStrength, punchScaleDuration, 1, 0.1f).SetFullId("MaterialsPunchScale", this);
		}
		if (num6 != lastItemsCount || forceRefresh)
		{
			flag = true;
			lastItemsCount = num6;
			itemRewardText.text = Localizer.Format("NightReportPanel_NightRewardItem", num6);
			itemsTween?.Kill();
			itemRewardText.rectTransform.localScale = Vector3.one;
			itemsTween = itemRewardText.rectTransform.DOPunchScale(Vector3.one * punchScaleStrength, punchScaleDuration, 1, 0.1f).SetFullId("ItemPunchScale", this);
		}
		if (flag && panicStepsCoinAudioClip[num] != null)
		{
			TPSingleton<NightReportPanel>.Instance.PlayAudioClip(panicStepsCoinAudioClip[num]);
		}
	}

	protected void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy && TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
		{
			RefreshValues(PanicManager.Panic);
		}
	}
}

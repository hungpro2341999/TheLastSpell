using System.Collections;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.MetaShops;

public class DarkMetaUpgradeLineView : MetaUpgradeLineView, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler
{
	private new static class Constants
	{
		public static class Animation
		{
			public const string FxExplode = "DarkFX_Explode";

			public const string SliderBackgroundIdle = "Idle";

			public const string SliderFillAreaIdle = "SoulGaugeIdle";

			public const string SliderHandleIdle = "SoulHandleIdle";
		}

		public const string ValueDisplayFormat = "<sprite name=DamnedSouls>{0}";
	}

	[SerializeField]
	private float minClampDuration = 0.4f;

	[SerializeField]
	private RectTransform boxRect;

	[SerializeField]
	private TextMeshProUGUI valueDisplayer;

	private Tween fillingTween;

	protected override string SliderBackgroundIdleKey => "Idle";

	protected override string SliderFillAreaIdleKey => "SoulGaugeIdle";

	protected override string SliderHandleIdleKey => "SoulHandleIdle";

	protected override string FxExplodeAnimationLabel => "DarkFX_Explode";

	public override void OnPointerDown(PointerEventData eventData = null)
	{
		if (base.State == E_State.Unlocked)
		{
			base.IsANewMetaUpgrade = false;
			TPSingleton<DarkShopManager>.Instance.UpdateNewMetaUpgrades();
			SoundManager.PlayAudioClip(MetaShopsManager.DarkUpgradeClickAudioClip);
			SoundManager.PlayAudioClip(MetaShopsManager.AudioSourceLoop, MetaShopsManager.DarkUpgradeFillAudioClip);
			fillingTween?.Kill();
			fillingTween = DOTween.To(() => slider.value, delegate(float x)
			{
				slider.value = x;
			}, slider.maxValue, ComputeRemainingTime()).SetEase(fillingTweenEasing);
		}
	}

	public override void OnPointerUp(PointerEventData eventData = null)
	{
		if (base.State == E_State.Unlocked)
		{
			MetaShopsManager.AudioSourceLoop.Stop();
			fillingTween?.Kill();
		}
	}

	public override void OnSliderValueChanged(float value)
	{
		RefreshHandleValueDisplayer(shouldDisplay: true);
		if ((float)ApplicationManager.Application.DamnedSouls - (value - previousSoulsSliderValue) < 0f)
		{
			MetaShopsManager.AudioSourceLoop.Stop();
			fillingTween?.Kill();
			slider.value = previousSoulsSliderValue;
			if (ApplicationManager.Application.DamnedSouls != 0)
			{
				slider.value += ApplicationManager.Application.DamnedSouls;
				ConsumeSouls(ApplicationManager.Application.DamnedSouls);
			}
		}
		else
		{
			if (base.State == E_State.Unlocked)
			{
				ConsumeSouls((uint)(value - previousSoulsSliderValue));
			}
			base.OnSliderValueChanged(value);
			if (slider.value == slider.maxValue && base.State == E_State.Unlocked)
			{
				ActivateMetaUpgradeLineAndModel();
			}
		}
	}

	protected override void ActivateMetaUpgradeLineAndModel()
	{
		SoundManager.PlayAudioClip(MetaShopsManager.DarkUpgradeCompleteAudioClip);
		MetaShopsManager.AudioSourceLoop.Stop();
		fillingTween?.Kill();
		base.ActivateMetaUpgradeLineAndModel();
		TPSingleton<DarkShopManager>.Instance.UpdateActivatedMetasText();
	}

	protected override bool IsFromLightShop()
	{
		return false;
	}

	protected override IEnumerator MoveMetaOnTopOfUnlocked()
	{
		yield return base.MoveMetaOnTopOfUnlocked();
		TPSingleton<DarkShopManager>.Instance.RefreshShop();
		TPSingleton<DarkShopManager>.Instance.RefreshActivatedLines();
	}

	protected override void RefreshShopNewMetaUpgrades()
	{
		TPSingleton<DarkShopManager>.Instance.UpdateNewMetaUpgrades();
	}

	protected override void RefreshSliderValues()
	{
		slider.maxValue = ((base.State != E_State.Unlocked) ? 1u : base.MetaUpgrade.MetaUpgradeDefinition.Price);
		slider.value = ((base.State != E_State.Unlocked) ? 1u : base.MetaUpgrade.InvestedSouls);
		fillerDynamic.fillAmount = slider.value / slider.maxValue;
		sliderFill.fillAmount = fillerDynamic.fillAmount;
		previousSoulsSliderValue = slider.value;
		RefreshHandleValueDisplayer(slider.value != (float)base.MetaUpgrade.MetaUpgradeDefinition.Price);
	}

	protected override void RefreshTexts(bool forceRefresh = false)
	{
		if (forceRefresh || base.StateHasChanged)
		{
			base.RefreshTexts(forceRefresh);
			selectorLabel.text = Localizer.Get("Meta_DarkShop_Selector");
		}
	}

	protected override Animator GetUnlockFxAnimator()
	{
		return TPSingleton<DarkShopManager>.Instance.MetaShopView.FxAnimator;
	}

	protected override RectTransform GetUnlockFxTransform()
	{
		return TPSingleton<DarkShopManager>.Instance.MetaShopView.FxTransform;
	}

	private float ComputeRemainingTime()
	{
		if (slider.value != 0f)
		{
			return Mathf.Clamp((1f - slider.value / slider.maxValue) * fillingTweenDuration, minClampDuration, fillingTweenDuration);
		}
		return fillingTweenDuration;
	}

	private void ConsumeSouls(uint value)
	{
		base.MetaUpgrade.InvestedSouls += value;
		TPSingleton<DarkShopManager>.Instance.SpendDamnedSouls(value);
	}

	private void RefreshHandleValueDisplayer(bool shouldDisplay = false)
	{
		if (!shouldDisplay && boxRect.gameObject.activeSelf)
		{
			boxRect.gameObject.SetActive(value: false);
		}
		else if (!shouldDisplay && !boxRect.gameObject.activeSelf)
		{
			return;
		}
		valueDisplayer.text = $"<sprite name=DamnedSouls>{(slider.maxValue - slider.value).ToString()}";
		boxRect.sizeDelta = new Vector2(valueDisplayer.preferredWidth, boxRect.sizeDelta.y);
	}
}

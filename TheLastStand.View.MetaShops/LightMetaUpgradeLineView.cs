using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Meta;
using TheLastStand.Database;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Meta;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.MetaShops;

public class LightMetaUpgradeLineView : MetaUpgradeLineView
{
	private new static class Constants
	{
		public static class Animation
		{
			public const string ActivationReady = "ActivationReady";

			public const string FxExplode = "LightFX_Explode";

			public const string SliderBackgroundIdle = "BlessingGaugeBackgroundIdle";

			public const string SliderFillAreaIdle = "BlessingGaugeIdle";

			public const string SliderHandleIdle = "BlessingHandleIdle";
		}
	}

	[SerializeField]
	private GameObject fillerBackground;

	[SerializeField]
	protected TextMeshProUGUI selectorLabelGamepad;

	[SerializeField]
	protected GameObject selectorLabelGamepadContainer;

	[SerializeField]
	protected TextMeshProUGUI conditionsTextField;

	[SerializeField]
	protected TextMeshProUGUI conditionsTitleField;

	[SerializeField]
	protected Animator activationReadyHighlightAnimator;

	protected override string SliderBackgroundIdleKey => "BlessingGaugeBackgroundIdle";

	protected override string SliderFillAreaIdleKey => "BlessingGaugeIdle";

	protected override string SliderHandleIdleKey => "BlessingHandleIdle";

	protected override string FxExplodeAnimationLabel => "LightFX_Explode";

	public override void OnPointerDown(PointerEventData eventData = null)
	{
	}

	public override void OnPointerUp(PointerEventData eventData = null)
	{
		if (base.State == E_State.Unlocked && TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Contains(base.MetaUpgrade))
		{
			SoundManager.PlayAudioClip(MetaShopsManager.LightUpgradeClickAudioClip);
			ActivateMetaUpgradeLineAndModel();
		}
	}

	protected override void ActivateMetaUpgradeLineAndModel()
	{
		noSliderLayoutElement.preferredHeight += conditionsTextField.rectTransform.rect.height + conditionsTitleField.rectTransform.rect.height;
		base.ActivateMetaUpgradeLineAndModel();
		TPSingleton<LightShopManager>.Instance.UpdateActivatedMetasText();
	}

	protected override bool IsFromLightShop()
	{
		return true;
	}

	protected override IEnumerator MoveMetaOnTopOfUnlocked()
	{
		yield return base.MoveMetaOnTopOfUnlocked();
		TPSingleton<LightShopManager>.Instance.RefreshShop();
		TPSingleton<LightShopManager>.Instance.RefreshActivatedLines();
	}

	protected override void RefreshMetaIcon()
	{
		base.RefreshMetaIcon();
		bool flag = slider.value == slider.maxValue && slider.maxValue > 0f && base.State == E_State.Unlocked;
		activationReadyHighlightAnimator.gameObject.SetActive(flag);
		if (flag)
		{
			activationReadyHighlightAnimator.Play("ActivationReady", 0, UnityEngine.Random.value);
		}
	}

	protected override void RefreshSelector()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			selectorLabel.gameObject.SetActive(value: false);
			selectorLabelGamepadContainer.gameObject.SetActive(TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Contains(base.MetaUpgrade));
		}
		else
		{
			selectorLabel.gameObject.SetActive(TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Contains(base.MetaUpgrade));
			selectorLabelGamepadContainer.gameObject.SetActive(value: false);
		}
	}

	protected override void RefreshShopNewMetaUpgrades()
	{
		TPSingleton<LightShopManager>.Instance.UpdateNewMetaUpgrades();
	}

	protected override void RefreshSliderValues()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (List<MetaConditionController> condition in base.MetaUpgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Activation))
		{
			foreach (MetaConditionController item in condition)
			{
				if (item.MetaCondition.MetaConditionDefinition.Hidden)
				{
					continue;
				}
				if (item.MetaCondition.MetaConditionDefinition.Occurences > 1)
				{
					num += (float)item.MetaCondition.OccurenceProgression;
					num2 += (float)item.MetaCondition.MetaConditionDefinition.Occurences;
					continue;
				}
				MetaConditionsDatabase.ProgressionDatas progressionValues = item.GetProgressionValues(TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary);
				if (float.TryParse(progressionValues.GoalValueToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					num2 += 1f;
				}
				if (float.TryParse(progressionValues.ProgressionValueToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					num += Mathf.Min(result2, result) / result;
				}
			}
		}
		bool flag = base.State == E_State.Unlocked && !TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Contains(base.MetaUpgrade);
		slider.maxValue = (flag ? num2 : 1f);
		slider.value = (flag ? num : 1f);
		fillerDynamic.fillAmount = slider.value / slider.maxValue;
		sliderFill.fillAmount = fillerDynamic.fillAmount;
		previousSoulsSliderValue = slider.value;
		fillerBackground.gameObject.SetActive(base.State != E_State.Locked);
	}

	protected override void RefreshTexts(bool forceRefresh = false)
	{
		base.RefreshTexts(forceRefresh);
		conditionsTextField.text = string.Empty;
		selectorLabel.text = Localizer.Get("Meta_LightShop_Selector");
		selectorLabelGamepad.text = Localizer.Get("Meta_LightShop_Selector_Gamepad");
		if (base.State == E_State.Locked || base.State == E_State.Activated)
		{
			conditionsTextField.text = string.Empty;
			conditionsTitleField.text = string.Empty;
			return;
		}
		conditionsTitleField.text = Localizer.Get("Meta_Conditions");
		foreach (List<MetaConditionController> condition in base.MetaUpgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Activation))
		{
			foreach (MetaConditionController item in condition)
			{
				if (!item.MetaCondition.MetaConditionDefinition.Hidden)
				{
					TextMeshProUGUI textMeshProUGUI = conditionsTextField;
					textMeshProUGUI.text = textMeshProUGUI.text + "<sprite name=LightShopDot> " + item.GetLocalizedDescription() + "\r\n";
				}
			}
		}
	}

	protected override Animator GetUnlockFxAnimator()
	{
		return TPSingleton<LightShopManager>.Instance.MetaShopView.FxAnimator;
	}

	protected override RectTransform GetUnlockFxTransform()
	{
		return TPSingleton<LightShopManager>.Instance.MetaShopView.FxTransform;
	}
}

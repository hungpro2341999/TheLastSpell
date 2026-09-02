using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class SkillParameterDisplay : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI effectBonusText;

	[SerializeField]
	protected TextMeshProUGUI effectNameText;

	[SerializeField]
	protected TextMeshProUGUI effectValueText;

	[SerializeField]
	protected Color effectValueColor = Color.white;

	[SerializeField]
	protected TextMeshProUGUI operatorSignText;

	[SerializeField]
	protected GameObject separator;

	protected string nameLocalizationKey;

	public virtual void Display(bool show)
	{
		if (separator != null)
		{
			separator.SetActive(show);
		}
		base.gameObject.SetActive(show);
	}

	public float GetVerticalPosition()
	{
		return effectValueText.rectTransform.localPosition.y;
	}

	public float GetPositionAtTheEndOfLine()
	{
		return effectValueText.rectTransform.localPosition.x + effectValueText.rectTransform.sizeDelta.x;
	}

	public virtual void Refresh(string effectName, string effectValue, string overrideSign = "")
	{
		nameLocalizationKey = effectName;
		RefreshName();
		effectValueText.text = effectValue;
		if (operatorSignText != null && !string.IsNullOrEmpty(overrideSign))
		{
			operatorSignText.text = overrideSign;
		}
		if (effectBonusText != null)
		{
			effectBonusText.text = string.Empty;
		}
	}

	public virtual void Refresh(string effectName, string effectValue, Color? valueColor, string overrideSign = "")
	{
		if (valueColor.HasValue)
		{
			effectValueColor = valueColor.Value;
			RefreshColor();
		}
		Refresh(effectName, effectValue, overrideSign);
	}

	public virtual void Refresh(string effectName, string effectValue, int bonusValue, string overrideSign = "")
	{
		Refresh(effectName, effectValue, overrideSign);
		effectBonusText.text = ((bonusValue > 0) ? $"<style=GoodNb>+{bonusValue}</style>" : string.Empty);
	}

	protected virtual void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshName();
		}
	}

	protected virtual void RefreshColor()
	{
		effectValueText.color = effectValueColor;
		if (operatorSignText != null)
		{
			operatorSignText.color = effectValueColor;
		}
	}

	protected virtual void RefreshName()
	{
		effectNameText.text = Localizer.Get(nameLocalizationKey);
	}

	protected virtual void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	protected virtual void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}
}

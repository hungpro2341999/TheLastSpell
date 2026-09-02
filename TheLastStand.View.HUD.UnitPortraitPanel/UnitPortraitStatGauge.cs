using System.Collections;
using TMPro;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitPortraitPanel;

[RequireComponent(typeof(Image))]
public class UnitPortraitStatGauge : MonoBehaviour
{
	[SerializeField]
	[Range(0.01f, 1f)]
	private float gaugeChangeSpeed = 0.1f;

	[SerializeField]
	private Image image;

	[SerializeField]
	private GameObject emptyBarGameObject;

	[SerializeField]
	private float defaultFontSize = 16f;

	[SerializeField]
	private float smallFontSize = 14f;

	[SerializeField]
	private Image statIcon;

	[SerializeField]
	private TextMeshProUGUI statValue;

	private float targetGaugeValue = -1f;

	private bool isHovered;

	public float TargetGaugeValue
	{
		set
		{
			if (targetGaugeValue != value)
			{
				targetGaugeValue = value;
				StartCoroutine(UpdateGaugeCoroutine());
			}
		}
	}

	public void Hover(bool hover)
	{
		statValue.enabled = hover || TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayUnitPortraitAttribute;
		statIcon.enabled = hover;
		isHovered = hover;
	}

	public void Refresh(float unitCurrentStat, float unitCurrentStatTotal)
	{
		if (unitCurrentStat < 1f)
		{
			unitCurrentStat = 0f;
		}
		bool alwaysDisplayMaxStatValue = TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayMaxStatValue;
		statValue.fontSize = (alwaysDisplayMaxStatValue ? smallFontSize : defaultFontSize);
		statValue.text = Mathf.RoundToInt(unitCurrentStat) + (alwaysDisplayMaxStatValue ? ("/" + Mathf.RoundToInt(unitCurrentStatTotal)) : string.Empty);
		TargetGaugeValue = unitCurrentStat / unitCurrentStatTotal;
		emptyBarGameObject.SetActive(targetGaugeValue < Mathf.Epsilon);
		statValue.enabled = isHovered || TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayUnitPortraitAttribute;
	}

	private IEnumerator UpdateGaugeCoroutine()
	{
		while (Mathf.Abs(image.fillAmount - targetGaugeValue) > Mathf.Epsilon)
		{
			image.fillAmount = Mathf.Lerp(image.fillAmount, targetGaugeValue, gaugeChangeSpeed);
			yield return SharedYields.WaitForEndOfFrame;
		}
	}
}

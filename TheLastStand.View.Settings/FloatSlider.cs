using TMPro;
using TheLastStand.Framework.UI;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class FloatSlider : MonoBehaviour
{
	[SerializeField]
	protected BetterSlider slider;

	[SerializeField]
	protected float multiplier = 100f;

	[SerializeField]
	private TextMeshProUGUI progressText;

	private bool hasBeenInitialized;

	public float Value => slider.value;

	public virtual void OnValueSliderChange()
	{
		if (hasBeenInitialized)
		{
			RefreshProgressText();
		}
	}

	public virtual void RefreshValue(float value, bool applyMultiplier = false)
	{
		hasBeenInitialized = true;
		slider.value = value * (applyMultiplier ? multiplier : 1f);
		RefreshProgressText();
	}

	private void RefreshProgressText()
	{
		progressText.text = $"{Mathf.RoundToInt(Value * multiplier)}%";
	}
}

using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class UISizePanel : MonoBehaviour
{
	[SerializeField]
	private UIScalerSlider uiSizeSlider;

	[SerializeField]
	private RectTransform windowSizeReference;

	private Vector2 referenceSize = Vector2.zero;

	public void Refresh()
	{
		uiSizeSlider.RefreshValue(TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale, applyMultiplier: true);
	}

	private void Awake()
	{
		referenceSize = windowSizeReference.rect.size;
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += delegate(Resolution res)
			{
				uiSizeSlider.AutoSetValueRange(res, referenceSize);
			};
		}
		Refresh();
	}

	private void Start()
	{
		uiSizeSlider.AutoSetValueRange(TPSingleton<SettingsManager>.Instance.Settings.Resolution, referenceSize);
	}

	private void OnDestroy()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= delegate(Resolution res)
			{
				uiSizeSlider.AutoSetValueRange(res, referenceSize);
			};
		}
	}
}

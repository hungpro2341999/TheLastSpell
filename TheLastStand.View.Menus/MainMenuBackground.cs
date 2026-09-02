using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Menus;

public class MainMenuBackground : MonoBehaviour
{
	[SerializeField]
	private Image background;

	private Vector2 nativeSize;

	private void OnResolutionChanged(Resolution resolution)
	{
		if ((float)resolution.height >= 1440f)
		{
			background.rectTransform.sizeDelta = nativeSize * 4f;
		}
		else if ((float)resolution.height >= 1080f)
		{
			background.rectTransform.sizeDelta = nativeSize * 3f;
		}
		else
		{
			background.rectTransform.sizeDelta = nativeSize * 2f;
		}
	}

	private void OnDestroy()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChanged;
		}
	}

	private void Start()
	{
		nativeSize = background.sprite.rect.size;
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChanged;
		OnResolutionChanged(TPSingleton<SettingsManager>.Instance.Settings.Resolution);
	}
}

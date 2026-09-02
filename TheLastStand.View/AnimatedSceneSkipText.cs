using Rewired;
using TMPro;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View;

public class AnimatedSceneSkipText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI keyboardText;

	[SerializeField]
	private TextMeshProUGUI controllerText;

	[SerializeField]
	private string keyboardLocalizationKey = string.Empty;

	[SerializeField]
	private string controllerLocalizationKey = string.Empty;

	private void Awake()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		OnLastActiveControllerChanged(TheLastStand.Manager.InputManager.GetLastControllerType());
	}

	private void OnDestroy()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		keyboardText.gameObject.SetActive(controllerType != ControllerType.Joystick);
		controllerText.gameObject.SetActive(controllerType == ControllerType.Joystick);
		if (controllerType == ControllerType.Joystick)
		{
			controllerText.text = Localizer.Get(controllerLocalizationKey);
		}
		else
		{
			keyboardText.text = Localizer.Get(keyboardLocalizationKey);
		}
	}
}

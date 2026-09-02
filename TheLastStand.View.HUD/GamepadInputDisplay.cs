using Rewired;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

[DisallowMultipleComponent]
public class GamepadInputDisplay : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private GamepadButtonsSet gamepadButtonsSet;

	public void Display(bool show)
	{
		image.enabled = show;
	}

	private void Reset()
	{
		if (image == null)
		{
			image = GetComponent<Image>();
		}
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Joystick:
			UpdateIcon(gamepadButtonsSet);
			Display(show: true);
			break;
		default:
			Display(show: false);
			break;
		}
	}

	private void OnInputDeviceTypeChanged(SettingsManager.E_InputDeviceType inputDeviceType)
	{
		switch (inputDeviceType)
		{
		case SettingsManager.E_InputDeviceType.MouseKeyboard:
			Display(show: false);
			break;
		case SettingsManager.E_InputDeviceType.Controller:
			Display(show: true);
			break;
		case SettingsManager.E_InputDeviceType.Auto:
			break;
		}
	}

	private void OnEnable()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		TPSingleton<SettingsManager>.Instance.OnInputDeviceTypeChangeEvent += OnInputDeviceTypeChanged;
		OnLastActiveControllerChanged(TheLastStand.Manager.InputManager.GetLastControllerType());
	}

	private void OnDisable()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnInputDeviceTypeChangeEvent -= OnInputDeviceTypeChanged;
		}
	}

	private void UpdateIcon(GamepadButtonsSet buttonsSet)
	{
		Sprite icon = buttonsSet.GetIcon();
		image.sprite = icon;
		image.SetNativeSize();
	}
}

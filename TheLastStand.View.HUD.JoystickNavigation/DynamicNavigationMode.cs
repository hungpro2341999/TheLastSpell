using Rewired;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.JoystickNavigation;

public class DynamicNavigationMode : MonoBehaviour
{
	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private Navigation.Mode joystickNavigationMode = Navigation.Mode.Explicit;

	public void RefreshNavigationMode(ControllerType controllerType)
	{
		RefreshNavigationMode(controllerType == ControllerType.Joystick);
	}

	public void RefreshNavigationMode(bool usingJoystick)
	{
		Navigation navigation = selectable.navigation;
		navigation.mode = (usingJoystick ? joystickNavigationMode : Navigation.Mode.None);
		selectable.navigation = navigation;
	}

	private void OnEnable()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += RefreshNavigationMode;
		RefreshNavigationMode(TheLastStand.Manager.InputManager.IsLastControllerJoystick);
	}

	private void OnDisable()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= RefreshNavigationMode;
	}

	private void Reset()
	{
		selectable = GetComponent<Selectable>();
	}
}

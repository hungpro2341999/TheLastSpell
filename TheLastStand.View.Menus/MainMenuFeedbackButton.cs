using Rewired;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Menus;

public class MainMenuFeedbackButton : MonoBehaviour
{
	[SerializeField]
	private Button button;

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		button.interactable = controllerType != ControllerType.Joystick;
	}

	private void Awake()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
	}

	private void OnDestroy()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}
}

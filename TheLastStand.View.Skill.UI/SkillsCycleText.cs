using Rewired;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class SkillsCycleText : MonoBehaviour
{
	[SerializeField]
	private GameObject keyboardTextContainer;

	[SerializeField]
	private GameObject controllerTextContainer;

	private void OnEnable()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		OnLastActiveControllerChanged(TheLastStand.Manager.InputManager.GetLastControllerType());
	}

	private void OnDisable()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		controllerTextContainer.SetActive(controllerType == ControllerType.Joystick);
		keyboardTextContainer.SetActive(controllerType != ControllerType.Joystick);
	}
}

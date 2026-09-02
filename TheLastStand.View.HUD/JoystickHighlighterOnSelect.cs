using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace TheLastStand.View.HUD;

public class JoystickHighlighterOnSelect : JoystickHighlighter, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private UnityEvent onSelect = new UnityEvent();

	[SerializeField]
	private UnityEvent onDeselect = new UnityEvent();

	public void OnSelect(BaseEventData eventData)
	{
		if (InputManager.IsLastControllerJoystick && TPSingleton<SettingsManager>.Instance.Settings.InputDeviceType != SettingsManager.E_InputDeviceType.MouseKeyboard)
		{
			onSelect?.Invoke();
			OnHighlight();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		onDeselect?.Invoke();
		OnUnhighlight();
	}
}

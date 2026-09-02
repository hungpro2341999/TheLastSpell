using Rewired;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class JoystickSelectable : Selectable, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private UnityEvent onSelect;

	[SerializeField]
	private UnityEvent onDeselect;

	[SerializeField]
	private BoolEvent onTooltipsToggled;

	[SerializeField]
	private TooltipDisplayer tooltipDisplayer;

	public TooltipDisplayer TooltipDisplayer => tooltipDisplayer;

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		onSelect?.Invoke();
		if (tooltipDisplayer != null && TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			tooltipDisplayer.DisplayTooltip();
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		if (tooltipDisplayer != null && tooltipDisplayer.Displayed)
		{
			tooltipDisplayer.HideTooltip();
		}
		base.OnDeselect(eventData);
		onDeselect?.Invoke();
	}

	public void AddListenerOnSelect(UnityAction unityAction)
	{
		onSelect.AddListener(unityAction);
	}

	public void AddListenerOnDeselect(UnityAction unityAction)
	{
		onDeselect.AddListener(unityAction);
	}

	public void RemoveListenerOnSelect(UnityAction unityAction)
	{
		onSelect.RemoveListener(unityAction);
	}

	public void RemoveListenerOnDeselect(UnityAction unityAction)
	{
		onDeselect.RemoveListener(unityAction);
	}

	public void ClearEvents()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearEvents();
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (this == null || EventSystem.current.currentSelectedGameObject != base.gameObject)
		{
			return;
		}
		if (tooltipDisplayer != null)
		{
			if (showTooltips)
			{
				tooltipDisplayer.DisplayTooltip();
			}
			else
			{
				tooltipDisplayer.HideTooltip();
			}
		}
		onTooltipsToggled?.Invoke(showTooltips);
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Joystick:
			this.SetMode(Navigation.Mode.Explicit);
			break;
		default:
			this.SetMode(Navigation.Mode.None);
			break;
		}
	}
}

using System;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.HUD.UnitManagement;

public abstract class UnitModifiersIconDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private JoystickSelectable joystickSelectable;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public event Action<UnitModifiersIconDisplay> Hovered;

	public event Action<UnitModifiersIconDisplay> Unhovered;

	public virtual void Display()
	{
		base.gameObject.SetActive(value: true);
	}

	public virtual void Hide()
	{
		OnPointerExit(null);
		base.gameObject.SetActive(value: false);
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		this.Hovered?.Invoke(this);
		DiplayTooltip();
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		this.Unhovered?.Invoke(this);
		HideTooltip();
	}

	public void OnJoystickSelect()
	{
		this.Hovered?.Invoke(this);
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			DiplayTooltip();
		}
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnTooltipsToggled(bool showTooltips)
	{
		if (showTooltips)
		{
			DiplayTooltip();
		}
		else
		{
			HideTooltip();
		}
	}

	protected abstract void DiplayTooltip();

	protected abstract void HideTooltip();

	private void Awake()
	{
		TileObjectSelectionManager.OnUnitSelectionChange += OnNewUnitSelected;
	}

	private void OnDestroy()
	{
		TileObjectSelectionManager.OnUnitSelectionChange -= OnNewUnitSelected;
	}

	private void OnNewUnitSelected()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			OnPointerExit(null);
		}
	}
}

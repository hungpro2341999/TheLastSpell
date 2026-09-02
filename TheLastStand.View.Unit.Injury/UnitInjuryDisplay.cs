using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuryDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private RectTransform injuryRect;

	[SerializeField]
	private GameObject raycaster;

	[SerializeField]
	private Image injuryImage;

	[SerializeField]
	private Sprite injuryOn;

	[SerializeField]
	private Sprite injuryOff;

	[SerializeField]
	private Sprite injuryOnHover;

	[SerializeField]
	private Sprite injuryOffHover;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	[SerializeField]
	private FollowElement.FollowDatas tooltipFollowData;

	private InjuryDefinition injuryDefinition;

	private int injuryIndex;

	private bool isOn;

	private bool isJoystickSelected;

	private TheLastStand.Model.Unit.Unit currentUnit;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public void Refresh(bool isOn, float xPos, InjuryDefinition injuryDefinition, int injuryIndex, TheLastStand.Model.Unit.Unit unit)
	{
		currentUnit = unit;
		this.isOn = isOn;
		injuryImage.sprite = (isOn ? injuryOn : injuryOff);
		injuryRect.anchoredPosition = new Vector2(xPos, injuryRect.anchoredPosition.y);
		this.injuryDefinition = injuryDefinition;
		this.injuryIndex = injuryIndex;
	}

	public void ToggleRaycaster(bool state)
	{
		raycaster.SetActive(state);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		injuryImage.sprite = (isOn ? injuryOnHover : injuryOffHover);
		DisplayTooltip();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		injuryImage.sprite = (isOn ? injuryOn : injuryOff);
		HideTooltip();
	}

	public void OnJoystickSelect()
	{
		isJoystickSelected = true;
		injuryImage.sprite = (isOn ? injuryOnHover : injuryOffHover);
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			FollowElement followElement = UIManager.InjuryTooltip.FollowElement;
			followElement.FollowElementDatas.FollowTarget = tooltipFollowData.FollowTarget;
			followElement.FollowElementDatas.Offset = tooltipFollowData.Offset;
			DisplayTooltip();
		}
	}

	public void OnJoystickDeselect()
	{
		isJoystickSelected = false;
		FollowElement followElement = UIManager.InjuryTooltip.FollowElement;
		followElement.FollowElementDatas.FollowTarget = null;
		followElement.RestoreFollowDatasOffset();
		OnPointerExit(null);
	}

	private void Awake()
	{
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	private void OnDestroy()
	{
		JoystickSelectable.ClearEvents();
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnTooltipsToggled(bool state)
	{
		if (isJoystickSelected)
		{
			if (state)
			{
				FollowElement followElement = UIManager.InjuryTooltip.FollowElement;
				followElement.FollowElementDatas.FollowTarget = tooltipFollowData.FollowTarget;
				followElement.FollowElementDatas.Offset = tooltipFollowData.Offset;
				DisplayTooltip();
			}
			else
			{
				HideTooltip();
			}
		}
	}

	private void DisplayTooltip()
	{
		UIManager.InjuryTooltip.SetContent(injuryDefinition, injuryIndex, currentUnit);
		UIManager.InjuryTooltip.Display();
	}

	private void HideTooltip()
	{
		UIManager.InjuryTooltip.Hide();
	}
}

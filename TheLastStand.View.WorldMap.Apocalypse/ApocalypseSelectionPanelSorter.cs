using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseSelectionPanelSorter : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Sprite iconSpriteOn;

	[SerializeField]
	private Sprite iconSpriteOff;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Sprite backgroundSpriteOn;

	[SerializeField]
	private Sprite backgroundSpriteOff;

	private bool toggled;

	public event Action<bool> OnToggled;

	public void OnClick()
	{
		Toggle(!toggled);
		this.OnToggled?.Invoke(toggled);
	}

	private void Toggle(bool toggle)
	{
		toggled = toggle;
		ToggleView(toggled);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ToggleBackgroundHover(toggle: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ToggleBackgroundHover(toggle: false);
	}

	public void OnSelect()
	{
		OnPointerEnter(null);
	}

	public void OnUnselect()
	{
		OnPointerExit(null);
	}

	private void ToggleView(bool toggle)
	{
		iconImage.sprite = (toggle ? iconSpriteOn : iconSpriteOff);
	}

	private void ToggleBackgroundHover(bool toggle)
	{
		backgroundImage.sprite = (toggle ? backgroundSpriteOn : backgroundSpriteOff);
	}
}

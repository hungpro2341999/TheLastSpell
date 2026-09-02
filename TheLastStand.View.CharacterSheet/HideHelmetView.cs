using System;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.CharacterSheet;

public class HideHelmetView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler
{
	[Serializable]
	public struct StateSprites
	{
		public Sprite NormalSprite;

		public Sprite HoveredSprite;
	}

	[SerializeField]
	private StateSprites shownHelmetSprites;

	[SerializeField]
	private StateSprites hiddenHelmetSprites;

	[SerializeField]
	private Image image;

	private PlayableUnit playableUnit;

	public void OnJoystickSelect()
	{
		OnPointerEnter(null);
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnSubmit(BaseEventData eventData)
	{
		OnPointerDown(null);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		playableUnit.HelmetDisplayed = !playableUnit.HelmetDisplayed;
		UpdateHoveredSprite();
		playableUnit.PlayableUnitView.ToggleHelmetDisplay();
		TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar(playableUnit);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		UpdateHoveredSprite();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UpdateNormalSprite();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void Refresh(PlayableUnit playableUnit)
	{
		if (playableUnit != null)
		{
			this.playableUnit = playableUnit;
			if (InputManager.IsLastControllerJoystick && EventSystem.current.currentSelectedGameObject == base.gameObject)
			{
				UpdateHoveredSprite();
			}
			else
			{
				UpdateNormalSprite();
			}
		}
	}

	private void UpdateHoveredSprite()
	{
		image.sprite = (playableUnit.HelmetDisplayed ? shownHelmetSprites.HoveredSprite : hiddenHelmetSprites.HoveredSprite);
	}

	private void UpdateNormalSprite()
	{
		image.sprite = (playableUnit.HelmetDisplayed ? shownHelmetSprites.NormalSprite : hiddenHelmetSprites.NormalSprite);
	}
}

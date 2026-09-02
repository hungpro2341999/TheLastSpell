using System.Collections;
using DG.Tweening;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public abstract class BuildingCapacityPanel : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IJoystickSkillConfirmHandler, IJoystickSelect
{
	[SerializeField]
	[FormerlySerializedAs("buildingSkillRect")]
	private RectTransform buildingCapacityRect;

	[SerializeField]
	[FormerlySerializedAs("buildingSkillsPanel")]
	protected BuildingCapacitiesPanel buildingCapacitiesPanel;

	[SerializeField]
	protected BetterButton button;

	[SerializeField]
	protected BetterButton confirmButton;

	[SerializeField]
	protected Canvas confirmButtonCanvas;

	[SerializeField]
	protected Animator selectorAnimator;

	[SerializeField]
	protected Canvas selectorCanvas;

	[SerializeField]
	protected Transform tooltipAnchor;

	[SerializeField]
	protected JoystickHighlighter joystickHighlighter;

	private bool confirmedThisFrame;

	private Tween confirmButtonTween;

	public RectTransform BuildingCapacityRect => buildingCapacityRect;

	public bool IsConfirmSelected { get; set; }

	public virtual void DisplayTooltip(bool show)
	{
		if (show)
		{
			OnPointerEnter(null);
		}
		else
		{
			OnPointerExit(null);
		}
	}

	public abstract void OnSkillPanelHovered(bool hover);

	public abstract void Refresh();

	public void DeselectConfirmButton(bool deselectAll)
	{
		if (IsConfirmSelected)
		{
			if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips && deselectAll)
			{
				OnPointerExit(null);
			}
			IsConfirmSelected = false;
			EventSystem.current.SetSelectedGameObject(GetButton());
			buildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
			if (joystickHighlighter != null)
			{
				joystickHighlighter.OnHighlight();
			}
		}
	}

	public void DisplaySelector(bool display)
	{
		selectorCanvas.enabled = display;
		selectorAnimator.enabled = display;
	}

	public bool IsDisplayed()
	{
		return BuildingCapacityRect.gameObject.activeSelf;
	}

	public void OnDisplayTooltip(bool display)
	{
		if (!InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnBuildingSkill)
		{
			DisplayTooltip(display);
		}
	}

	public virtual void OnSkillHover(bool select)
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips || InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnBuildingSkill)
		{
			if (select)
			{
				OnPointerEnter(null);
			}
			else
			{
				OnPointerExit(null);
			}
		}
		EventSystem.current.SetSelectedGameObject(select ? GetButton() : null);
		if (select)
		{
			GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.OnCapacityHovered(this);
			if (joystickHighlighter != null)
			{
				joystickHighlighter.OnHighlight();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnSkillPanelHovered(hover: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnSkillPanelHovered(hover: false);
	}

	public void Select(bool select)
	{
		DisplaySelector(select);
		if (!(confirmButton != null))
		{
			return;
		}
		confirmButtonTween?.Kill();
		confirmButtonTween = confirmButton.image.rectTransform.DOAnchorPosY(select ? 0f : (-87f), 0.2f, snapping: true);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		if (select)
		{
			confirmButtonTween.OnComplete(delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			});
			confirmButton.gameObject.SetActive(value: true);
			confirmButton.interactable = true;
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<BuildingManager>.Instance.StartCoroutine(ToggleConfirmCanvasCoroutine());
			}
		}
		else
		{
			confirmButtonTween.OnComplete(delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
				confirmButton.gameObject.SetActive(value: false);
			});
		}
	}

	public virtual void SelectConfirmButton()
	{
		GameObject gameObject = GetConfirmButton();
		if (!(gameObject == null) && button.interactable && !IsConfirmSelected && !confirmedThisFrame)
		{
			IsConfirmSelected = true;
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
	}

	protected void OnConfirmButtonClick()
	{
		TPSingleton<BuildingManager>.Instance.StartCoroutine(ConfirmedCoroutine());
	}

	private GameObject GetButton()
	{
		return button.gameObject;
	}

	private GameObject GetConfirmButton()
	{
		if (!(confirmButton != null))
		{
			return null;
		}
		return confirmButton.gameObject;
	}

	private IEnumerator ToggleConfirmCanvasCoroutine()
	{
		if (!(confirmButtonCanvas == null))
		{
			confirmButtonCanvas.sortingOrder++;
			yield return null;
			confirmButtonCanvas.sortingOrder--;
		}
	}

	private IEnumerator ConfirmedCoroutine()
	{
		confirmedThisFrame = true;
		yield return null;
		confirmedThisFrame = false;
	}
}

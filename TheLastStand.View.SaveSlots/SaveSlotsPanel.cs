using Rewired;
using TPLib;
using TPLib.UI;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.Menus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.SaveSlots;

public class SaveSlotsPanel : TPSingleton<SaveSlotsPanel>, IOverlayUser
{
	public enum E_State
	{
		Closed,
		Opened
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private SaveSlotBox[] saveSlotBoxes;

	[SerializeField]
	private JoystickSelectableDynamic[] saveSlotJoystickSelectables;

	[SerializeField]
	private Scrollbar panelScrollbar;

	[SerializeField]
	private RectTransform panelViewport;

	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private GameObject defaultJoystickSelectedObject;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float scrollBarJoystickStepValue = 0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float scrollBarJoystickMargin;

	public GraphicRaycaster GraphicRaycaster => graphicRaycaster;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public E_State PopupState { get; private set; }

	public GameObject LastSelectedGameObject { get; set; }

	public JoystickSelectableDynamic[] SaveSlotJoystickSelectables => saveSlotJoystickSelectables;

	public void Close()
	{
		if (PopupState != E_State.Closed)
		{
			canvas.enabled = false;
			CameraView.AttenuateWorldForPopupFocus(null);
			ApplicationManager.Application.ApplicationController.BackToPreviousState();
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
			{
				TPSingleton<MainMenuView>.Instance.JoystickSelectSaveSlotsToggle();
			}
			PopupState = E_State.Closed;
		}
	}

	public void Open()
	{
		if (PopupState != E_State.Opened)
		{
			canvas.enabled = true;
			CameraView.AttenuateWorldForPopupFocus(this);
			TPSingleton<SaveSlotsPanel>.Instance.GraphicRaycaster.enabled = true;
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
			{
				JoystickSelectDefault();
			}
			PopupState = E_State.Opened;
		}
	}

	public void JoystickSelectDefault()
	{
		EventSystem.current.SetSelectedGameObject(defaultJoystickSelectedObject);
	}

	public void OnSaveSlotJoystickSelect(RectTransform source)
	{
		GUIHelpers.AdjustHorizontalScrollViewToFocusedItem(source, panelViewport, panelScrollbar, scrollBarJoystickStepValue, scrollBarJoystickMargin);
	}

	public void Refresh()
	{
		for (int i = 0; i < saveSlotBoxes.Length && i < 5; i++)
		{
			saveSlotBoxes[i].ChangeLinkedProfileIndex(i);
			saveSlotBoxes[i].Refresh(TPSingleton<SaveManager>.Instance.PreloadedAppSaves[i], TPSingleton<SaveManager>.Instance.PreloadedGameSaves[i]);
		}
	}

	public void OnLeftButtonClick()
	{
		panelScrollbar.value = Mathf.Clamp01(panelScrollbar.value - scrollButtonsSensitivity);
	}

	public void OnRightButtonClick()
	{
		panelScrollbar.value = Mathf.Clamp01(panelScrollbar.value + scrollButtonsSensitivity);
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		if (PopupState != E_State.Opened || GenericConsent.IsOpen)
		{
			return;
		}
		if (controllerType == ControllerType.Joystick)
		{
			if (EventSystem.current.currentSelectedGameObject == null)
			{
				EventSystem.current.SetSelectedGameObject(defaultJoystickSelectedObject);
			}
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
	}

	private void Update()
	{
		if (TPSingleton<TheLastStand.Manager.InputManager>.Exist() && TheLastStand.Manager.InputManager.GetButtonDown(23) && PopupState == E_State.Opened && !GenericConsent.IsOpen && !GenericBlockingPopup.IsOpen && !GenericPopUp.IsOpen)
		{
			Close();
		}
	}

	private void Start()
	{
		for (int i = 0; i < saveSlotBoxes.Length; i++)
		{
			saveSlotBoxes[i].gameObject.SetActive(i < 5);
		}
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
	}

	private void OnDestroy()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}
}

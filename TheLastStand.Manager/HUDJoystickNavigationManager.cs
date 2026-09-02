using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TPLib;
using TPLib.Yield;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.Manager;

public class HUDJoystickNavigationManager : Manager<HUDJoystickNavigationManager>
{
	[SerializeField]
	private HUDJoystickTarget firstHUDJoystickTarget;

	[SerializeField]
	private JoystickHighlight joystickHighlight;

	private Vector2 previousPanelsNavigationInput;

	private HUDJoystickTarget currentPanel;

	private GameObject previousSelection;

	[HideInInspector]
	public bool SlotSelectionToggleThisFrame;

	[HideInInspector]
	public InventorySlot InventorySlotToPlace;

	private Tween highlightPositionTween;

	private Tween highlightSizeTween;

	public bool HUDNavigationOn { get; private set; }

	public bool ShowTooltips { get; private set; }

	public Dictionary<Selectable, HUDJoystickTarget> SelectablesContainerHUDTargets { get; } = new Dictionary<Selectable, HUDJoystickTarget>();

	public JoystickHighlight JoystickHighlight => joystickHighlight;

	public static event Action<bool> HUDNavigationToggled;

	public static event Action<bool> TooltipsToggled;

	public bool CanOpenHUDNavigationMode()
	{
		if (ApplicationManager.Application.State.GetName() != "Game")
		{
			return false;
		}
		return TPSingleton<GameManager>.Instance.Game.State switch
		{
			Game.E_State.Management => TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits, 
			Game.E_State.Construction => true, 
			_ => false, 
		};
	}

	public bool CanExitHUDNavigationMode()
	{
		if (ApplicationManager.Application.State.GetName() == "Game")
		{
			return TPSingleton<GameManager>.Instance.Game.State switch
			{
				Game.E_State.Management => true, 
				Game.E_State.Construction => true, 
				_ => false, 
			};
		}
		if (ApplicationManager.Application.State.GetName() == "Settings" || ApplicationManager.Application.State.GetName() == "GameLobby")
		{
			return false;
		}
		return true;
	}

	public void OpenHUDNavigationMode(bool selectDefaultPanel = true)
	{
		if (!HUDNavigationOn)
		{
			if (selectDefaultPanel)
			{
				SelectHUDDefaultPanel();
			}
			HUDNavigationOn = true;
			HUDJoystickNavigationManager.HUDNavigationToggled?.Invoke(HUDNavigationOn);
		}
	}

	public bool ExitHUDNavigationMode()
	{
		if (!HUDNavigationOn)
		{
			return false;
		}
		if (currentPanel != null)
		{
			currentPanel.RaiseDeselectEvent();
		}
		EventSystem.current.SetSelectedGameObject(null);
		JoystickHighlight.Display(state: false);
		HUDNavigationOn = false;
		HUDJoystickNavigationManager.HUDNavigationToggled?.Invoke(HUDNavigationOn);
		return true;
	}

	public void SelectPanel(HUDJoystickTarget.SelectionInfo selectionInfo, bool updateSelection = true)
	{
		if (currentPanel != null)
		{
			currentPanel.RaiseDeselectEvent();
		}
		currentPanel = selectionInfo.HUDTarget;
		currentPanel.RaiseSelectEvent();
		if (updateSelection && selectionInfo.Selectable != null)
		{
			EventSystem.current.SetSelectedGameObject(selectionInfo.Selectable.gameObject);
		}
	}

	public void SelectHUDDefaultPanel()
	{
		SelectPanel(firstHUDJoystickTarget.GetSelectionInfo());
	}

	public void RegisterSelectable(Selectable selectable, HUDJoystickTarget container)
	{
		HUDJoystickTarget value;
		if (selectable == null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.LogError("Trying to register a null selectable on " + container.transform.name + "! Aborting.");
		}
		else if (SelectablesContainerHUDTargets.TryGetValue(selectable, out value))
		{
			if (container != value)
			{
				LogWarning("Selectable " + selectable.transform.name + " has already been registered with a different container (trying to register using " + container.transform.name + ", already registered with " + value.transform.name + ").");
			}
		}
		else
		{
			SelectablesContainerHUDTargets.Add(selectable, container);
		}
	}

	public void UnregisterSelectable(Selectable selectable)
	{
		if (SelectablesContainerHUDTargets.ContainsKey(selectable))
		{
			SelectablesContainerHUDTargets.Remove(selectable);
		}
	}

	public void OnPopupExitToWorld()
	{
		if (InputManager.JoystickConfig.HUDNavigation.StayInHUDOnPopupExit && (ApplicationManager.Application.State.GetName() != "Game" || TPSingleton<GameManager>.Instance.Game.State != Game.E_State.PlaceUnit))
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectHUDDefaultPanel();
		}
		else
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		}
	}

	private void UpdatePanelForCurrentSelection()
	{
		if (!(currentPanel == null))
		{
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			if (currentSelectedGameObject != null && previousSelection != currentSelectedGameObject && InventorySlotToPlace == null && currentSelectedGameObject.TryGetComponent<Selectable>(out var component) && SelectablesContainerHUDTargets.TryGetValue(component, out var value) && value != currentPanel)
			{
				currentPanel.RaiseDeselectEvent();
				currentPanel = value;
				currentPanel.RaiseSelectEvent();
			}
		}
	}

	private void UpdateHUDNavigation()
	{
		if (ApplicationManager.Application.State.GetName() == "Game" && (InventorySlotToPlace != null || TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.IsInRerollMode))
		{
			return;
		}
		Vector2 vector = new Vector2(InputManager.GetAxis(85), InputManager.GetAxis(86));
		if (previousPanelsNavigationInput.magnitude <= InputManager.JoystickConfig.DefaultDeadZone && vector.magnitude > InputManager.JoystickConfig.DefaultDeadZone)
		{
			HUDJoystickTarget hUDJoystickTarget = ((currentPanel != null) ? currentPanel.GetNextPanelForDirection(vector) : firstHUDJoystickTarget);
			if (hUDJoystickTarget != null)
			{
				HUDJoystickTarget.SelectionInfo selectionInfo = hUDJoystickTarget.GetSelectionInfo(vector);
				if (selectionInfo.HUDTarget != currentPanel)
				{
					SelectPanel(selectionInfo);
				}
			}
		}
		previousPanelsNavigationInput = vector;
	}

	protected override void Awake()
	{
		base.Awake();
		if (firstHUDJoystickTarget == null && ApplicationManager.Application.State.GetName() == "Game")
		{
			LogError("Missing firstHUDJoystickTarget reference! Trying to get it dynamically even though it may be a wrong panel!", base.gameObject);
			firstHUDJoystickTarget = UnityEngine.Object.FindObjectOfType<HUDJoystickTarget>();
		}
		if (InputManager.JoystickConfig.HUDNavigation.TooltipsToggledInit)
		{
			ToggleTooltips();
		}
	}

	private void ToggleTooltips()
	{
		ShowTooltips = !ShowTooltips;
		HUDJoystickNavigationManager.TooltipsToggled?.Invoke(ShowTooltips);
	}

	public IEnumerator ToggleSlotSelectionCoroutine()
	{
		SlotSelectionToggleThisFrame = true;
		yield return SharedYields.WaitForEndOfFrame;
		SlotSelectionToggleThisFrame = false;
	}

	private void Update()
	{
		bool flag = ApplicationManager.Application.State.GetName() != "WorldMap";
		if ((InputManager.GetButtonDown(84) && flag && (InputManager.JoystickConfig.HUDNavigation.CanLeaveHUDUsingEnterInput || !HUDNavigationOn)) || (InputManager.GetButtonDown(80) && HUDNavigationOn))
		{
			if (!HUDNavigationOn)
			{
				if (CanOpenHUDNavigationMode())
				{
					OpenHUDNavigationMode();
				}
			}
			else if (CanExitHUDNavigationMode())
			{
				ExitHUDNavigationMode();
			}
		}
		if (HUDNavigationOn)
		{
			UpdatePanelForCurrentSelection();
			UpdateHUDNavigation();
			previousSelection = EventSystem.current.currentSelectedGameObject;
		}
		if (InputManager.GetButtonDown(87))
		{
			ToggleTooltips();
		}
	}
}

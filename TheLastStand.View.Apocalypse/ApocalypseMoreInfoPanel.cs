using System;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using TPLib;
using TPLib.UI;
using TheLastStand.Controller;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using TheLastStand.View.WorldMap;
using TheLastStand.View.WorldMap.Apocalypse;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Apocalypse;

public class ApocalypseMoreInfoPanel : TPSingleton<ApocalypseMoreInfoPanel>, IOverlayUser
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GameObject overlay;

	[SerializeField]
	private ApocalypseHeader apocalypseHeader;

	[SerializeField]
	private ApocalypseLevelView apocalypseLevelView;

	[SerializeField]
	private ApocalypseMoreInfoPanelModifierDisplay modifierDisplayPrefab;

	[SerializeField]
	private RectTransform modifiersDisplayContainer;

	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	private RectTransform scrollViewport;

	[SerializeField]
	private float scrollSensitivity = -0.2f;

	[SerializeField]
	[Min(0f)]
	private float snapshotTransitionDurationIn = 0.1f;

	[SerializeField]
	[Min(0f)]
	private float snapshotTransitionDurationOut = 0.4f;

	[SerializeField]
	private LayoutNavigationInitializer scrollViewNavigationInitializer;

	[SerializeField]
	private HUDJoystickDynamicTarget joystickDynamicTarget;

	[SerializeField]
	private HUDJoystickSimpleTarget headerJoystickSimpleTarget;

	[SerializeField]
	private HUDJoystickSimpleTarget scrollViewJoystickSimpleTarget;

	private List<ApocalypseModifierStepDefinition> apocalypseModifierStepDefinitions;

	private List<ApocalypseMoreInfoPanelModifierDisplay> apocalypseModifierDisplays = new List<ApocalypseMoreInfoPanelModifierDisplay>();

	private Game.E_State savedGameState;

	public bool CanClosePanel => true;

	public bool Displayed { get; protected set; }

	public bool OpeningOrClosing { get; private set; }

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public static event Action OnPanelClosed;

	public void OnScrollbarTopButtonClick()
	{
		scrollbar.value = Mathf.Clamp01(scrollbar.value - scrollSensitivity);
	}

	public void OnScrollbarBotButtonClick()
	{
		scrollbar.value = Mathf.Clamp01(scrollbar.value + scrollSensitivity);
	}

	public void AdjustScrollView(RectTransform modifierDisplay)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(modifierDisplay, scrollViewport, scrollbar, 0.02f, 0f, 0.1f);
	}

	public void Open()
	{
		if (!Displayed)
		{
			if (TPSingleton<GameManager>.Exist())
			{
				savedGameState = TPSingleton<GameManager>.Instance.Game.State;
				GameController.SetState(Game.E_State.ApocalypseMoreInfo);
				TPSingleton<SoundManager>.Instance.TransitionToSettingsSnapshot(snapshotTransitionDurationIn);
			}
			Display(mustDisplay: true);
		}
	}

	public void Close()
	{
		if (Displayed)
		{
			Display(mustDisplay: false);
			ApocalypseMoreInfoPanel.OnPanelClosed();
			if (TPSingleton<GameManager>.Exist())
			{
				TPSingleton<SoundManager>.Instance.TransitionToNormalSnapshot(snapshotTransitionDurationOut);
				GameController.SetState(savedGameState);
			}
		}
	}

	private void Display(bool mustDisplay)
	{
		canvas.enabled = mustDisplay;
		Displayed = mustDisplay;
		if (Displayed)
		{
			DisplayOverlay(mustDisplay: true);
			RefreshContent();
			apocalypseHeader.ContinueAnimations();
			apocalypseLevelView.StopAnimation(stopAnimations: false);
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
			{
				SelectDefaultJoystickSelectable();
			}
			return;
		}
		DisplayOverlay(mustDisplay: false);
		apocalypseHeader.PauseAnimations();
		apocalypseLevelView.StopAnimation(stopAnimations: true);
		if (!TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			return;
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		if (ApplicationManager.Application.State.GetName() == "WorldMap")
		{
			if (TPSingleton<ApocalypseSelectionPanel>.Instance != null && TPSingleton<ApocalypseSelectionPanel>.Instance.Displayed)
			{
				TPSingleton<ApocalypseSelectionPanel>.Instance.SelectDefaultJoystickSelectableAfterAFrame();
			}
			else
			{
				TPSingleton<GameConfigurationsView>.Instance.JoystickSelectPanel();
			}
		}
	}

	private void DisplayOverlay(bool mustDisplay)
	{
		if (ApplicationManager.Application.State.GetName() == "WorldMap")
		{
			overlay.SetActive(mustDisplay);
			return;
		}
		overlay.SetActive(value: false);
		CameraView.AttenuateWorldForPopupFocus(mustDisplay ? this : null);
	}

	private void InstantiateModifierDisplays(int modifierStepToDisplayNb)
	{
		int num = modifierStepToDisplayNb - apocalypseModifierDisplays.Count;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				ApocalypseMoreInfoPanelModifierDisplay item = UnityEngine.Object.Instantiate(modifierDisplayPrefab, modifiersDisplayContainer);
				apocalypseModifierDisplays.Add(item);
			}
		}
	}

	private void OnDestroy()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	private void OnLastActiveControllerChanged(Rewired.ControllerType controllerType)
	{
		if (Displayed)
		{
			if (controllerType == Rewired.ControllerType.Joystick)
			{
				SelectDefaultJoystickSelectable();
			}
			else
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			}
		}
	}

	private void RefreshContent()
	{
		RefreshHeader();
		int level = ((ApocalypseManager.CurrentApocalypse != null) ? ApocalypseManager.CurrentApocalypse.CurrentLevel : 0);
		apocalypseLevelView.Init(level);
		RefreshModifiersDescriptions();
		scrollbar.value = 1f;
	}

	private void RefreshHeader()
	{
		apocalypseHeader.RefreshCityName();
		apocalypseHeader.RefreshRewardsFlames();
		apocalypseHeader.RefreshApocalypseLevel(useTween: false);
		if (apocalypseHeader.ApocalypseEffectsTooltip != null)
		{
			apocalypseHeader.ApocalypseEffectsTooltip.SetApocalypseModifierStepDefinitions(null);
		}
	}

	private void RefreshJoystickNavigation()
	{
		headerJoystickSimpleTarget.ClearSelectables();
		if (apocalypseHeader.ApocalypseGaugeDisplay != null)
		{
			int rewardAtLevelTooltipDisplayersNb = apocalypseHeader.ApocalypseGaugeDisplay.RewardAtLevelTooltipDisplayersNb;
			if (rewardAtLevelTooltipDisplayersNb == 0)
			{
				return;
			}
			ApocalypseMoreInfoPanelModifierDisplay apocalypseMoreInfoPanelModifierDisplay = null;
			if (apocalypseModifierDisplays.Count > 0)
			{
				apocalypseMoreInfoPanelModifierDisplay = apocalypseModifierDisplays.FirstOrDefault((ApocalypseMoreInfoPanelModifierDisplay modifierDisplay) => modifierDisplay.gameObject.activeSelf);
			}
			for (int num = 0; num < rewardAtLevelTooltipDisplayersNb; num++)
			{
				ApocalypseRewardAtLevelTooltipDisplayer rewardTooltipDisplayerAtIndex = apocalypseHeader.ApocalypseGaugeDisplay.GetRewardTooltipDisplayerAtIndex(num);
				if (!(rewardTooltipDisplayerAtIndex != null))
				{
					continue;
				}
				headerJoystickSimpleTarget.AddSelectable(rewardTooltipDisplayerAtIndex.JoystickSelectable);
				if (!(apocalypseMoreInfoPanelModifierDisplay == null))
				{
					rewardTooltipDisplayerAtIndex.JoystickSelectable.SetSelectOnDown(apocalypseMoreInfoPanelModifierDisplay.JoystickSelectable);
					if (num == 0)
					{
						apocalypseMoreInfoPanelModifierDisplay.JoystickSelectable.SetSelectOnUp(rewardTooltipDisplayerAtIndex.JoystickSelectable);
					}
				}
			}
		}
		scrollViewJoystickSimpleTarget.ClearSelectables();
		if (apocalypseModifierDisplays.Count <= 0)
		{
			return;
		}
		foreach (ApocalypseMoreInfoPanelModifierDisplay apocalypseModifierDisplay in apocalypseModifierDisplays)
		{
			if (apocalypseModifierDisplay.gameObject.activeSelf)
			{
				scrollViewJoystickSimpleTarget.AddSelectable(apocalypseModifierDisplay.JoystickSelectable);
			}
		}
	}

	private void RefreshModifiersDescriptions()
	{
		apocalypseModifierStepDefinitions = ApocalypseManager.CurrentApocalypseModifierStepDefinitions;
		int num = 0;
		if (apocalypseModifierStepDefinitions != null || apocalypseModifierStepDefinitions.Count == 0)
		{
			num = apocalypseModifierStepDefinitions.Count;
		}
		InstantiateModifierDisplays(num);
		bool flag = true;
		int num2 = 0;
		for (int i = 0; i < apocalypseModifierDisplays.Count; i++)
		{
			if (i < num)
			{
				apocalypseModifierDisplays[i].gameObject.SetActive(value: true);
				bool flag2 = i + 1 < num;
				apocalypseModifierDisplays[i].Init(apocalypseModifierStepDefinitions[i], !flag2, flag);
				apocalypseModifierDisplays[i].Refresh();
				flag = !flag;
				num2++;
			}
			else
			{
				apocalypseModifierDisplays[i].gameObject.SetActive(value: false);
			}
		}
		if (num2 > 0)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollViewNavigationInitializer.transform);
			scrollViewNavigationInitializer.InitNavigation(reset: true);
		}
		RefreshJoystickNavigation();
	}

	private void SelectDefaultJoystickSelectable()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
		TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickDynamicTarget.GetSelectionInfo());
	}

	private void Start()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
	}

	private void Update()
	{
		if (Displayed && (TheLastStand.Manager.InputManager.GetButtonDown(23) || TheLastStand.Manager.InputManager.GetButtonDown(137) || TheLastStand.Manager.InputManager.GetButtonDown(80)))
		{
			Close();
		}
	}

	static ApocalypseMoreInfoPanel()
	{
		ApocalypseMoreInfoPanel.OnPanelClosed = delegate
		{
		};
	}
}

using DG.Tweening;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.HUD;

public class GameAccelerationPanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	public static class Constants
	{
		public const string AccelerateGameRewiredKey = "GameAcceleration";

		public const string TooltipAvailableToggleIsOn = "GameAcceleration_Tooltip_Available_Toggle_IsOn";

		public const string TooltipAvailableToggleIsOff = "GameAcceleration_Tooltip_Available_Toggle_IsOff";

		public const string TooltipAvailableOnPress = "GameAcceleration_Tooltip_Available_OnPress";

		public const string TooltipUnvailable = "GameAcceleration_Tooltip_Unvailable";
	}

	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private GameObject toggleImageAnimated;

	[SerializeField]
	private RectTransform parentRectTransform;

	[SerializeField]
	private GamepadInputDisplay gamepadInputDisplay;

	[SerializeField]
	private float openPosition = -14f;

	[SerializeField]
	private float closePosition = -73f;

	[SerializeField]
	[Range(0f, 1f)]
	private float transitionDuration = 0.25f;

	[SerializeField]
	private Ease openTransitionEasing = Ease.OutBack;

	[SerializeField]
	private Ease closeTransitionEasing = Ease.InBack;

	private bool accelerateGame;

	private Tween closeTween;

	private bool mouseIsHover;

	private Tween openTween;

	private bool wasAcceleratingBeforeOpenPanel;

	public bool CanAccelerate { get; private set; }

	public bool Shown { get; private set; }

	public void Hide()
	{
		if (Shown)
		{
			closeTween?.Kill();
			openTween?.Kill();
			closeTween = parentRectTransform.DOAnchorPosX(closePosition, transitionDuration, snapping: true).SetEase(closeTransitionEasing).OnComplete(delegate
			{
				Shown = false;
			});
			closeTween.Play();
			gamepadInputDisplay.gameObject.SetActive(value: false);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!CanAccelerate)
		{
			return;
		}
		switch (TPSingleton<SettingsManager>.Instance.Settings.SpeedMode)
		{
		case SettingsManager.E_SpeedMode.Toggle:
			ToggleAcceleration(!accelerateGame);
			if (mouseIsHover)
			{
				DisplayTooltip();
			}
			break;
		case SettingsManager.E_SpeedMode.OnPress:
			ToggleAcceleration(toggle: true);
			if (mouseIsHover)
			{
				UIManager.GenericTooltip.Hide();
			}
			break;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		mouseIsHover = true;
		DisplayTooltip();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		mouseIsHover = false;
		UIManager.GenericTooltip.Hide();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (CanAccelerate && TPSingleton<SettingsManager>.Instance.Settings.SpeedMode == SettingsManager.E_SpeedMode.OnPress)
		{
			ToggleAcceleration(toggle: false);
			if (mouseIsHover)
			{
				DisplayTooltip();
			}
		}
	}

	public void Refresh()
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			if (Shown)
			{
				DisableButtonAndAcceleration();
			}
			Hide();
			break;
		case Game.E_Cycle.Night:
			SetAvailability(state: true);
			Show();
			switch (TPSingleton<GameManager>.Instance.Game.State)
			{
			case Game.E_State.CharacterSheet:
			case Game.E_State.Settings:
			case Game.E_State.ConsentPopup:
				wasAcceleratingBeforeOpenPanel = accelerateGame;
				DisableButtonAndAcceleration();
				break;
			case Game.E_State.Management:
				if ((TPSingleton<SettingsManager>.Instance.Settings.SpeedMode == SettingsManager.E_SpeedMode.Toggle && wasAcceleratingBeforeOpenPanel) || InputManager.GetButton(76))
				{
					wasAcceleratingBeforeOpenPanel = false;
					accelerateGame = true;
					toggleImageAnimated.SetActive(accelerateGame);
					SettingsController.ToggleGameSpeed(accelerateGame);
				}
				break;
			}
			break;
		}
	}

	private void ToggleAcceleration(bool toggle)
	{
		accelerateGame = toggle;
		toggleImageAnimated.SetActive(accelerateGame);
		SettingsController.ToggleGameSpeed(accelerateGame);
	}

	private void Show()
	{
		if (!Shown && UIManager.DebugToggleUI != false)
		{
			closeTween?.Kill();
			openTween?.Kill();
			openTween = parentRectTransform.DOAnchorPosX(openPosition, transitionDuration, snapping: true).SetEase(openTransitionEasing).OnComplete(delegate
			{
				Shown = true;
			});
			openTween.Play();
			gamepadInputDisplay.gameObject.SetActive(value: true);
		}
	}

	private void DisableButtonAndAcceleration()
	{
		SetAvailability(state: false);
		accelerateGame = false;
		toggleImageAnimated.SetActive(value: false);
		SettingsController.ToggleGameSpeed(isOn: false);
		if (UIManager.GenericTooltip.Displayed)
		{
			UIManager.GenericTooltip.Hide();
		}
	}

	private void DisplayTooltip()
	{
		UIManager.GenericTooltip.SetContentWithHotkeys(Localizer.Get(RetrieveProperLocKey()), "GameAcceleration", mustFormatHotkey: true);
		UIManager.GenericTooltip.Display();
	}

	private string RetrieveProperLocKey()
	{
		if (CanAccelerate)
		{
			switch (TPSingleton<SettingsManager>.Instance.Settings.SpeedMode)
			{
			case SettingsManager.E_SpeedMode.Toggle:
				if (!accelerateGame)
				{
					return "GameAcceleration_Tooltip_Available_Toggle_IsOff";
				}
				return "GameAcceleration_Tooltip_Available_Toggle_IsOn";
			case SettingsManager.E_SpeedMode.OnPress:
				return "GameAcceleration_Tooltip_Available_OnPress";
			}
		}
		return string.Empty;
	}

	private void SetAvailability(bool state)
	{
		button.Interactable = state;
		CanAccelerate = state;
	}

	private void Update()
	{
		if (CanAccelerate)
		{
			if (InputManager.GetButtonDown(76))
			{
				OnPointerDown(null);
			}
			if (InputManager.GetButtonUp(76))
			{
				OnPointerUp(null);
			}
		}
	}
}

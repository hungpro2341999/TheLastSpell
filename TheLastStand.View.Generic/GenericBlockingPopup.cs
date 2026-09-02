using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class GenericBlockingPopup : MonoBehaviour, IOverlayUser
{
	public static class Constants
	{
		public const string SmallContentLocalizedFontChilds = "SmallContent";

		public const string ContentLocalizedFontChilds = "Content";
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Image overlay;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private GameObject popupContainer;

	[SerializeField]
	private BetterButton closeButton;

	[SerializeField]
	private GameObject complexBox;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI coreComplexText;

	[SerializeField]
	private HyperlinkListener coreComplexHyperlink;

	[SerializeField]
	private TextMeshProUGUI coreSimpleText;

	[SerializeField]
	private HyperlinkListener coreSimpleHyperlink;

	[SerializeField]
	private BlockingPopupLine levelUpBlockingPopupLine;

	[SerializeField]
	private BlockingPopupLine rewardBlockingPopupLine;

	[SerializeField]
	private GameObject simpleBox;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameObject smallPopupContainer;

	[SerializeField]
	private BetterButton smallCloseButton;

	[SerializeField]
	private BetterButton smallConfirmButton;

	[SerializeField]
	private TextMeshProUGUI smallCoreSimpleText;

	[SerializeField]
	private HyperlinkListener smallCoreSimpleHyperlink;

	[SerializeField]
	private TextMeshProUGUI smallTitleText;

	private bool isSmallVersion;

	private bool openThisFrame;

	private Action cancelAction;

	private bool isOpen;

	public static GenericBlockingPopup Instance { get; private set; }

	public static bool IsOpen => Instance.isOpen;

	public Canvas Canvas => canvas;

	public Localizer.ParameterizedLocalizationLine Text { get; private set; }

	public Localizer.ParameterizedLocalizationLine Title { get; private set; }

	public int OverlaySortingOrder => Canvas.sortingOrder - 1;

	public static bool IsWaitingForInput()
	{
		if (Instance != null && Instance.Canvas.enabled)
		{
			return !Instance.openThisFrame;
		}
		return false;
	}

	public static GenericBlockingPopup OpenAsComplex(string titleLocKey, string textLocKey, UnityAction confirmAction)
	{
		return Open(new Localizer.ParameterizedLocalizationLine(titleLocKey), new Localizer.ParameterizedLocalizationLine(textLocKey), shouldShowAsComplex: true, confirmAction);
	}

	public static GenericBlockingPopup OpenAsSimple(string titleLocKey, string textLocKey, UnityAction confirmAction, bool smallVersion = false, Action cancelAction = null)
	{
		return Open(new Localizer.ParameterizedLocalizationLine(titleLocKey), new Localizer.ParameterizedLocalizationLine(textLocKey), shouldShowAsComplex: false, confirmAction, smallVersion, cancelAction);
	}

	public static GenericBlockingPopup Open(Localizer.ParameterizedLocalizationLine titleLocKey, Localizer.ParameterizedLocalizationLine textLocKey, bool shouldShowAsComplex, UnityAction confirmAction, bool smallVersion = false, Action cancelAction = null)
	{
		if (Instance == null)
		{
			Debug.LogError("Someone tried to open a generic blocking popup, but there are NONE here! Please add it to the scene.");
			return null;
		}
		Instance.cancelAction = cancelAction;
		Instance.isSmallVersion = smallVersion;
		Instance.Canvas.enabled = true;
		Instance.isOpen = true;
		Instance.Title = titleLocKey;
		Instance.Text = textLocKey;
		Instance.RefreshContent(shouldShowAsComplex);
		Instance.confirmButton.onClick.RemoveAllListeners();
		Instance.confirmButton.onClick.AddListener(Instance.StartCloseCoroutine);
		Instance.confirmButton.onClick.AddListener(confirmAction);
		Instance.smallConfirmButton.onClick.RemoveAllListeners();
		Instance.smallConfirmButton.onClick.AddListener(Instance.StartCloseCoroutine);
		Instance.smallConfirmButton.onClick.AddListener(confirmAction);
		CameraView.AttenuateWorldForPopupFocus(Instance);
		if (TPSingleton<GameManager>.Exist())
		{
			GameController.SetState(Game.E_State.BlockingPopup);
		}
		InputManager.OnGenericConsentViewToggled(state: true);
		Instance.InitJoystickNavigation();
		if (InputManager.IsLastControllerJoystick)
		{
			Instance.StartCoroutine(Instance.JoystickHighlightFollowTarget());
		}
		Instance.StartCoroutine(Instance.OpenThisFrameCoroutine());
		return Instance;
	}

	public void RefreshContent(bool shouldShowAsComplex)
	{
		overlay.enabled = !TPSingleton<ACameraView>.Exist();
		if (Text.key == null || Title.key == null)
		{
			return;
		}
		popupContainer.SetActive(Canvas.enabled && !isSmallVersion);
		smallPopupContainer.SetActive(Canvas.enabled && isSmallVersion);
		if (isSmallVersion)
		{
			smallTitleText.text = Localizer.Get(Title);
			smallCoreSimpleText.text = Localizer.Get(Text);
			smallCoreSimpleHyperlink.ForceRefresh();
		}
		else
		{
			titleText.text = Localizer.Get(Title);
			complexBox.SetActive(shouldShowAsComplex);
			simpleBox.SetActive(!shouldShowAsComplex);
			if (shouldShowAsComplex)
			{
				coreComplexText.text = Localizer.Get(Text);
				coreComplexHyperlink.ForceRefresh();
				if (TurnEndValidationManager.AnyPlayableUnitWaitingForLevelUp)
				{
					levelUpBlockingPopupLine.gameObject.SetActive(value: true);
					levelUpBlockingPopupLine.UpdateDisplayedText("GenericBlocking_FinishLevelUp", GetPlayableUnitsLevelingUpNames());
				}
				else
				{
					levelUpBlockingPopupLine.gameObject.SetActive(value: false);
				}
				if (TurnEndValidationManager.AnyProdItemsLeft)
				{
					rewardBlockingPopupLine.gameObject.SetActive(value: true);
					rewardBlockingPopupLine.UpdateDisplayedText("GenericBlocking_CollectReward");
				}
				else
				{
					rewardBlockingPopupLine.gameObject.SetActive(value: false);
				}
			}
			else
			{
				coreSimpleText.text = Localizer.Get(Text);
				coreSimpleHyperlink.ForceRefresh();
			}
		}
		if (complexFontLocalizedParent != null)
		{
			complexFontLocalizedParent.TargetKey = (isSmallVersion ? "SmallContent" : "Content");
			complexFontLocalizedParent.RefreshChildren();
		}
	}

	private List<string> GetPlayableUnitsLevelingUpNames()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			PlayableUnit playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i];
			if (playableUnit.LevelPoints > 0)
			{
				list.Add(playableUnit.PlayableUnitName);
			}
		}
		return list;
	}

	private void Awake()
	{
		Instance = this;
		Instance.closeButton.onClick.AddListener(StartCloseCoroutine);
		Instance.smallCloseButton.onClick.AddListener(StartCloseCoroutine);
		Instance.confirmButton.onClick.AddListener(StartCloseCoroutine);
		Instance.smallConfirmButton.onClick.AddListener(StartCloseCoroutine);
		levelUpBlockingPopupLine.MainButton.onClick.AddListener(delegate
		{
			StartCloseCoroutine(attenuateOverlayBlur: false);
			TPSingleton<ToDoListView>.Instance.OnLevelUpButtonClick();
			EventSystem.current.SetSelectedGameObject(null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		});
		rewardBlockingPopupLine.MainButton.onClick.AddListener(delegate
		{
			StartCloseCoroutine(attenuateOverlayBlur: false);
			TPSingleton<ProductionReportPanel>.Instance.Open();
		});
	}

	private void Close(bool attenuateOverlayBlur = true)
	{
		if (TPSingleton<GameManager>.Exist() && TPSingleton<GameManager>.Instance.Game.PreviousState != Game.E_State.BlockingPopup)
		{
			GameController.SetState(TPSingleton<GameManager>.Instance.Game.PreviousState);
		}
		InputManager.OnGenericConsentViewToggled(state: false);
		if (attenuateOverlayBlur)
		{
			CameraView.AttenuateWorldForPopupFocus(null);
		}
		Instance.cancelAction?.Invoke();
		Instance.Canvas.enabled = false;
		Instance.isOpen = false;
	}

	private IEnumerator CloseAtEndOfFrame(bool attenuateOverlayBlur = true)
	{
		yield return SharedYields.WaitForEndOfFrame;
		Close(attenuateOverlayBlur);
	}

	private void InitJoystickNavigation()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			levelUpBlockingPopupLine.MainButton.SetMode(Navigation.Mode.Explicit);
			rewardBlockingPopupLine.MainButton.SetMode(Navigation.Mode.Explicit);
			if (levelUpBlockingPopupLine.MainButton.gameObject.activeInHierarchy && rewardBlockingPopupLine.MainButton.gameObject.activeInHierarchy)
			{
				levelUpBlockingPopupLine.MainButton.SetSelectOnDown(rewardBlockingPopupLine.MainButton);
				rewardBlockingPopupLine.MainButton.SetSelectOnUp(levelUpBlockingPopupLine.MainButton);
			}
			EventSystem.current.SetSelectedGameObject(levelUpBlockingPopupLine.MainButton.gameObject.activeInHierarchy ? levelUpBlockingPopupLine.MainButton.gameObject : rewardBlockingPopupLine.MainButton.gameObject);
		}
	}

	private IEnumerator OpenThisFrameCoroutine()
	{
		openThisFrame = true;
		yield return SharedYields.WaitForEndOfFrame;
		openThisFrame = false;
	}

	private void StartCloseCoroutine()
	{
		StartCloseCoroutine(attenuateOverlayBlur: true);
	}

	private void StartCloseCoroutine(bool attenuateOverlayBlur)
	{
		StartCoroutine(CloseAtEndOfFrame(attenuateOverlayBlur));
	}

	private void Update()
	{
		if (IsWaitingForInput() && (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(66) || InputManager.GetButtonDown(7) || InputManager.GetButtonDown(80)))
		{
			StartCloseCoroutine();
			EventSystem.current.SetSelectedGameObject(null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
	}

	private IEnumerator JoystickHighlightFollowTarget()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		yield return SharedYields.WaitForSeconds(0.5f);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
	}
}

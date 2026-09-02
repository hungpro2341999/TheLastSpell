using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TheLastStand.Controller;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Database.Building;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.Model;
using TheLastStand.Model.ProductionReport;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

public class ProductionReportPanel : TPSingleton<ProductionReportPanel>, IOverlayUser
{
	[SerializeField]
	private float panelDeltaPosY = -20f;

	[SerializeField]
	private RectTransform productionReportMask;

	[SerializeField]
	private Scrollbar productionReportScrollbar;

	[SerializeField]
	private Button productionReportTopButton;

	[SerializeField]
	private Button productionReportBotButton;

	[SerializeField]
	[Range(0f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private RectTransform viewPort;

	[SerializeField]
	private Scrollbar scrollBar;

	[SerializeField]
	private ProductionObjectDisplay productionObjectPrefab;

	[SerializeField]
	private RectTransform productionObjectParent;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer layoutNavigationInitializer;

	[SerializeField]
	private AudioClip openAudioClip;

	[SerializeField]
	private bool playCloseSound = true;

	[SerializeField]
	private AudioSource closeAudioSource;

	[SerializeField]
	private AudioSource newItemsAudioSource;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private bool isOpened;

	private Tween moveTween;

	private float posYInit;

	private List<ProductionObjectDisplay> productionObjects = new List<ProductionObjectDisplay>();

	private RectTransform rectTransform;

	private bool firstFrameOpened = true;

	private int lastClosedEnabledObjectsCount;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public HUDJoystickSimpleTarget JoystickTarget => joystickTarget;

	public static void RefreshScrollbar()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(TPSingleton<ProductionReportPanel>.Instance.productionObjectParent);
		bool interactable = TPSingleton<ProductionReportPanel>.Instance.productionObjectParent.sizeDelta.y > TPSingleton<ProductionReportPanel>.Instance.productionReportMask.sizeDelta.y;
		TPSingleton<ProductionReportPanel>.Instance.productionReportScrollbar.interactable = interactable;
		TPSingleton<ProductionReportPanel>.Instance.productionReportTopButton.interactable = interactable;
		TPSingleton<ProductionReportPanel>.Instance.productionReportBotButton.interactable = interactable;
	}

	public void AdjustScrollView(RectTransform focusedRect)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(focusedRect, viewPort, scrollBar, 0.01f, 0.01f);
	}

	public static void RefreshTitle()
	{
		TPSingleton<ProductionReportPanel>.Instance.titleText.text = Localizer.Format("ProductionReport_JournalSentence", TPSingleton<GameManager>.Instance.Game.DayNumber);
	}

	public void CheckOnProductionObjectHide()
	{
		RefreshScrollbar();
		if (TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count == 0)
		{
			GameController.SetState(Game.E_State.Management);
			Close();
		}
		else if (InputManager.IsLastControllerJoystick)
		{
			layoutNavigationInitializer.InitNavigation();
			SelectFirstProduct();
		}
	}

	public void Close()
	{
		if (isOpened)
		{
			CLoggerManager.Log("ProductionReportPanel closed", this, LogType.Log, CLogLevel.DETAILED);
			CameraView.AttenuateWorldForPopupFocus(null);
			lastClosedEnabledObjectsCount = productionObjects.Count((ProductionObjectDisplay o) => o.gameObject.activeSelf);
			if (moveTween != null)
			{
				moveTween.Kill();
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);
			}
			isOpened = false;
			moveTween = rectTransform.DOAnchorPosY(posYInit, 0.25f).SetEase(Ease.InBack).OnComplete(delegate
			{
				canvas.enabled = false;
			});
			canvasGroup.blocksRaycasts = false;
			for (int num = 0; num < TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count; num++)
			{
				productionObjects[num].Hide();
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			if (playCloseSound)
			{
				closeAudioSource.Play();
			}
		}
	}

	public void RefreshGameObjects()
	{
		int count = TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count;
		int i;
		for (i = 0; i < count; i++)
		{
			while (productionObjects.Count <= i)
			{
				ProductionObjectDisplay item = UnityEngine.Object.Instantiate(productionObjectPrefab, productionObjectParent);
				productionObjects.Add(item);
			}
			productionObjects[i].gameObject.SetActive(value: true);
			productionObjects[i].ProductionObject = TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects[i];
			productionObjects[i].ProductionObject.ProductionObjectView = productionObjects[i];
			productionObjects[i].Display();
		}
		for (; i < productionObjects.Count; i++)
		{
			productionObjects[i].gameObject.SetActive(value: false);
		}
	}

	public bool SelectFirstProduct()
	{
		GameObject gameObject = productionObjects.FirstOrDefault((ProductionObjectDisplay x) => x.gameObject.activeSelf)?.gameObject;
		EventSystem.current.SetSelectedGameObject(gameObject);
		return gameObject != null;
	}

	public void Open()
	{
		if (isOpened)
		{
			return;
		}
		firstFrameOpened = true;
		CLoggerManager.Log("ProductionReportPanel opened", this, LogType.Log, CLogLevel.DETAILED);
		GameController.SetState(Game.E_State.ProductionReport);
		CameraView.AttenuateWorldForPopupFocus(this);
		if (moveTween != null)
		{
			moveTween.Kill();
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posYInit);
		}
		isOpened = true;
		simpleFontLocalizedParent?.RefreshChildren();
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		moveTween = rectTransform.DOAnchorPosY(panelDeltaPosY, 0.25f).SetEase(Ease.OutBack).OnComplete(delegate
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		});
		RefreshGameObjects();
		RefreshTitle();
		RefreshScrollbar();
		canvas.enabled = true;
		canvasGroup.blocksRaycasts = true;
		if (InputManager.IsLastControllerJoystick)
		{
			joystickTarget.ClearSelectables();
			foreach (ProductionObjectDisplay productionObject in productionObjects)
			{
				joystickTarget.AddSelectable(productionObject.Selectable);
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(JoystickTarget.GetSelectionInfo());
			layoutNavigationInitializer.InitNavigation();
			SelectFirstProduct();
		}
		SoundManager.PlayAudioClip(openAudioClip);
		if (TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count > lastClosedEnabledObjectsCount)
		{
			newItemsAudioSource.Play();
		}
	}

	public void OnCloseButtonClick()
	{
		if (!TPSingleton<ChooseRewardPanel>.Instance.IsOpened)
		{
			GameController.SetState(Game.E_State.Management);
			Close();
		}
	}

	public void OnTopButtonClick()
	{
		productionReportScrollbar.value = Mathf.Clamp01(productionReportScrollbar.value + scrollButtonsSensitivity);
	}

	public void OnBotButtonClick()
	{
		productionReportScrollbar.value = Mathf.Clamp01(productionReportScrollbar.value - scrollButtonsSensitivity);
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		canvas = TPSingleton<ProductionReportPanel>.Instance.GetComponent<Canvas>();
		canvas.enabled = false;
		canvasGroup = TPSingleton<ProductionReportPanel>.Instance.GetComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		isOpened = false;
		rectTransform = GetComponent<RectTransform>();
		posYInit = rectTransform.anchoredPosition.y;
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshTitle();
		}
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void Update()
	{
		if (isOpened && !TPSingleton<ChooseRewardPanel>.Instance.IsOpened)
		{
			if (firstFrameOpened)
			{
				firstFrameOpened = false;
			}
			else if ((InputManager.GetButtonDown(29) || InputManager.GetButtonDown(80)) && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.ProductionReport)
			{
				OnCloseButtonClick();
			}
		}
	}

	[ContextMenu("Open")]
	public void DebugOpen()
	{
		if (!UnityEngine.Application.isPlaying)
		{
			Debug.LogError("Unable to use this context menu when the application is not running");
		}
		else
		{
			if (TPSingleton<ProductionReportPanel>.Instance.isOpened)
			{
				return;
			}
			GameController.SetState(Game.E_State.ProductionReport);
			if (TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count == 0)
			{
				for (int i = 0; i < 11; i++)
				{
					ProductionItems productionItem = new ProductionItemController(BuildingDatabase.BuildingDefinitions["Blacksmith"]).ProductionItem;
					productionItem.IsNightProduction = false;
					TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.AddProductionObject(productionItem);
				}
			}
			Open();
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Controller;
using TheLastStand.Controller.Unit;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ToDoList;

public class ToDoListView : TPSingleton<ToDoListView>
{
	[SerializeField]
	private Canvas toDoListCanvas;

	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private RectTransform toDoListRectTransform;

	[SerializeField]
	private RectTransform contentRectTransform;

	[SerializeField]
	private RectTransform bgRectTransform;

	[SerializeField]
	private RectTransform scrollViewRectTransform;

	[SerializeField]
	private ScrollRect toDoListScrollRect;

	[SerializeField]
	private GameObject scrollTopButton;

	[SerializeField]
	private GameObject scrollBotButton;

	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	[Range(0f, 1000f)]
	[Tooltip("This is the value in pixels used to move the content of the scrollView")]
	private float scrollButtonsSensitivity = 100f;

	[SerializeField]
	private ToDoListLevelUpNotificationView toDoListUnitLevelUpNotifications;

	[SerializeField]
	private ToDoListProductionNotificationView toDoListProductionNotificationView;

	[SerializeField]
	private ToDoListGoldNotificationView toDoListGoldNotificationView;

	[SerializeField]
	private ToDoListWorkersNotificationView toDoListWorkersNotificationView;

	[SerializeField]
	private ToDoListNotificationView toDoListMaterialsNotificationView;

	[SerializeField]
	private ToDoListWavesNotificationView toDoListWavesNotificationView;

	[SerializeField]
	private ToDoListPositionNotificationView toDoListPositionNotificationView;

	[SerializeField]
	private ToDoListInventoryNotificationView toDoListInventoryNotificationView;

	[SerializeField]
	private ToDoListMetaShopsNotificationView toDoListMetaShopsNotificationView;

	[SerializeField]
	private RectTransform[] displacedRectTransforms;

	[SerializeField]
	[Tooltip("That will be the position offset of the resources panel when NOT folded")]
	private float displacedFoldRectTransformsOrigin = 12f;

	[SerializeField]
	[Tooltip("That will be the position offset of the resources panel when folded")]
	private float displacedFoldRectTransformsDestination = 60f;

	[SerializeField]
	private RectTransform foldButtonRectTransform;

	[SerializeField]
	private Button foldButton;

	[SerializeField]
	[Tooltip("That will be the position offset of the todo list panel when folded")]
	private float foldedPosition = -200f;

	[SerializeField]
	private RectTransform todoListFoldButtonRectTransform;

	[SerializeField]
	private RectTransform todoListFoldButtonNoScrollTransform;

	[SerializeField]
	private RectTransform todoListFoldButtonCanScrollTransform;

	[SerializeField]
	[Range(0f, 2f)]
	private float unfoldDuration = 0.4f;

	[SerializeField]
	private Ease unfoldEasing = Ease.InCubic;

	[SerializeField]
	[Range(0f, 2f)]
	private float foldDuration = 0.4f;

	[SerializeField]
	private Ease foldEasing = Ease.OutCubic;

	[SerializeField]
	private Selectable[] notificationsButtons;

	private int currentSpawnWaveFeedbackTargetIndex;

	private bool isFolded;

	private bool canScroll;

	private float foldDisplacementPosition;

	private Tween displacedFoldTween;

	private Tween todoListFoldTween;

	public bool IsDisplayed { get; private set; } = true;

	public void CloseInventoryNotification()
	{
		toDoListInventoryNotificationView.Refresh();
		RefreshToDoList();
	}

	public Selectable GetFirstActiveSelectable()
	{
		if (!IsDisplayed)
		{
			return null;
		}
		for (int i = 0; i < notificationsButtons.Length; i++)
		{
			if (notificationsButtons[i].gameObject.activeInHierarchy && notificationsButtons[i].IsInteractable())
			{
				return notificationsButtons[i];
			}
		}
		return null;
	}

	public Selectable GetLastActiveSelectable()
	{
		if (!IsDisplayed)
		{
			return null;
		}
		for (int num = notificationsButtons.Length - 1; num >= 0; num--)
		{
			if (notificationsButtons[num].gameObject.activeInHierarchy && notificationsButtons[num].IsInteractable())
			{
				return notificationsButtons[num];
			}
		}
		return null;
	}

	public Selectable GetFoldButton()
	{
		return foldButton;
	}

	public void Hide()
	{
		if (IsDisplayed)
		{
			if (simpleFontLocalizedParent != null)
			{
				simpleFontLocalizedParent.UnregisterChildren();
			}
			HideNotifications();
			IsDisplayed = false;
			toDoListCanvas.enabled = false;
		}
	}

	private void HideNotifications()
	{
		toDoListUnitLevelUpNotifications.Display(show: false);
		toDoListProductionNotificationView.Display(show: false);
		toDoListGoldNotificationView.Display(show: false);
		toDoListWorkersNotificationView.Display(show: false);
		toDoListMaterialsNotificationView.Display(show: false);
		toDoListWavesNotificationView.Display(show: false);
		toDoListPositionNotificationView.Display(show: false);
		toDoListInventoryNotificationView.Display(show: false);
		toDoListMetaShopsNotificationView.Display(show: false);
	}

	public void OnNotificationButtonSelected(BetterButton button)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(button.transform as RectTransform, scrollViewRectTransform, scrollbar, 0.01f, 0.01f);
	}

	public void OnWorkersButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != Construction.E_State.None)
		{
			ConstructionManager.ExitConstructionMode();
		}
		Predicate<BuildingAction> filter = delegate(BuildingAction buildingAction)
		{
			int modifiedWorkersCost = ResourceManager.GetModifiedWorkersCost(buildingAction.BuildingActionDefinition);
			return modifiedWorkersCost > 0 && modifiedWorkersCost <= TPSingleton<ResourceManager>.Instance.Workers && buildingAction.UsesPerTurnRemaining != 0;
		};
		List<TheLastStand.Model.Building.Building> buildingsWithActions = GetBuildingsWithActions(filter);
		if (buildingsWithActions.Count > 0)
		{
			SelectNextBuilding(buildingsWithActions);
		}
	}

	public void OnFreeActionsButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != Construction.E_State.None)
		{
			ConstructionManager.ExitConstructionMode();
		}
		Predicate<BuildingAction> filter = (BuildingAction buildingAction) => ResourceManager.GetModifiedWorkersCost(buildingAction.BuildingActionDefinition) == 0 && buildingAction.UsesPerTurnRemaining != 0;
		List<TheLastStand.Model.Building.Building> buildingsWithActions = GetBuildingsWithActions(filter);
		if (buildingsWithActions.Count > 0)
		{
			SelectNextBuilding(buildingsWithActions);
		}
	}

	public void OnBotButtonClick()
	{
		toDoListScrollRect.verticalScrollbar.value = Mathf.Clamp01(toDoListScrollRect.verticalScrollbar.value - scrollButtonsSensitivity / contentRectTransform.sizeDelta.y);
	}

	public void OnFoldButtonClick()
	{
		if (isFolded)
		{
			Unfold();
		}
		else
		{
			Fold();
		}
	}

	public void OnGoldConstructionButtonClick()
	{
		if (GameController.CanOpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Production))
		{
			ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Production);
			ConstructionManager.SetState(Construction.E_State.ChooseBuilding);
		}
	}

	public void OnInnButtonClick()
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Recruitment && RecruitmentController.CanOpenRecruitmentPanel())
		{
			RecruitmentController.OpenRecruitmentPanel();
		}
	}

	public void OnInventoryButtonClick()
	{
		if (TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory())
		{
			if (!TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[0]);
			}
			CharacterSheetManager.OpenCharacterSheetPanel();
			TPSingleton<CharacterSheetPanel>.Instance.OpenInventory();
		}
	}

	public void OnLevelUpButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != Construction.E_State.None)
		{
			ConstructionManager.ExitConstructionMode();
		}
		PlayableUnit playableUnit = null;
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].StatsPoints > 0)
			{
				playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i];
				break;
			}
		}
		for (int j = 0; j < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; j++)
		{
			if (TileObjectSelectionManager.SelectedUnit != playableUnit)
			{
				PlayableUnitManager.SelectNextUnit();
			}
		}
		if (UnitLevelUpController.CanOpenUnitLevelUpView)
		{
			TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = TileObjectSelectionManager.SelectedPlayableUnit.LevelUp;
			TPSingleton<UnitLevelUpView>.Instance.Open();
		}
	}

	public void OnMaterialsButtonButtonClick()
	{
		if (GameController.CanOpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive))
		{
			ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive);
			ConstructionManager.SetState(Construction.E_State.ChooseBuilding);
		}
	}

	public void OnDarkShopButtonButtonClick()
	{
		TPSingleton<MetaShopsManager>.Instance.OpenMetaShop(darkShop: true);
	}

	public void OnLightShopButtonButtonClick()
	{
		TPSingleton<MetaShopsManager>.Instance.OpenMetaShop(darkShop: false);
	}

	public void OnProductionButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != Construction.E_State.None && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction)
		{
			ConstructionManager.ExitConstructionMode();
		}
		GameController.SetState(Game.E_State.ProductionReport);
		TPSingleton<ProductionReportPanel>.Instance.Open();
	}

	public void OnShopButtonClick()
	{
		if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel())
		{
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.OpenShopPanel(fromAnotherPopup: false, TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit));
		}
	}

	public void OnTopButtonClick()
	{
		toDoListScrollRect.verticalScrollbar.value = Mathf.Clamp01(toDoListScrollRect.verticalScrollbar.value + scrollButtonsSensitivity / contentRectTransform.sizeDelta.y);
	}

	public void OnUnitPositionClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != Construction.E_State.None)
		{
			ConstructionManager.ExitConstructionMode();
		}
		List<PlayableUnit> list = new List<PlayableUnit>();
		int i = 0;
		for (int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i < count; i++)
		{
			if (!TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].MovedThisDay || TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].OriginTile.HasFog)
			{
				list.Add(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i]);
			}
		}
		if (!TileObjectSelectionManager.HasPlayableUnitSelected || !list.Contains(TileObjectSelectionManager.SelectedPlayableUnit))
		{
			TileObjectSelectionManager.SetSelectedPlayableUnit(list[0], focusCameraOnUnit: true);
			return;
		}
		int num = list.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
		TileObjectSelectionManager.SetSelectedPlayableUnit(list[++num % list.Count], focusCameraOnUnit: true);
	}

	public void OnWaveIncomingButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != Construction.E_State.None)
		{
			ConstructionManager.ExitConstructionMode();
		}
		for (int i = 0; i < SpawnDirectionsDefinition.OrderedDirections.Count; i++)
		{
			currentSpawnWaveFeedbackTargetIndex++;
			if (currentSpawnWaveFeedbackTargetIndex == SpawnDirectionsDefinition.OrderedDirections.Count)
			{
				currentSpawnWaveFeedbackTargetIndex = 0;
			}
			SpawnWaveView.SpawnWaveArrowPair spawnWaveArrowPair = SpawnWaveManager.SpawnWaveView.SpawnWavePreviewFeedbacks.FirstOrDefault((SpawnWaveView.SpawnWaveArrowPair x) => x.SpawnWaveDetailedZone.IsCentralZone() && x.CentralSpawnDirection == SpawnDirectionsDefinition.OrderedDirections[currentSpawnWaveFeedbackTargetIndex]);
			if (spawnWaveArrowPair.SpawnWaveViewPreviewFeedback != null)
			{
				SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
				if (currentSpawnWave != null && currentSpawnWave.RotatedProportionPerDirection.ContainsKey(spawnWaveArrowPair.CentralSpawnDirection))
				{
					ACameraView.MoveTo(spawnWaveArrowPair.SpawnWaveViewPreviewFeedback.CamTarget);
					break;
				}
			}
		}
	}

	[ContextMenu("Refresh all notifications")]
	public void RefreshAllNotifications()
	{
		toDoListUnitLevelUpNotifications.Refresh();
		toDoListProductionNotificationView.Refresh();
		toDoListGoldNotificationView.Refresh();
		toDoListWorkersNotificationView.Refresh();
		toDoListMaterialsNotificationView.Refresh();
		toDoListWavesNotificationView.Refresh();
		toDoListPositionNotificationView.Refresh();
		toDoListInventoryNotificationView.Refresh();
		toDoListMetaShopsNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshActionPointsNotification()
	{
	}

	public void RefreshGoldNotification()
	{
		toDoListGoldNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshInventoryNotification()
	{
		toDoListInventoryNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshMetaShopsNotification()
	{
		toDoListMetaShopsNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshProductionNotification()
	{
		toDoListProductionNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshPositionNotification()
	{
		toDoListPositionNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshSpawnWavePositionView()
	{
		toDoListWavesNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshToDoList()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);
		canScroll = contentRectTransform.rect.height > toDoListRectTransform.rect.height - Mathf.Abs(scrollViewRectTransform.offsetMax.y) - Mathf.Abs(scrollViewRectTransform.offsetMin.y);
		toDoListScrollRect.vertical = canScroll;
		scrollTopButton.SetActive(canScroll && !InputManager.IsLastControllerJoystick);
		scrollBotButton.SetActive(canScroll && !InputManager.IsLastControllerJoystick);
		todoListFoldButtonRectTransform.localPosition = (canScroll ? todoListFoldButtonCanScrollTransform.localPosition : todoListFoldButtonNoScrollTransform.localPosition);
		if (!canScroll)
		{
			toDoListScrollRect.verticalScrollbar.value = 1f;
		}
		bgRectTransform.sizeDelta = new Vector2(bgRectTransform.sizeDelta.x, Mathf.Min(contentRectTransform.rect.height + Mathf.Abs(scrollViewRectTransform.offsetMax.y) + Mathf.Abs(scrollViewRectTransform.offsetMin.y), toDoListRectTransform.rect.height));
		RefreshJoystickNavigation();
	}

	public void RefreshUnitLevelUpNotification()
	{
		toDoListUnitLevelUpNotifications.Refresh();
		RefreshToDoList();
	}

	public void RefreshWorkersNotification()
	{
		toDoListWorkersNotificationView.Refresh();
		RefreshToDoList();
	}

	public void Show()
	{
		if (!IsDisplayed && UIManager.DebugToggleUI != false && TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
		{
			if (simpleFontLocalizedParent != null)
			{
				simpleFontLocalizedParent.RegisterChildren();
			}
			toDoListWavesNotificationView.Display(show: true);
			IsDisplayed = true;
			toDoListCanvas.enabled = true;
			RefreshAllNotifications();
		}
	}

	public void SwitchRaycastTargetState(bool state)
	{
		graphicRaycaster.enabled = state;
	}

	private void Fold()
	{
		if (isFolded)
		{
			return;
		}
		todoListFoldTween?.Kill();
		displacedFoldTween?.Kill();
		todoListFoldTween = toDoListRectTransform.DOAnchorPosX(foldedPosition, foldDuration).SetEase(foldEasing).SetFullId("FoldTween", this);
		if (displacedRectTransforms != null && displacedRectTransforms.Length != 0)
		{
			foldDisplacementPosition = displacedRectTransforms[0].anchoredPosition.x;
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		displacedFoldTween = DOTween.To(() => foldDisplacementPosition, delegate(float x)
		{
			for (int i = 0; i < displacedRectTransforms.Length; i++)
			{
				displacedRectTransforms[i].anchoredPosition = new Vector2(x, displacedRectTransforms[i].anchoredPosition.y);
			}
		}, displacedFoldRectTransformsDestination, foldDuration).SetFullId("DisplacedFoldTween", this).OnComplete(delegate
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		});
		if (simpleFontLocalizedParent != null)
		{
			simpleFontLocalizedParent.UnregisterChildren();
		}
		foldButtonRectTransform.localScale = new Vector3(1f, 1f, 1f);
		isFolded = true;
	}

	private List<TheLastStand.Model.Building.Building> GetBuildingsWithActions(Predicate<BuildingAction> filter)
	{
		List<TheLastStand.Model.Building.Building> list = new List<TheLastStand.Model.Building.Building>();
		List<TheLastStand.Model.Building.Building> list2 = new List<TheLastStand.Model.Building.Building>();
		List<TheLastStand.Model.Building.Building> list3 = new List<TheLastStand.Model.Building.Building>();
		List<TheLastStand.Model.Building.Building> list4 = new List<TheLastStand.Model.Building.Building>();
		List<TheLastStand.Model.Building.Building> list5 = new List<TheLastStand.Model.Building.Building>();
		int i = 0;
		for (int count = TPSingleton<BuildingManager>.Instance.Buildings.Count; i < count; i++)
		{
			TheLastStand.Model.Building.Building building = TPSingleton<BuildingManager>.Instance.Buildings[i];
			if (building.ProductionModule?.BuildingActions == null || !building.ProductionModule.BuildingActions.Exists(filter))
			{
				continue;
			}
			if (building.ConstructionModule.CostsGold)
			{
				if (building.BuildingDefinition.ProductionModuleDefinition?.BuildingGaugeEffectDefinition != null)
				{
					list3.Add(building);
				}
				else
				{
					list2.Add(building);
				}
			}
			else if (building.IsDefensive)
			{
				list4.Add(building);
			}
			else
			{
				list5.Add(building);
			}
		}
		list.AddRange(list2);
		list.AddRange(list3);
		list.AddRange(list4);
		list.AddRange(list5);
		return list;
	}

	private void RefreshJoystickNavigation()
	{
		List<Selectable> list = new List<Selectable>();
		for (int i = 0; i < notificationsButtons.Length; i++)
		{
			if (notificationsButtons[i].gameObject.activeInHierarchy && notificationsButtons[i].IsInteractable())
			{
				list.Add(notificationsButtons[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].SetMode(Navigation.Mode.Explicit);
			if (j > 0)
			{
				list[j].SetSelectOnUp(list[j - 1]);
			}
			if (j < list.Count - 1)
			{
				list[j].SetSelectOnDown(list[j + 1]);
			}
		}
		foldButton.SetMode(Navigation.Mode.Explicit);
		if (list.Count > 0)
		{
			foldButton.SetSelectOnDown(list[0]);
			list[0].SetSelectOnUp(foldButton);
		}
		else
		{
			foldButton.SetSelectOnDown(null);
		}
	}

	private void OnDisable()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChange;
			TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent.RemoveListener(OnUIScaleChanged);
		}
		if (simpleFontLocalizedParent != null)
		{
			simpleFontLocalizedParent.UnregisterChildren();
		}
		MetaConditionManager.OnConditionsRefreshed -= RefreshAllNotifications;
	}

	private void OnEnable()
	{
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChange;
		TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent.AddListener(OnUIScaleChanged);
		MetaConditionManager.OnConditionsRefreshed += RefreshAllNotifications;
		if (simpleFontLocalizedParent != null)
		{
			simpleFontLocalizedParent.RegisterChildren();
		}
	}

	private void OnResolutionChange(Resolution resolution)
	{
		RefreshToDoList();
	}

	private void OnUIScaleChanged(float scale)
	{
		StartCoroutine(OnUIScaleChangedCoroutine());
	}

	private IEnumerator OnUIScaleChangedCoroutine()
	{
		yield return null;
		RefreshToDoList();
	}

	private void SelectNextBuilding(List<TheLastStand.Model.Building.Building> buildings)
	{
		int num = ((TileObjectSelectionManager.SelectedBuilding != null && buildings.Contains(TileObjectSelectionManager.SelectedBuilding)) ? (buildings.IndexOf(TileObjectSelectionManager.SelectedBuilding) + 1) : 0);
		if (num == buildings.Count)
		{
			num = 0;
		}
		TileObjectSelectionManager.SelectBuilding(buildings[num], focusCameraOnBuilding: true);
	}

	private void Start()
	{
		if (simpleFontLocalizedParent != null)
		{
			simpleFontLocalizedParent.RegisterChildren();
		}
	}

	private void Unfold()
	{
		if (!isFolded)
		{
			return;
		}
		todoListFoldTween?.Kill();
		displacedFoldTween?.Kill();
		todoListFoldTween = toDoListRectTransform.DOAnchorPosX(0f, unfoldDuration).SetEase(unfoldEasing).SetFullId("UnfoldTween", this);
		if (displacedRectTransforms != null && displacedRectTransforms.Length != 0)
		{
			foldDisplacementPosition = displacedRectTransforms[0].anchoredPosition.x;
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		displacedFoldTween = DOTween.To(() => foldDisplacementPosition, delegate(float x)
		{
			for (int i = 0; i < displacedRectTransforms.Length; i++)
			{
				displacedRectTransforms[i].anchoredPosition = new Vector2(x, displacedRectTransforms[i].anchoredPosition.y);
			}
		}, displacedFoldRectTransformsOrigin, foldDuration).SetFullId("DisplacedUnfoldTween", this).OnComplete(delegate
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		});
		if (simpleFontLocalizedParent != null)
		{
			simpleFontLocalizedParent.RegisterChildren();
		}
		foldButtonRectTransform.localScale = new Vector3(-1f, 1f, 1f);
		isFolded = false;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Controller.Unit;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using TheLastStand.View.Item;
using TheLastStand.View.PlayableUnitCustomisation;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.Perk;
using TheLastStand.View.Unit.Race;
using TheLastStand.View.Unit.Stat;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.CharacterSheet;

public class CharacterSheetPanel : TPSingleton<CharacterSheetPanel>, IOverlayUser
{
	public static class Constants
	{
		public const string DefaultLocalizedFontChildren = "Default";

		public const string LeftPanelLocalizedFontChildren = "LeftPanel";

		public const string CharacterDetailsLocalizedFontChildren = "CharacterDetails";

		public const string InventoryPanelLocalizedFontChildren = "InventoryPanel";

		public const string PerksPanelLocalizedFontChildren = "PerksPanel";
	}

	[SerializeField]
	private float panelDeltaPosY = -20f;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private Dictionary<Toggle, TabbedPageView> tabPagePairs;

	[SerializeField]
	private Toggle toggleUnitDetails;

	[SerializeField]
	private Toggle toggleInventory;

	[SerializeField]
	private Toggle togglePerk;

	[SerializeField]
	private GameObject inventoryUnavailableParent;

	[SerializeField]
	private GameObject inventoryUnavailableText;

	[SerializeField]
	private GameObject perkUnavailableParent;

	[SerializeField]
	private GameObject perkUnavailableText;

	[SerializeField]
	private Transform tabPosOn;

	[SerializeField]
	private Transform tabPosOff;

	[SerializeField]
	private TextMeshProUGUI unitName;

	[SerializeField]
	private UnitLevelDisplay unitLevel;

	[SerializeField]
	private GameObject changeSelectedHeroButtonsParent;

	[SerializeField]
	private UnitStatWithRegenDisplay healthStatDisplay;

	[SerializeField]
	private UnitStatDisplay healthRegenStatDisplay;

	[SerializeField]
	private UnitStatWithRegenDisplay manaStatDisplay;

	[SerializeField]
	private UnitStatDisplay manaRegenStatDisplay;

	[SerializeField]
	private UnitStatDisplay actionPointsStatDisplay;

	[SerializeField]
	private UnitStatDisplay movementPointsDisplay;

	[SerializeField]
	private UnitStatDisplay overallDamageStatDisplay;

	[SerializeField]
	private AdditionalUnitStatDisplay criticalStatDisplay;

	[SerializeField]
	private UnitStatDisplay resistanceReductionStatDisplay;

	[SerializeField]
	private UnitStatDisplay accuracyStatDisplay;

	[SerializeField]
	private UnitStatDisplay blockStatDisplay;

	[SerializeField]
	private UnitStatDisplay armorStatDisplay;

	[SerializeField]
	private UnitStatDisplay resistanceStatDisplay;

	[SerializeField]
	private UnitStatDisplay dodgeStatDisplay;

	[SerializeField]
	private Image avatarImage;

	[SerializeField]
	private ItemTooltip itemTooltip;

	[SerializeField]
	private Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlots = new Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>>();

	[SerializeField]
	private Dictionary<ItemSlotDefinition.E_ItemSlotId, Image> slotsBackgroundImages = new Dictionary<ItemSlotDefinition.E_ItemSlotId, Image>();

	[SerializeField]
	private HideHelmetView hideHelmetView;

	[SerializeField]
	private SkillListDisplay equippedSkillsDisplay;

	[SerializeField]
	private Image characterDetailsNotif;

	[SerializeField]
	private UnitPortraitView unitPortaitDetails;

	[SerializeField]
	private UnitPerkTreeView unitPerkTreeView;

	[SerializeField]
	private Image perkAvailableNotif;

	[SerializeField]
	private UnitRaceDisplay unitRaceDisplay;

	[SerializeField]
	private Selectable nextHeroButtonSelectable;

	[SerializeField]
	private HUDJoystickTarget rightPanelJoystickTarget;

	[SerializeField]
	private HUDJoystickTarget inventoryJoystickTarget;

	[SerializeField]
	private HUDJoystickTarget perksJoystickTarget;

	[SerializeField]
	private HUDJoystickSimpleTarget closeJoystickTarget;

	[SerializeField]
	private HUDJoystickTarget joystickTarget;

	[SerializeField]
	private Selectable movePointsSelectable;

	[SerializeField]
	private HUDJoystickTarget unitRaceJoystickTarget;

	[SerializeField]
	private AudioClip openClip;

	[SerializeField]
	private AudioClip closeClip;

	private Canvas canvas;

	private Tweener displayTween;

	private bool initialized;

	private RectTransform rectTransform;

	private float startY;

	public static Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> EquipmentSlots => TPSingleton<CharacterSheetPanel>.Instance.equipmentSlots;

	public bool IsDisplayTweenPlaying => displayTween?.IsPlaying() ?? false;

	public static bool HasClosedThisFrame { get; private set; }

	public static ItemTooltip ItemTooltip => TPSingleton<CharacterSheetPanel>.Instance.itemTooltip;

	public bool IsInventoryOpened => toggleInventory.isOn;

	public bool IsPerksPanelOpened => togglePerk.isOn;

	public bool IsOpened { get; private set; }

	public int OverlaySortingOrder => canvas.sortingOrder - 2;

	public EquipmentSlotView FocusedEquipmentSlotView { get; set; }

	public HUDJoystickTarget PerksJoystickTarget => perksJoystickTarget;

	public HUDJoystickTarget RightPanelJoystickTarget => rightPanelJoystickTarget;

	public UnitPerkTreeView UnitPerkTreeView => unitPerkTreeView;

	public UnitRaceDisplay UnitRaceDisplay => unitRaceDisplay;

	public event Action<bool> OnCharacterSheetToggle;

	public static void Init()
	{
		if (TPSingleton<CharacterSheetPanel>.Instance.initialized)
		{
			return;
		}
		TPSingleton<CharacterSheetPanel>.Instance.initialized = true;
		TPSingleton<CharacterSheetPanel>.Instance.canvas = TPSingleton<CharacterSheetPanel>.Instance.GetComponent<Canvas>();
		TPSingleton<CharacterSheetPanel>.Instance.canvas.enabled = false;
		TPSingleton<CharacterSheetPanel>.Instance.rectTransform = TPSingleton<CharacterSheetPanel>.Instance.GetComponent<RectTransform>();
		TPSingleton<CharacterSheetPanel>.Instance.startY = TPSingleton<CharacterSheetPanel>.Instance.rectTransform.anchoredPosition.y;
		if (TPSingleton<CharacterSheetPanel>.Instance.tabPagePairs == null)
		{
			CLoggerManager.Log("[CharacterSheetPanel] tabPagePairs is null!", LogType.Error);
		}
		else
		{
			foreach (KeyValuePair<Toggle, TabbedPageView> tabbedPage in TPSingleton<CharacterSheetPanel>.Instance.tabPagePairs)
			{
				tabbedPage.Key.onValueChanged.AddListener(delegate(bool value)
				{
					TPSingleton<CharacterSheetPanel>.Instance.TabbedPageToggle_ValueChanged(tabbedPage.Key, value);
				});
			}
		}
		TPSingleton<CharacterSheetPanel>.Instance.healthStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
		TPSingleton<CharacterSheetPanel>.Instance.healthStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
		TPSingleton<CharacterSheetPanel>.Instance.healthStatDisplay.RegenStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthRegen];
		TPSingleton<CharacterSheetPanel>.Instance.healthRegenStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthRegen];
		TPSingleton<CharacterSheetPanel>.Instance.manaStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Mana];
		TPSingleton<CharacterSheetPanel>.Instance.manaStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaTotal];
		TPSingleton<CharacterSheetPanel>.Instance.manaStatDisplay.RegenStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaRegen];
		TPSingleton<CharacterSheetPanel>.Instance.manaRegenStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaRegen];
		TPSingleton<CharacterSheetPanel>.Instance.actionPointsStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPoints];
		TPSingleton<CharacterSheetPanel>.Instance.actionPointsStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPointsTotal];
		TPSingleton<CharacterSheetPanel>.Instance.movementPointsDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePoints];
		TPSingleton<CharacterSheetPanel>.Instance.movementPointsDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePointsTotal];
		TPSingleton<CharacterSheetPanel>.Instance.overallDamageStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.OverallDamage];
		TPSingleton<CharacterSheetPanel>.Instance.criticalStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Critical];
		TPSingleton<CharacterSheetPanel>.Instance.criticalStatDisplay.AdditionalStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.CriticalPower];
		TPSingleton<CharacterSheetPanel>.Instance.resistanceReductionStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ResistanceReduction];
		TPSingleton<CharacterSheetPanel>.Instance.accuracyStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Accuracy];
		TPSingleton<CharacterSheetPanel>.Instance.blockStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Block];
		TPSingleton<CharacterSheetPanel>.Instance.armorStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Armor];
		TPSingleton<CharacterSheetPanel>.Instance.armorStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ArmorTotal];
		TPSingleton<CharacterSheetPanel>.Instance.resistanceStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Resistance];
		TPSingleton<CharacterSheetPanel>.Instance.dodgeStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Dodge];
	}

	public void Close(bool toAnotherPopup = false)
	{
		if (!IsOpened)
		{
			return;
		}
		IsOpened = false;
		this.OnCharacterSheetToggle?.Invoke(obj: false);
		if (tabPagePairs.Count((KeyValuePair<Toggle, TabbedPageView> x) => x.Key.isOn) == 0)
		{
			return;
		}
		if (!toAnotherPopup)
		{
			CameraView.AttenuateWorldForPopupFocus(null);
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OnPopupExitToWorld();
			}
		}
		if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
		{
			TPSingleton<UnitLevelUpView>.Instance.Close();
		}
		if (TPSingleton<ToDoListView>.Instance.IsDisplayed && IsInventoryOpened)
		{
			TPSingleton<ToDoListView>.Instance.CloseInventoryNotification();
		}
		displayTween?.Kill();
		if (toAnotherPopup)
		{
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startY);
			canvas.enabled = false;
		}
		else
		{
			displayTween = rectTransform.DOAnchorPosY(startY, 0.25f, snapping: true).SetEase(Ease.InBack).OnComplete(delegate
			{
				canvas.enabled = false;
			})
				.OnKill(delegate
				{
					displayTween = null;
				});
		}
		InventoryManager.InventoryView.DraggableItem.Reset();
		DeactivateSlots();
		StartCoroutine(CloseCoroutine());
		SoundManager.PlayAudioClip(closeClip, UIManager.PooledAudioSourceData);
	}

	public void OnChangeWeaponSetButtonClick()
	{
		TPSingleton<PlayableUnitManager>.Instance.ChangeEquipment();
	}

	public void OnCloseButtonClick()
	{
		CharacterSheetManager.CloseCharacterSheetPanel();
	}

	public void OnNextUnitButtonClick()
	{
		OnSelectNewUnitButtonClick(next: true);
	}

	public void OnPreviousUnitButtonClick()
	{
		OnSelectNewUnitButtonClick(next: false);
	}

	private void OnSelectNewUnitButtonClick(bool next)
	{
		if (TPSingleton<UnitLevelUpView>.Instance.IsProceedingToALevelUp)
		{
			return;
		}
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace != null)
		{
			if (!InputManager.JoystickConfig.HUDNavigation.CanChangeHeroWhileEquipmentSlotSelection)
			{
				return;
			}
			OnEquipmentSlotJoystickSelectionOver();
		}
		else if (TPSingleton<UnitLevelUpView>.Instance.IsOpened && !IsInventoryOpened && !IsPerksPanelOpened && InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(GameView.CharacterDetailsView.SecondaryAttributesHUDJoystickTarget.GetSelectionInfo());
		}
		bool num = InputManager.IsLastControllerJoystick && EventSystem.current.currentSelectedGameObject == GameView.CharacterDetailsView.LevelUpButton.gameObject;
		InventoryManager.InventoryView.DraggableItem.Reset();
		PlayableUnitManager.SelectNewUnit(next);
		if (IsPerksPanelOpened && UnitPerkTreeView.IsInRerollMode)
		{
			UnitPerkTreeView.CancelReroll();
		}
		Refresh();
		RefreshLevelUpPanel();
		if (num)
		{
			StartCoroutine(LevelUpButtonJoystickDeselectedCoroutine());
		}
		if (InputManager.IsLastControllerJoystick && !TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.IsMoving)
		{
			StartCoroutine(UpdateJoystickHighlightPositionEndOfFrame());
		}
	}

	private IEnumerator UpdateJoystickHighlightPositionEndOfFrame()
	{
		yield return null;
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ForcePositionUpdate();
	}

	public void OnCustomisationUnitButtonClick()
	{
		GameController.SetState(Game.E_State.UnitCustomisation);
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.Open(TileObjectSelectionManager.SelectedPlayableUnit);
	}

	public void OnGameStateChange(Game.E_State state, Game.E_State previousState)
	{
		if (state == Game.E_State.CharacterSheet && previousState == Game.E_State.UnitCustomisation && InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(rightPanelJoystickTarget.GetSelectionInfo());
		}
	}

	public void OnUnitLevelButtonClick(bool shouldRefreshTooltip = true)
	{
		if (UnitLevelUpController.CanOpenUnitLevelUpView)
		{
			TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = TileObjectSelectionManager.SelectedPlayableUnit.LevelUp;
			TPSingleton<UnitLevelUpView>.Instance.Open();
			unitLevel.RefreshButton(interactable: false);
			if (shouldRefreshTooltip)
			{
				unitLevel.UnitExperienceTooltipDisplayer.Refresh();
			}
		}
	}

	public void Open(bool instant = false, PlayableUnit playableUnit = null)
	{
		if (IsOpened)
		{
			return;
		}
		IsOpened = true;
		this.OnCharacterSheetToggle?.Invoke(obj: true);
		if (complexFontLocalizedParent != null)
		{
			complexFontLocalizedParent.TargetKey = "Default";
			complexFontLocalizedParent.RefreshChildren();
			complexFontLocalizedParent.TargetKey = "LeftPanel";
			complexFontLocalizedParent.RefreshChildren();
		}
		CameraView.AttenuateWorldForPopupFocus(this);
		UIManager.HideInfoPanels();
		displayTween?.Kill();
		if (instant)
		{
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, panelDeltaPosY);
		}
		else
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
			displayTween = rectTransform.DOAnchorPosY(panelDeltaPosY, 0.25f, snapping: true).SetEase(Ease.OutBack).OnKill(delegate
			{
				displayTween = null;
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			})
				.OnComplete(delegate
				{
					TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
				});
		}
		canvas.enabled = true;
		ActivateSlots();
		if (playableUnit == null)
		{
			Refresh();
		}
		else
		{
			RefreshWith(playableUnit);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			closeJoystickTarget.ClearUnavailableNavigations();
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
		}
		SoundManager.PlayAudioClip(openClip, UIManager.PooledAudioSourceData);
	}

	public void OpenUnitDetails()
	{
		if (toggleUnitDetails.isOn)
		{
			toggleUnitDetails.isOn = false;
		}
		toggleUnitDetails.isOn = true;
		if (complexFontLocalizedParent != null)
		{
			complexFontLocalizedParent.TargetKey = "CharacterDetails";
			complexFontLocalizedParent.RefreshChildren();
		}
	}

	public void OpenInventory()
	{
		if (TPSingleton<ToDoListView>.Instance.IsDisplayed)
		{
			TPSingleton<ToDoListView>.Instance.CloseInventoryNotification();
		}
		TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.MarkAllItemsAsSeen();
		StartCoroutine(TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ResetScrollbarAfterAFrame());
		if (toggleInventory.isOn)
		{
			toggleInventory.isOn = false;
		}
		toggleInventory.isOn = true;
		if (complexFontLocalizedParent != null)
		{
			complexFontLocalizedParent.TargetKey = "InventoryPanel";
			complexFontLocalizedParent.RefreshChildren();
		}
	}

	public void OpenPerkTree()
	{
		if (togglePerk.isOn)
		{
			togglePerk.isOn = false;
		}
		togglePerk.isOn = true;
		if (complexFontLocalizedParent != null)
		{
			complexFontLocalizedParent.TargetKey = "PerksPanel";
			complexFontLocalizedParent.RefreshChildren();
		}
	}

	public void Refresh()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("No unit selected to refresh CharacterSheetPanel!");
		}
		else
		{
			RefreshWith(TileObjectSelectionManager.SelectedPlayableUnit);
		}
	}

	public void RefreshAvatar(PlayableUnit unit = null)
	{
		if (unit == null)
		{
			if (!TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TPSingleton<PlayableUnitManager>.Instance.LogError("No unit selected!");
				return;
			}
			unit = TileObjectSelectionManager.SelectedPlayableUnit;
		}
		avatarImage.sprite = unit.UiSprite;
	}

	public void RefreshCharacterDetailsNotif(PlayableUnit playableUnit)
	{
		characterDetailsNotif.enabled = playableUnit != null && playableUnit.StatsPoints > 0;
	}

	public void RefreshEquipmentSlotsValidity()
	{
		DraggedItem draggableItem = InventoryManager.InventoryView.DraggableItem;
		TheLastStand.Model.Item.Item obj = TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.FocusedInventorySlotView?.ItemSlot?.Item;
		bool isDragging = draggableItem != null && draggableItem.Displayed;
		bool isHovering = obj != null;
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				equipmentSlot.Value[i].RefreshSlotValidity(isDragging, isHovering);
			}
		}
	}

	public void OnEquipmentSlotJoystickSelectionOver()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.StartCoroutine(TPSingleton<HUDJoystickNavigationManager>.Instance.ToggleSlotSelectionCoroutine());
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				equipmentSlot.Value[i].RestoreJoystickNavigation();
			}
		}
		EventSystem.current.SetSelectedGameObject(TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace.InventorySlotView.gameObject);
		TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace = null;
		RefreshEquipmentSlotsValidity();
	}

	public void RefreshName(PlayableUnit playableUnit)
	{
		unitName.text = playableUnit.Name;
	}

	public void RefreshOpenedPage()
	{
		foreach (KeyValuePair<Toggle, TabbedPageView> tabPagePair in tabPagePairs)
		{
			if (tabPagePair.Value != null && tabPagePair.Key.isOn)
			{
				tabPagePair.Value.IsDirty = true;
			}
		}
	}

	public void RefreshPerkAvailableNotif(PlayableUnit playableUnit)
	{
		perkAvailableNotif.enabled = playableUnit != null && playableUnit.PerksPoints > 0 && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver;
	}

	public void RefreshPortrait(PlayableUnit playableUnit)
	{
		unitPortaitDetails.PlayableUnit = playableUnit;
		unitPortaitDetails.RefreshPortrait();
	}

	public void RefreshRaceIcon(PlayableUnit playableUnit)
	{
		unitRaceDisplay.SetContent(playableUnit?.RaceDefinition, playableUnit);
		unitRaceDisplay.Refresh();
	}

	public void RefreshSkills(PlayableUnit playableUnit)
	{
		List<TheLastStand.Model.Skill.Skill> skills = playableUnit.PlayableUnitController.GetSkills();
		equippedSkillsDisplay.SetSkills(skills, playableUnit);
	}

	public void RefreshStats()
	{
		healthStatDisplay.Refresh();
		healthRegenStatDisplay.Refresh();
		manaStatDisplay.Refresh();
		manaRegenStatDisplay.Refresh();
		actionPointsStatDisplay.Refresh();
		movementPointsDisplay.Refresh();
		overallDamageStatDisplay.Refresh();
		criticalStatDisplay.Refresh();
		resistanceReductionStatDisplay.Refresh();
		accuracyStatDisplay.Refresh();
		blockStatDisplay.Refresh();
		armorStatDisplay.Refresh();
		resistanceStatDisplay.Refresh();
		dodgeStatDisplay.Refresh();
	}

	public void RefreshUnitHeader(PlayableUnit playableUnit)
	{
		RefreshName(playableUnit);
		unitLevel.PlayableUnit = playableUnit;
		unitLevel.Refresh();
	}

	public void SetBackgroundColorForSlot(ItemSlotDefinition.E_ItemSlotId slotId, Color color)
	{
		if (slotsBackgroundImages != null && slotsBackgroundImages.TryGetValue(slotId, out var value))
		{
			value.color = color;
		}
	}

	private void ActivateSlots()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			foreach (EquipmentSlotView item in equipmentSlot.Value)
			{
				item.enabled = true;
			}
		}
		foreach (InventorySlot inventorySlot in InventoryManager.InventoryView.Inventory.InventorySlots)
		{
			inventorySlot.InventorySlotView.enabled = true;
		}
	}

	private IEnumerator CloseCoroutine()
	{
		HasClosedThisFrame = true;
		yield return SharedYields.WaitForEndOfFrame;
		HasClosedThisFrame = false;
	}

	private void DeactivateSlots()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			foreach (EquipmentSlotView item in equipmentSlot.Value)
			{
				item.OnPointerExit(null);
				item.enabled = false;
			}
		}
		foreach (InventorySlot inventorySlot in InventoryManager.InventoryView.Inventory.InventorySlots)
		{
			inventorySlot.InventorySlotView.OnPointerExit(null);
			inventorySlot.InventorySlotView.enabled = false;
		}
	}

	private void RefreshEquipment(PlayableUnit playableUnit)
	{
		hideHelmetView.Refresh(playableUnit);
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				EquipmentSlotView equipmentSlotView = equipmentSlot.Value[i];
				if (!playableUnit.EquipmentSlots.TryGetValue(equipmentSlot.Key, out var value) || i >= value.Count)
				{
					equipmentSlotView.ItemSlot = null;
				}
				else if (equipmentSlotView.ItemSlot != null && !equipmentSlotView.gameObject.activeInHierarchy)
				{
					equipmentSlotView.gameObject.SetActive(value: true);
				}
				equipmentSlotView.Refresh();
			}
		}
		List<TheLastStand.Model.Skill.Skill> skills = playableUnit.PlayableUnitController.GetSkills();
		equippedSkillsDisplay.SetSkills(skills, playableUnit);
		RefreshCharacterDetailsNotif(playableUnit);
		RefreshPerkAvailableNotif(playableUnit);
		RefreshOpenedPage();
	}

	private void RefreshLevelUpPanel()
	{
		if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
		{
			TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = ((TileObjectSelectionManager.SelectedPlayableUnit.StatsPoints > 0) ? TileObjectSelectionManager.SelectedPlayableUnit.LevelUp : null);
			TPSingleton<UnitLevelUpView>.Instance.Reinitialize();
		}
	}

	private void RefreshNotifs(PlayableUnit playableUnit)
	{
		RefreshCharacterDetailsNotif(playableUnit);
		RefreshPerkAvailableNotif(playableUnit);
	}

	public void RefreshTabs()
	{
		toggleInventory.interactable = TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory();
		inventoryUnavailableParent.SetActive(!TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory());
		inventoryUnavailableText.SetActive(TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver);
		togglePerk.interactable = true;
		perkUnavailableParent.SetActive(value: false);
		perkUnavailableText.SetActive(value: false);
		changeSelectedHeroButtonsParent.SetActive(TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver);
	}

	private void RefreshWith(PlayableUnit playableUnit)
	{
		if (playableUnit == null)
		{
			CLoggerManager.Log("Tried to refresh CharacterSheet with a null playableUnit. Skipping refresh.", LogType.Warning);
			return;
		}
		RefreshStats();
		RefreshPortrait(playableUnit);
		RefreshRaceIcon(playableUnit);
		RefreshUnitHeader(playableUnit);
		RefreshTabs();
		RefreshAvatar(playableUnit);
		RefreshEquipment(playableUnit);
		RefreshSkills(playableUnit);
		RefreshNotifs(playableUnit);
		RefreshOpenedPage();
		if (PlayableUnitManager.StatTooltip.Displayed)
		{
			PlayableUnitManager.StatTooltip.Refresh();
		}
		if (PlayableUnitManager.RaceTooltip.Displayed)
		{
			PlayableUnitManager.RaceTooltip.Refresh();
		}
	}

	private void Update()
	{
		if (!(ApplicationManager.Application.State is GameState))
		{
			return;
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver)
		{
			if (!IsOpened)
			{
				return;
			}
			if (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(80))
			{
				CharacterSheetManager.CloseCharacterSheetPanel();
			}
			else if (InputManager.GetButtonDown(88))
			{
				if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					SelectNeighbourTab(next: true);
				}
			}
			else if (InputManager.GetButtonDown(89) && TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
			{
				SelectNeighbourTab(next: false);
			}
		}
		else if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet)
		{
			if (InputManager.GetButtonDown(29))
			{
				CharacterSheetManager.CloseCharacterSheetPanel();
			}
			else if (InputManager.GetButtonDown(80) && InputManager.IsLastControllerJoystick)
			{
				if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace != null)
				{
					OnEquipmentSlotJoystickSelectionOver();
				}
				else if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
				{
					TPSingleton<UnitLevelUpView>.Instance.Close();
					TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(GameView.CharacterDetailsView.SecondaryAttributesHUDJoystickTarget.GetSelectionInfo());
				}
				else if (IsPerksPanelOpened && UnitPerkTreeView.IsInRerollMode)
				{
					UnitPerkTreeView.CancelReroll();
				}
				else
				{
					CharacterSheetManager.CloseCharacterSheetPanel();
				}
			}
			else if (InputManager.GetButtonDown(137))
			{
				if (IsPerksPanelOpened && UnitPerkTreeView.IsInRerollMode)
				{
					UnitPerkTreeView.CancelReroll();
				}
			}
			else if (InputManager.GetButtonDown(0))
			{
				OnNextUnitButtonClick();
			}
			else if (InputManager.GetButtonDown(11))
			{
				OnPreviousUnitButtonClick();
			}
			else if (InputManager.GetButtonDown(10))
			{
				if (IsInventoryOpened)
				{
					CharacterSheetManager.CloseCharacterSheetPanel();
				}
				else if (TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory())
				{
					OpenInventory();
				}
			}
			else if (InputManager.GetButtonDown(16))
			{
				if (toggleUnitDetails.isOn)
				{
					CharacterSheetManager.CloseCharacterSheetPanel();
				}
				else
				{
					OpenUnitDetails();
				}
			}
			else if (InputManager.GetButtonDown(88))
			{
				if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					SelectNeighbourTab(next: true);
				}
			}
			else if (InputManager.GetButtonDown(89))
			{
				if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					SelectNeighbourTab(next: false);
				}
			}
			else if (InputManager.GetButtonDown(94))
			{
				if ((!IsInventoryOpened || InputManager.JoystickConfig.HUDNavigation.CanLevelUpInInventoryTab) && !IsPerksPanelOpened && !TPSingleton<UnitLevelUpView>.Instance.IsOpened && TileObjectSelectionManager.SelectedPlayableUnit.LevelPoints > 0 && TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = TileObjectSelectionManager.SelectedPlayableUnit.LevelUp;
					TPSingleton<UnitLevelUpView>.Instance.Open();
				}
			}
			else if (InputManager.GetButtonDown(79))
			{
				if (!IsPerksPanelOpened)
				{
					if (FocusedEquipmentSlotView != null && InputManager.IsLastControllerJoystick)
					{
						FocusedEquipmentSlotView.OnJoystickSubmit();
					}
					return;
				}
				bool flag = false;
				if (IsPerksPanelOpened && InputManager.GetButtonDown(79))
				{
					if (UnitPerkTreeView.IsInRerollMode)
					{
						if (UnitPerkTreeView.RerollTargetSelected != null && UnitPerkTreeView.RerollTargetSelected.CanReroll())
						{
							UnitPerkTreeView.OnConfirmRerollButtonClick();
						}
					}
					else if (UnitPerkTreeView.SelectedPerk != null && !UnitPerkTreeView.SelectedPerk.Perk.Unlocked && UnitPerkTreeView.SelectedPerk.Perk.PerkTier.Available && UnitPerkTreeView.UnitPerkTree.CanBuyPerk())
					{
						unitPerkTreeView.OnTrainButtonClick();
						flag = true;
					}
				}
				if (InputManager.GetButtonDown(94) && (!IsInventoryOpened || InputManager.JoystickConfig.HUDNavigation.CanLevelUpInInventoryTab) && !flag && !TPSingleton<UnitLevelUpView>.Instance.IsOpened && TileObjectSelectionManager.SelectedPlayableUnit.LevelPoints > 0 && TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = TileObjectSelectionManager.SelectedPlayableUnit.LevelUp;
					TPSingleton<UnitLevelUpView>.Instance.Open();
				}
			}
			else if (InputManager.GetButtonDown(143) && IsPerksPanelOpened && TPSingleton<SinkManager>.Instance.IsSinkUnlocked)
			{
				if (!unitPerkTreeView.IsInRerollMode)
				{
					unitPerkTreeView.OnStartRerollButtonClick();
				}
				else
				{
					unitPerkTreeView.OnCancelRerollButtonClick();
				}
			}
			else
			{
				int unitIndexHotkeyPressed = TPSingleton<PlayableUnitManager>.Instance.GetUnitIndexHotkeyPressed();
				if (unitIndexHotkeyPressed != -1 && !InventoryManager.InventoryView.DraggableItem.Displayed && !TPSingleton<UnitLevelUpView>.Instance.IsProceedingToALevelUp)
				{
					TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed]);
					Refresh();
					RefreshLevelUpPanel();
				}
			}
		}
		else if (InputManager.GetButtonDown(10) && TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory())
		{
			TileObjectSelectionManager.EnsureUnitSelection();
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Shopping)
			{
				CharacterSheetManager.OpenCharacterSheetPanel();
				OpenInventory();
			}
			else
			{
				TPSingleton<BuildingManager>.Instance.Shop.ShopController.CloseShopPanel(toAnotherPopup: true);
				CharacterSheetManager.OpenCharacterSheetPanel(fromAnotherPopup: true, TPSingleton<BuildingManager>.Instance.Shop.UnitToCompareIndex);
				OpenInventory();
			}
		}
		else if (CharacterSheetManager.CanOpenCharacterSheetPanel() && InputManager.GetButtonDown(16))
		{
			TileObjectSelectionManager.EnsureUnitSelection();
			CharacterSheetManager.OpenCharacterSheetPanel();
			OpenUnitDetails();
		}
	}

	private void SelectNeighbourTab(bool next)
	{
		List<Toggle> list = tabPagePairs.Keys.Where((Toggle x) => x.interactable).ToList();
		int count = list.Count;
		for (int num = 0; num < count; num++)
		{
			if (list[num].isOn)
			{
				list[num].isOn = false;
				int index = (num + (next ? 1 : (-1))).Mod(count);
				list[index].isOn = true;
				break;
			}
		}
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(rightPanelJoystickTarget.GetSelectionInfo());
		}
	}

	private void TabbedPageToggle_ValueChanged(Toggle sender, bool value)
	{
		sender.transform.localPosition = new Vector3(sender.transform.localPosition.x, value ? tabPosOn.transform.localPosition.y : tabPosOff.transform.localPosition.y, sender.transform.localPosition.z);
		if (value)
		{
			tabPagePairs[sender].Open();
			RefreshRaceIconJoystickNavigation();
		}
		else
		{
			tabPagePairs[sender].Close();
		}
	}

	private IEnumerator LevelUpButtonJoystickDeselectedCoroutine()
	{
		yield return SharedYields.WaitForEndOfFrame;
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<UnitLevelUpView>.Instance.HudTarget.GetSelectionInfo());
			}
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(GameView.CharacterDetailsView.LevelUpButtonDisabledTarget.gameObject);
		}
	}

	private void RefreshRaceIconJoystickNavigation()
	{
		unitRaceDisplay.JoystickSelectable.SetMode(Navigation.Mode.Explicit);
		unitRaceDisplay.JoystickSelectable.ClearNavigation();
		unitRaceDisplay.JoystickSelectable.SetSelectOnLeft(movePointsSelectable);
		nextHeroButtonSelectable.SetSelectOnRight(null);
		unitRaceJoystickTarget.gameObject.SetActive(value: false);
		if (unitPerkTreeView.IsOpened)
		{
			unitRaceJoystickTarget.gameObject.SetActive(value: true);
			unitRaceDisplay.JoystickSelectable.SetSelectOnDown(unitPerkTreeView.JoystickTarget.GetSelectionInfo().Selectable);
			movePointsSelectable.SetSelectOnRight(unitRaceDisplay.JoystickSelectable);
			nextHeroButtonSelectable.SetSelectOnRight(unitRaceDisplay.JoystickSelectable);
			return;
		}
		if (GameView.CharacterDetailsView.IsOpened)
		{
			unitRaceJoystickTarget.gameObject.SetActive(value: true);
			unitRaceDisplay.JoystickSelectable.SetSelectOnRight(GameView.CharacterDetailsView.DismissHeroButtonSelectable);
			unitRaceDisplay.JoystickSelectable.SetSelectOnDown(GameView.CharacterDetailsView.LeftTraitSelectable);
			movePointsSelectable.SetSelectOnRight(unitRaceDisplay.JoystickSelectable);
			nextHeroButtonSelectable.SetSelectOnRight(unitRaceDisplay.JoystickSelectable);
		}
		if (IsInventoryOpened)
		{
			movePointsSelectable.SetSelectOnRight(inventoryJoystickTarget.GetSelectionInfo().Selectable);
		}
	}
}

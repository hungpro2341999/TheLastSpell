using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Shop;

public class ShopView : TPSingleton<ShopView>, IOverlayUser
{
	private static class Constants
	{
		public const string ItemCategoryResourcePathFormat = "View/Sprites/UI/ShopFilters/ShopFilters_{0}_On";

		public const string RerollAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_Shop";
	}

	[SerializeField]
	private CanvasGroup shopCanvasGroup;

	[SerializeField]
	private RectTransform shopShelvesParent;

	[SerializeField]
	private ShopSellRectView shopSellRectView;

	[SerializeField]
	private ShopSlotView shopSlotPrefab;

	[SerializeField]
	private GameObject shopShelfPrefab;

	[SerializeField]
	private UnitDropdownPanel unitDropdown;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	[SerializeField]
	private Transform shopInventoryItemsPanelTransform;

	[SerializeField]
	private Scrollbar shopInventoryScrollbar;

	[SerializeField]
	private Transform shopItemsPanelTransform;

	[SerializeField]
	private ScrollRect inventoryScrollRect;

	[SerializeField]
	private RectTransform inventoryViewport;

	[SerializeField]
	private ScrollRect shelvesScrollRect;

	[SerializeField]
	private Scrollbar shopScrollbar;

	[SerializeField]
	private RectTransform scrollViewContentRectTransform;

	[SerializeField]
	private RectTransform shopSlotsScrollViewport;

	[SerializeField]
	private BetterButton rerollButton;

	[SerializeField]
	private TextMeshProUGUI rerollPrice;

	[SerializeField]
	private DataColor validPriceColor;

	[SerializeField]
	private DataColor invalidPriceColor;

	[SerializeField]
	private RectTransform filtersContainer;

	[SerializeField]
	private TMP_Dropdown categoryFilterDropdown;

	[SerializeField]
	private RectTransform categoryFilterScrollRect;

	[SerializeField]
	private RectTransform categoryFilterItem;

	[SerializeField]
	private TMP_Dropdown sortTypeDropdown;

	[SerializeField]
	private RectTransform sortTypeScrollRect;

	[SerializeField]
	private RectTransform sortItem;

	[SerializeField]
	private float dropdownRectsAdditionalHeight = 14f;

	[SerializeField]
	private float disabledRerollDropdownsWidth = 339f;

	[SerializeField]
	private ItemDefinition.E_Category[] displayedCategories;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private AudioClip[] openClips;

	[SerializeField]
	private AudioClip[] sellClips;

	[SerializeField]
	private AudioClip[] buyClips;

	[SerializeField]
	private bool playCloseSound = true;

	[SerializeField]
	private AudioClip closeClip;

	[SerializeField]
	private HUDJoystickTarget joystickTarget;

	[SerializeField]
	private HUDJoystickSimpleTarget shelvesJoystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer shelvesNavigationInitializer;

	[SerializeField]
	private ShopInventoryToSlotsNavigation inventoryToSlotsNavigation;

	private int activeShelvesCount;

	private bool initialized;

	private float disabledRerollDropdownsWidthBase;

	private Tween fadeTween;

	private Canvas canvas;

	private List<GameObject> shopShelves = new List<GameObject>();

	private List<ShopSlot> orderedSlots = new List<ShopSlot>();

	private List<ShopSlot> soldOutSlots = new List<ShopSlot>();

	private string[] filtersKeys;

	private List<string> sortKeys;

	private int nextSoldItemClipIndex;

	private int nextBoughtItemClipIndex;

	private AudioClip[] rerollAudioClips;

	public bool HasActiveFilter => categoryFilterDropdown.value != 0;

	public bool HasActiveSort => sortTypeDropdown.value != 0;

	public DataColor InvalidPriceColor => invalidPriceColor;

	public Transform InventoryItemsPanelTransform => shopInventoryItemsPanelTransform;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public TheLastStand.Model.Building.Shop Shop { get; set; }

	public ShopSellRectView ShopSellRectView => shopSellRectView;

	public UnitDropdownPanel UnitDropdown => unitDropdown;

	public DataColor ValidPriceColor => validPriceColor;

	public HUDJoystickTarget JoystickTarget => joystickTarget;

	public static void Init()
	{
		if (!TPSingleton<ShopView>.Instance.initialized)
		{
			TPSingleton<ShopView>.Instance.canvas = TPSingleton<ShopView>.Instance.GetComponent<Canvas>();
			TPSingleton<ShopView>.Instance.canvas.enabled = false;
			TPSingleton<ShopView>.Instance.InitFilterDropdown();
			TPSingleton<ShopView>.Instance.InitSortDropdown();
			TPSingleton<ShopView>.Instance.shopScrollbar.onValueChanged.AddListener(TPSingleton<ShopView>.Instance.ResetShopScrollbar);
			TPSingleton<ShopView>.Instance.disabledRerollDropdownsWidthBase = TPSingleton<ShopView>.Instance.filtersContainer.sizeDelta.x;
			TPSingleton<ShopView>.Instance.rerollAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_Shop");
			TPSingleton<ShopView>.Instance.initialized = true;
		}
	}

	public void OnItemBought(ShopSlot shopSlot)
	{
		if (HasActiveSort)
		{
			shopSlot.ShopSlotView.transform.SetAsLastSibling();
		}
		if (HasActiveFilter)
		{
			shopSlot.ShopSlotView.gameObject.SetActive(value: false);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			StartCoroutine(RefreshJoystickNavigationEndOfFrame());
			if (HasActiveFilter)
			{
				StartCoroutine(RedirectSlotSelectionEndOfFrame(shopSlot));
			}
		}
		PlayBoughtItemSound();
	}

	public void OnItemSold(ShopSlot shopSlot)
	{
		if (HasActiveSort)
		{
			(TheLastStand.Model.Building.Shop.E_SortType sortType, int sortDirection) tuple = ParseSortDropdownData();
			TheLastStand.Model.Building.Shop.E_SortType item = tuple.sortType;
			int item2 = tuple.sortDirection;
			bool flag = false;
			orderedSlots.Remove(shopSlot);
			for (int i = 0; i < orderedSlots.Count; i++)
			{
				ShopSlot shopSlot2 = orderedSlots[(item2 > 0) ? i : (orderedSlots.Count - 1 - i)];
				if (shopSlot2.Item != null)
				{
					flag = item switch
					{
						TheLastStand.Model.Building.Shop.E_SortType.Rarity => !shopSlot2.IsSoldOut && shopSlot.Item.Rarity.CompareTo(shopSlot2.Item.Rarity) * item2 < 0, 
						TheLastStand.Model.Building.Shop.E_SortType.Level => !shopSlot2.IsSoldOut && shopSlot.Item.Level.CompareTo(shopSlot2.Item.Level) * item2 < 0, 
						TheLastStand.Model.Building.Shop.E_SortType.Price => !shopSlot2.IsSoldOut && (shopSlot.Item.HasBeenSoldBefore ? shopSlot.Item.SellingPrice : shopSlot.Item.FinalPrice).CompareTo(shopSlot2.Item.HasBeenSoldBefore ? shopSlot2.Item.SellingPrice : shopSlot2.Item.FinalPrice) * item2 < 0, 
						_ => flag, 
					};
					if (flag)
					{
						orderedSlots.Insert(i, shopSlot);
						shopSlot.ShopSlotView.transform.SetSiblingIndex(shopSlot2.ShopSlotView.transform.GetSiblingIndex());
						break;
					}
				}
			}
		}
		if (HasActiveFilter)
		{
			ItemDefinition.E_Category e_Category = ParseFilterDropdownCategory();
			shopSlot.ShopSlotView.gameObject.SetActive(e_Category.HasFlag(shopSlot.Item.ItemDefinition.Category));
		}
		if (InputManager.IsLastControllerJoystick)
		{
			StartCoroutine(RefreshJoystickNavigationEndOfFrame());
		}
		PlaySoldItemSound();
	}

	public ShopSlotView AddNewSlotView()
	{
		ShopSlotView shopSlotView = UnityEngine.Object.Instantiate(shopSlotPrefab, shopItemsPanelTransform);
		shopSlotView.gameObject.SetActive(value: true);
		inventoryToSlotsNavigation.ShelvesSlots.Add(shopSlotView.JoystickSelectable);
		return shopSlotView;
	}

	public void CheckShelves()
	{
		int num = 0;
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			if (Shop.ShopSlots[i].ShopSlotView.ShopSlot != null)
			{
				num++;
			}
		}
		num += 2;
		int num2 = Mathf.FloorToInt((float)num / 3f);
		if (activeShelvesCount < num2)
		{
			GameObject gameObject = FindFreeShelf();
			if (gameObject == null)
			{
				gameObject = UnityEngine.Object.Instantiate(shopShelfPrefab, shopShelvesParent);
			}
			else
			{
				gameObject.SetActive(value: true);
			}
			shopShelves.Add(gameObject);
			gameObject.transform.SetSiblingIndex(num2);
			activeShelvesCount++;
			LayoutRebuilder.ForceRebuildLayoutImmediate(shopShelvesParent);
			scrollViewContentRectTransform.sizeDelta = new Vector2(scrollViewContentRectTransform.sizeDelta.x, shopShelvesParent.sizeDelta.y);
		}
	}

	public void ClearShelves()
	{
		for (int i = 0; i < shopShelves.Count; i++)
		{
			shopShelves[i].SetActive(value: false);
		}
		activeShelvesCount = 0;
	}

	public void Close(bool toAnotherPopup = false)
	{
		CLoggerManager.Log("ShopView closed", this, LogType.Log, CLogLevel.DETAILED);
		CameraView.AttenuateWorldForPopupFocus(null);
		TPSingleton<ShopView>.Instance.fadeTween?.Kill();
		InventoryManager.InventoryView.DraggableItem.Reset();
		if (toAnotherPopup)
		{
			TPSingleton<ShopView>.Instance.shopCanvasGroup.alpha = 0f;
			DeactivateView();
		}
		else
		{
			TPSingleton<ShopView>.Instance.fadeTween = TPSingleton<ShopView>.Instance.shopCanvasGroup.DOFade(0f, 0.4f).SetEase(Ease.OutCubic).OnComplete(DeactivateView)
				.SetFullId("ShopFadeOut", this);
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OnPopupExitToWorld();
			}
		}
		TPSingleton<ShopView>.Instance.shopCanvasGroup.blocksRaycasts = false;
		TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Hide();
		PlayCloseSound();
	}

	public void OnBotButtonClick()
	{
		shopScrollbar.value = Mathf.Clamp01(shopScrollbar.value - scrollButtonsSensitivity);
	}

	public void OnTopButtonClick()
	{
		shopScrollbar.value = Mathf.Clamp01(shopScrollbar.value + scrollButtonsSensitivity);
	}

	public void OnCloseButtonClick()
	{
		Shop.ShopController.CloseShopPanel();
	}

	public void OnGoldChanged(int gold)
	{
		RefreshPrices();
		RefreshRerollButton();
	}

	public void OnInventoryBotButtonClick()
	{
		shopInventoryScrollbar.value -= InventoryManager.InventoryView.ScrollButtonsSensitivity;
	}

	public void OnInventoryButtonClick()
	{
		if (TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory())
		{
			Shop.ShopController.CloseShopPanel(toAnotherPopup: true);
			TileObjectSelectionManager.EnsureUnitSelection();
			CharacterSheetManager.OpenCharacterSheetPanel(fromAnotherPopup: true, Shop.UnitToCompareIndex);
			TPSingleton<CharacterSheetPanel>.Instance.OpenInventory();
		}
	}

	public void OnInventoryTopButtonClick()
	{
		shopInventoryScrollbar.value += InventoryManager.InventoryView.ScrollButtonsSensitivity;
	}

	public void OnRerollButtonClick()
	{
		if (Shop.ShopController.TryToPayReroll())
		{
			SoundManager.PlayAudioClip(rerollAudioClips.PickRandom());
			OnShopReroll();
		}
	}

	public void OnShelvesItemSelected(RectTransform itemRectTransform)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			GUIHelpers.AdjustScrollViewToFocusedItem(itemRectTransform, shopSlotsScrollViewport, shopScrollbar, 0.01f, 0.01f);
		}
	}

	public void OnInventoryItemSelected(RectTransform itemRectTransform)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			GUIHelpers.AdjustScrollViewToFocusedItem(itemRectTransform, inventoryViewport, shopInventoryScrollbar, 0.01f, 0.01f);
		}
	}

	public void Open(bool instant = false)
	{
		CLoggerManager.Log("ShopView opened", this, LogType.Log, CLogLevel.DETAILED);
		CameraView.AttenuateWorldForPopupFocus(this);
		fadeTween?.Kill();
		if (fontLocalizedParent != null)
		{
			fontLocalizedParent.RefreshChildren();
		}
		shopInventoryScrollbar.value = 1f;
		RefreshPrices();
		RefreshRerollButton();
		LayoutRebuilder.ForceRebuildLayoutImmediate(shopShelvesParent);
		scrollViewContentRectTransform.sizeDelta = new Vector2(scrollViewContentRectTransform.sizeDelta.x, shopShelvesParent.sizeDelta.y);
		if (instant)
		{
			shopCanvasGroup.alpha = 1f;
		}
		else
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
			fadeTween = shopCanvasGroup.DOFade(1f, 0.4f).SetEase(Ease.OutCubic).SetFullId("ShopFadeIn", this)
				.OnComplete(delegate
				{
					TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
				});
		}
		ActivateAllSlots();
		canvas.enabled = true;
		ToggleScrollRects(enable: true);
		shopScrollbar.value = 1f;
		shopCanvasGroup.blocksRaycasts = true;
		unitDropdown.ResetDropdown(Shop.UnitToCompareIndex);
		UIManager.GenericTooltip.Hide();
		PlayOpenSound();
		if (InputManager.IsLastControllerJoystick)
		{
			RefreshJoystickNavigation();
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(JoystickTarget.GetSelectionInfo());
		}
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnShopOpen);
	}

	public void RefreshPrices()
	{
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			Shop.ShopSlots[i].ShopSlotView.RefreshPriceColor();
			Shop.ShopSlots[i].ShopSlotView.RefreshLocalizedFonts();
		}
	}

	public void ResetSort()
	{
		orderedSlots.Clear();
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			orderedSlots.Add(Shop.ShopSlots[i]);
			Shop.ShopSlots[i].ShopSlotView.transform.SetAsLastSibling();
		}
	}

	public void RefreshRerollButton()
	{
		if (!MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockShopRerollMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated) || effects.Length == 0)
		{
			rerollButton.gameObject.SetActive(value: false);
			filtersContainer.sizeDelta = new Vector2(disabledRerollDropdownsWidth, filtersContainer.sizeDelta.y);
			return;
		}
		rerollButton.gameObject.SetActive(value: true);
		filtersContainer.sizeDelta = new Vector2(disabledRerollDropdownsWidthBase, filtersContainer.sizeDelta.y);
		bool flag = EventSystem.current.currentSelectedGameObject == rerollButton.gameObject;
		int shopRerollPrice = Shop.ShopRerollPrice;
		rerollPrice.text = shopRerollPrice.ToString();
		bool flag2 = TPSingleton<ResourceManager>.Instance.Gold >= shopRerollPrice;
		rerollPrice.color = (flag2 ? ValidPriceColor._Color : InvalidPriceColor._Color);
		rerollButton.interactable = flag2;
		if (!flag2 && flag)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(rerollButton.gameObject);
		}
	}

	public void RefreshJoystickNavigation()
	{
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			Shop.ShopSlots[i].ShopSlotView.JoystickSelectable.ClearNavigation();
		}
		shelvesNavigationInitializer.InitNavigation();
		List<JoystickSelectable> list = (from o in Shop.ShopSlots
			select o.ShopSlotView.JoystickSelectable into o
			orderby o.transform.GetSiblingIndex()
			select o).ToList();
		List<JoystickSelectable> list2 = list.Where((JoystickSelectable o) => o.gameObject.activeInHierarchy).ToList();
		JoystickSelectable selectOnDown = ((list2.Count > 0) ? list2[0] : null);
		sortTypeDropdown.SetSelectOnDown(selectOnDown);
		rerollButton.SetSelectOnDown(selectOnDown);
		sortTypeDropdown.SetSelectOnLeft(rerollButton.gameObject.activeSelf ? rerollButton : null);
		categoryFilterDropdown.SetSelectOnLeft(rerollButton.gameObject.activeSelf ? rerollButton : null);
		for (int num = 0; num < Mathf.Min(3, list2.Count); num++)
		{
			list2[num].SetSelectOnUp(sortTypeDropdown);
		}
		JoystickSelectable joystickSelectable = InventoryItemsPanelTransform.GetChild(0).GetComponent<ShopInventorySlotView>().JoystickSelectable;
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			if (list2[num2].navigation.selectOnRight == null)
			{
				list2[num2].SetSelectOnRight(joystickSelectable);
			}
		}
		shelvesJoystickTarget.ClearSelectables();
		shelvesJoystickTarget.AddSelectables(list);
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void ActivateAllSlots()
	{
		foreach (ShopSlot shopSlot in Shop.ShopSlots)
		{
			shopSlot.ShopSlotView.enabled = true;
			shopSlot.ShopSlotView.Toggle(toggle: true);
		}
		ToggleAllShopInventorySlots(toggle: true);
	}

	private void ClearFilters()
	{
		categoryFilterDropdown.value = 0;
		sortTypeDropdown.value = 0;
	}

	private void DeactivateAllSlots()
	{
		foreach (ShopSlot shopSlot in Shop.ShopSlots)
		{
			shopSlot.ShopSlotView.OnPointerExit(null);
			shopSlot.ShopSlotView.enabled = false;
			shopSlot.ShopSlotView.Toggle(toggle: false);
		}
		ToggleAllShopInventorySlots(toggle: false);
	}

	private void DeactivateView()
	{
		canvas.enabled = false;
		ToggleScrollRects(enable: false);
		ClearFilters();
		DeactivateAllSlots();
	}

	private void FilterByCategory(ItemDefinition.E_Category category)
	{
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			ShopSlot shopSlot = Shop.ShopSlots[i];
			bool active = shopSlot.Item != null && (category == ItemDefinition.E_Category.All || (!shopSlot.IsSoldOut && category.HasFlag(shopSlot.Item.ItemDefinition.Category)));
			shopSlot.ShopSlotView.gameObject.SetActive(active);
		}
	}

	private GameObject FindFreeShelf()
	{
		for (int i = 0; i < shopShelves.Count; i++)
		{
			if (!shopShelves[i].activeSelf)
			{
				return shopShelves[i];
			}
		}
		return null;
	}

	private void InitFilterDropdown()
	{
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		filtersKeys = new string[displayedCategories.Length];
		for (int i = 0; i < displayedCategories.Length; i++)
		{
			string localizationKey = displayedCategories[i].GetLocalizationKey();
			filtersKeys[i] = localizationKey;
			TMP_Dropdown.OptionData item = new TMP_Dropdown.OptionData
			{
				text = Localizer.Get(localizationKey),
				image = ((displayedCategories[i] != ItemDefinition.E_Category.All && displayedCategories[i] != ItemDefinition.E_Category.None) ? ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/ShopFilters/ShopFilters_{displayedCategories[i]}_On") : null)
			};
			list.Add(item);
		}
		categoryFilterScrollRect.sizeDelta = new Vector2(categoryFilterScrollRect.sizeDelta.x, categoryFilterItem.rect.height * (float)list.Count + dropdownRectsAdditionalHeight);
		categoryFilterDropdown.options = list;
		categoryFilterDropdown.onValueChanged.AddListener(OnCategoryFilterDropdownValueChanged);
	}

	private void InitInventorySlots()
	{
		for (int i = 0; i < InventoryItemsPanelTransform.childCount; i++)
		{
			InventoryItemsPanelTransform.GetChild(i).GetComponent<ShopInventorySlotView>().ItemIndex = i;
		}
	}

	private void InitSortDropdown()
	{
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		TheLastStand.Model.Building.Shop.E_SortType[] array = (TheLastStand.Model.Building.Shop.E_SortType[])Enum.GetValues(typeof(TheLastStand.Model.Building.Shop.E_SortType));
		sortKeys = new List<string>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == TheLastStand.Model.Building.Shop.E_SortType.None)
			{
				string text = string.Format("{0}{1}", "Shop_Sort_", array[i]);
				sortKeys.Add(text);
				list.Add(new TMP_Dropdown.OptionData
				{
					text = Localizer.Get(text)
				});
				continue;
			}
			string text2 = string.Format("{0}{1}_{2}", "Shop_Sort_", array[i], "Ascending");
			string text3 = string.Format("{0}{1}_{2}", "Shop_Sort_", array[i], "Descending");
			sortKeys.Add(text2);
			sortKeys.Add(text3);
			TMP_Dropdown.OptionData item = new TMP_Dropdown.OptionData
			{
				text = Localizer.Get(text2),
				image = ResourcePooler.LoadOnce<Sprite>(string.Format("View/Sprites/UI/ShopFilters/ShopFilters_{0}_On", string.Format("{0}{1}", array[i], "Ascending")))
			};
			TMP_Dropdown.OptionData item2 = new TMP_Dropdown.OptionData
			{
				text = Localizer.Get(text3),
				image = ResourcePooler.LoadOnce<Sprite>(string.Format("View/Sprites/UI/ShopFilters/ShopFilters_{0}_On", string.Format("{0}{1}", array[i], "Descending")))
			};
			list.Add(item);
			list.Add(item2);
		}
		sortTypeScrollRect.sizeDelta = new Vector2(sortTypeScrollRect.sizeDelta.x, sortItem.rect.height * (float)list.Count + dropdownRectsAdditionalHeight);
		sortTypeDropdown.options = list;
		sortTypeDropdown.onValueChanged.AddListener(OnSortDropdownValueChanged);
	}

	private void OnCategoryFilterDropdownValueChanged(int value)
	{
		ItemDefinition.E_Category category = ParseFilterDropdownCategory();
		FilterByCategory(category);
		if (!HasActiveFilter)
		{
			categoryFilterDropdown.captionText.text = Localizer.Get("Shop_Filter_Title");
		}
		if (InputManager.IsLastControllerJoystick)
		{
			StartCoroutine(RefreshJoystickNavigationEndOfFrame());
		}
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		categoryFilterDropdown.onValueChanged.RemoveListener(OnCategoryFilterDropdownValueChanged);
		sortTypeDropdown.onValueChanged.RemoveListener(OnSortDropdownValueChanged);
	}

	private void OnLocalize()
	{
		for (int i = 0; i < filtersKeys.Length; i++)
		{
			categoryFilterDropdown.options[i].text = Localizer.Get(filtersKeys[i]);
		}
		for (int j = 0; j < sortKeys.Count; j++)
		{
			sortTypeDropdown.options[j].text = Localizer.Get(sortKeys[j]);
		}
		sortTypeDropdown.RefreshShownValue();
		categoryFilterDropdown.RefreshShownValue();
		if (!HasActiveSort)
		{
			sortTypeDropdown.captionText.text = Localizer.Get("Shop_Sort_Title");
		}
		if (!HasActiveFilter)
		{
			categoryFilterDropdown.captionText.text = Localizer.Get("Shop_Filter_Title");
		}
	}

	private void OnShopReroll()
	{
		if (HasActiveFilter)
		{
			ItemDefinition.E_Category category = ParseFilterDropdownCategory();
			FilterByCategory(category);
		}
		if (HasActiveSort)
		{
			var (sortType, sortDirection) = ParseSortDropdownData();
			Sort(sortType, sortDirection);
		}
		if (!InputManager.IsLastControllerJoystick)
		{
			return;
		}
		StartCoroutine(RefreshJoystickNavigationEndOfFrame());
		if (EventSystem.current.currentSelectedGameObject.activeInHierarchy)
		{
			return;
		}
		for (int num = shopItemsPanelTransform.childCount - 1; num >= 0; num--)
		{
			GameObject gameObject = shopItemsPanelTransform.GetChild(num).gameObject;
			if (gameObject.activeInHierarchy)
			{
				EventSystem.current.SetSelectedGameObject(gameObject);
				break;
			}
		}
	}

	private void OnSortDropdownValueChanged(int value)
	{
		var (sortType, sortDirection) = ParseSortDropdownData();
		Sort(sortType, sortDirection);
		if (!HasActiveSort)
		{
			sortTypeDropdown.captionText.text = Localizer.Get("Shop_Sort_Title");
		}
		if (InputManager.IsLastControllerJoystick)
		{
			StartCoroutine(RefreshJoystickNavigationEndOfFrame());
		}
	}

	private ItemDefinition.E_Category ParseFilterDropdownCategory()
	{
		return displayedCategories[categoryFilterDropdown.value];
	}

	private (TheLastStand.Model.Building.Shop.E_SortType sortType, int sortDirection) ParseSortDropdownData()
	{
		int value = sortTypeDropdown.value;
		TheLastStand.Model.Building.Shop.E_SortType item = (TheLastStand.Model.Building.Shop.E_SortType)Mathf.CeilToInt((float)value / 2f);
		int item2 = ((value % 2 != 0) ? 1 : (-1));
		return (sortType: item, sortDirection: item2);
	}

	private void PlaySoldItemSound()
	{
		AudioClip audioClip = sellClips[nextSoldItemClipIndex];
		int num = nextSoldItemClipIndex;
		do
		{
			nextSoldItemClipIndex = UnityEngine.Random.Range(0, sellClips.Length);
		}
		while (nextSoldItemClipIndex == num);
		SoundManager.PlayAudioClip(audioClip);
	}

	private void PlayBoughtItemSound()
	{
		AudioClip audioClip = buyClips[nextBoughtItemClipIndex];
		int num = nextBoughtItemClipIndex;
		do
		{
			nextBoughtItemClipIndex = UnityEngine.Random.Range(0, buyClips.Length);
		}
		while (nextBoughtItemClipIndex == num);
		SoundManager.PlayAudioClip(audioClip);
	}

	private void PlayOpenSound()
	{
		SoundManager.PlayAudioClip(openClips.PickRandom());
	}

	private void PlayCloseSound()
	{
		if (playCloseSound)
		{
			SoundManager.PlayAudioClip(closeClip);
		}
	}

	private IEnumerator RefreshJoystickNavigationEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		RefreshJoystickNavigation();
	}

	private IEnumerator RedirectSlotSelectionEndOfFrame(ShopSlot slot)
	{
		yield return SharedYields.WaitForEndOfFrame;
		TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(JoystickTarget.GetSelectionInfo());
	}

	private void ResetShopScrollbar(float value)
	{
		shopScrollbar.onValueChanged.RemoveListener(ResetShopScrollbar);
		shopScrollbar.value = 1f;
	}

	private void Sort(TheLastStand.Model.Building.Shop.E_SortType sortType, int sortDirection)
	{
		if (sortType == TheLastStand.Model.Building.Shop.E_SortType.None)
		{
			ResetSort();
			return;
		}
		orderedSlots.Clear();
		soldOutSlots.Clear();
		for (int i = 0; i < Shop.ShopSlots.Count; i++)
		{
			ShopSlot shopSlot = Shop.ShopSlots[i];
			if (shopSlot.Item != null)
			{
				if (shopSlot.IsSoldOut)
				{
					soldOutSlots.Add(Shop.ShopSlots[i]);
				}
				else
				{
					orderedSlots.Add(Shop.ShopSlots[i]);
				}
			}
		}
		switch (sortType)
		{
		case TheLastStand.Model.Building.Shop.E_SortType.Rarity:
			orderedSlots.Sort((ShopSlot a, ShopSlot b) => a.Item.Rarity.CompareTo(b.Item.Rarity));
			break;
		case TheLastStand.Model.Building.Shop.E_SortType.Level:
			orderedSlots.Sort((ShopSlot a, ShopSlot b) => a.Item.Level.CompareTo(b.Item.Level));
			break;
		case TheLastStand.Model.Building.Shop.E_SortType.Price:
			orderedSlots.Sort((ShopSlot a, ShopSlot b) => (a.Item.HasBeenSoldBefore ? a.Item.SellingPrice : a.Item.FinalPrice).CompareTo(b.Item.HasBeenSoldBefore ? b.Item.SellingPrice : b.Item.FinalPrice));
			break;
		}
		for (int num = 0; num < orderedSlots.Count; num++)
		{
			if (sortDirection == 1)
			{
				orderedSlots[num].ShopSlotView.transform.SetAsLastSibling();
			}
			else
			{
				orderedSlots[num].ShopSlotView.transform.SetAsFirstSibling();
			}
		}
		for (int num2 = 0; num2 < soldOutSlots.Count; num2++)
		{
			soldOutSlots[num2].ShopSlotView.transform.SetAsLastSibling();
		}
	}

	private void Start()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		InitInventorySlots();
		categoryFilterDropdown.captionText.text = Localizer.Get("Shop_Filter_Title");
		sortTypeDropdown.captionText.text = Localizer.Get("Shop_Sort_Title");
	}

	private void ToggleAllShopInventorySlots(bool toggle)
	{
		foreach (ShopInventorySlot shopInventorySlot in Shop.ShopInventorySlots)
		{
			shopInventorySlot.ShopInventorySlotView.Toggle(toggle);
		}
	}

	private void ToggleScrollRects(bool enable)
	{
		shelvesScrollRect.enabled = enable;
		inventoryScrollRect.enabled = enable;
	}

	private void Update()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping)
		{
			if (InputManager.GetButtonDown(29))
			{
				Shop.ShopController.CloseShopPanel();
			}
			else if (InputManager.GetButtonDown(101) && Shop.ShopController.TryToPayReroll())
			{
				OnShopReroll();
			}
		}
	}
}

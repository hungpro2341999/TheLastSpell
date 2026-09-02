using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Framework.UI.TMPro;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.NightReport;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

public class ChooseRewardPanel : TPSingleton<ChooseRewardPanel>, IOverlayUser
{
	private static class Constants
	{
		public const string AppearAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Appear";

		public const string DisappearAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Disappear";

		public const string SelectAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Select";
	}

	[SerializeField]
	[Range(0f, 3f)]
	private float delayBetweenItemApparition = 0.5f;

	[SerializeField]
	[Range(0f, 3f)]
	private float fadeInTweenDuration = 0.4f;

	[SerializeField]
	private Ease fadeInTweenEasing = Ease.OutCubic;

	[SerializeField]
	[Range(0f, 3f)]
	private float fadeOutTweenDuration = 0.4f;

	[SerializeField]
	private Ease fadeOutTweenEasing = Ease.OutCubic;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private ChooseRewardShelf chooseRewardShelf;

	[SerializeField]
	private RectMask2D shelvesMask;

	[SerializeField]
	private RectTransform shelvesContainer;

	[SerializeField]
	private TextMeshProUGUI productionBuildingText;

	[SerializeField]
	private GenericTooltipDisplayer tooltipDisplayer;

	[SerializeField]
	private UnitDropdownPanel unitDropdownPanel;

	[SerializeField]
	private TMP_BetterDropdown unitDropdown;

	[SerializeField]
	private Button previousUnitButton;

	[SerializeField]
	private Button nextUnitButton;

	[SerializeField]
	private CanvasGroup rerollCanvasGroup;

	[SerializeField]
	private BetterButton rerollButton;

	[SerializeField]
	private TextMeshProUGUI remainingRerollText;

	[SerializeField]
	private CanvasGroup rerollSinkCanvasGroup;

	[SerializeField]
	private BetterButton rerollSinkButton;

	[SerializeField]
	private TextMeshProUGUI rerollSinkCostText;

	[SerializeField]
	private GameObject damnedSoulsCountContainer;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsCountText;

	[SerializeField]
	private AudioSource chooseRewardAudioSource;

	[SerializeField]
	private AudioClip openAudioClip;

	[SerializeField]
	private DataColor validRemainingRerollColor;

	[SerializeField]
	private DataColor invalidRemainingRerollColor;

	[SerializeField]
	private DataColor damnedSoulsColor;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private HUDJoystickTarget joystickTarget;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private List<ChooseRewardShelf> chooseRewardShelves = new List<ChooseRewardShelf>();

	private Tween fadeTween;

	private bool isItemChanging;

	private Coroutine rerollCoroutine;

	private Coroutine selectPanelCoroutine;

	private bool firstFrameOpened = true;

	private AudioClip[] appearAudioClips;

	private AudioClip[] disappearAudioClips;

	private AudioClip[] selectAudioClips;

	public static AudioSource ChooseRewardAudioSource => TPSingleton<ChooseRewardPanel>.Instance.chooseRewardAudioSource;

	public bool IsOpened { get; set; }

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public ProductionItems ProductionItem { get; set; }

	public int UnitToCompareIndex { get; private set; } = -1;

	public HUDJoystickTarget JoystickTarget => joystickTarget;

	public event Action<bool> OnRewardPanelToggle;

	public void ChangeUnitToCompare(int newUnitIndex)
	{
		UnitToCompareIndex = newUnitIndex;
		if (newUnitIndex == -1)
		{
			TileObjectSelectionManager.DeselectUnit();
		}
		else
		{
			PlayableUnitManager.SelectUnitAtIndex(newUnitIndex);
		}
	}

	public void ChangeUnitToCompareAndResetDropdown(int newUnitIndex)
	{
		ChangeUnitToCompare(newUnitIndex);
		unitDropdownPanel.ResetDropdown(newUnitIndex + 1);
		if (ProductionItem == null)
		{
			return;
		}
		for (int i = 0; i < ProductionItem.Items.Count; i++)
		{
			if (chooseRewardShelves[i].RewardItemSlotView.HasFocus)
			{
				chooseRewardShelves[i].RewardItemSlotView.DisplayTooltip(display: true);
			}
		}
	}

	public void Close()
	{
		if (!IsOpened)
		{
			CLoggerManager.Log("ChooseRewardPanel is already closed", LogType.Error, CLogLevel.DETAILED);
			return;
		}
		IsOpened = false;
		if (selectPanelCoroutine != null)
		{
			StopCoroutine(selectPanelCoroutine);
		}
		this.OnRewardPanelToggle?.Invoke(obj: false);
		if (rerollCoroutine != null)
		{
			StopCoroutine(rerollCoroutine);
			rerollCoroutine = null;
		}
		isItemChanging = false;
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.ProductionReport:
			CameraView.AttenuateWorldForPopupFocus(TPSingleton<ProductionReportPanel>.Instance);
			break;
		case Game.E_State.NightReport:
			CameraView.AttenuateWorldForPopupFocus(TPSingleton<NightReportPanel>.Instance);
			break;
		default:
			CameraView.AttenuateWorldForPopupFocus(null);
			break;
		}
		InventoryManager.InventoryView.ItemTooltip.Hide();
		CharacterSheetPanel.ItemTooltip.Hide();
		fadeTween = canvasGroup.DOFade(0f, fadeOutTweenDuration).SetEase(fadeOutTweenEasing).SetFullId("ShopFadeOut", this)
			.OnComplete(delegate
			{
				canvas.enabled = false;
			});
		canvasGroup.blocksRaycasts = false;
		fadeTween.OnComplete(delegate
		{
			ToggleShelvesMask(toggle: false);
		});
		if (InputManager.IsLastControllerJoystick && !TPSingleton<ProductionReportPanel>.Instance.SelectFirstProduct())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
	}

	public void OnChosenItemChanged()
	{
		confirmButton.Interactable = ProductionItem.ChosenItem != null;
		if (ProductionItem.ChosenItem != null)
		{
			SoundManager.PlayAudioClip(selectAudioClips.PickRandom());
		}
		int i = 0;
		for (int count = chooseRewardShelves.Count; i < count; i++)
		{
			chooseRewardShelves[i].DisplaySelection();
		}
	}

	public void OnConfirm()
	{
		if (ProductionItem.ChosenItem != null)
		{
			ProductionItem.ProductionObjectController.ObtainContent();
			TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.RemoveProductionObject(ProductionItem);
			TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportPanel.RefreshGameObjects();
			TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportPanel.CheckOnProductionObjectHide();
			TPSingleton<ChooseRewardPanel>.Instance.Close();
		}
	}

	public void OnRerollButtonClick()
	{
		if (!isItemChanging)
		{
			rerollCoroutine = StartCoroutine(ReloadRerollRewardShelfs());
		}
	}

	public void Open()
	{
		if (IsOpened)
		{
			CLoggerManager.Log("ChooseRewardPanel is already opended", LogType.Error, CLogLevel.DETAILED);
			return;
		}
		firstFrameOpened = true;
		CameraView.AttenuateWorldForPopupFocus(this);
		TPSingleton<ChooseRewardPanel>.Instance.fadeTween?.Kill();
		SoundManager.PlayAudioClip(chooseRewardAudioSource, openAudioClip);
		IsOpened = true;
		this.OnRewardPanelToggle?.Invoke(obj: true);
		simpleFontLocalizedParent?.RefreshChildren();
		int num = PanicManager.Panic.PanicReward.PanicRewardController.ReloadBaseNbRerollReward();
		Refresh();
		ToggleShelvesMask(toggle: true);
		tooltipDisplayer.LocalizationArguments = new object[1] { num };
		canvas.enabled = true;
		canvasGroup.blocksRaycasts = true;
		TPSingleton<ChooseRewardPanel>.Instance.fadeTween = TPSingleton<ChooseRewardPanel>.Instance.canvasGroup.DOFade(1f, fadeInTweenDuration).SetEase(fadeInTweenEasing).SetFullId("ShopFadeIn", this);
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			unitDropdownPanel.ResetDropdown();
		}
		else
		{
			int num2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
			unitDropdownPanel.ResetDropdown(num2 + 1);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			selectPanelCoroutine = StartCoroutine(SelectPanelCoroutine());
		}
	}

	private bool IsAnyRewardJoystickSelected()
	{
		foreach (ChooseRewardShelf chooseRewardShelf in chooseRewardShelves)
		{
			if (EventSystem.current.currentSelectedGameObject == chooseRewardShelf.RewardItemSlotView.JoystickSelectable.gameObject)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator SelectPanelCoroutine()
	{
		float num = ((chooseRewardShelves.Count > 0) ? chooseRewardShelves[0].AppearTweenDuration : 0f);
		yield return new WaitForSeconds(num + fadeInTweenDuration + (float)ProductionItem.Items.Count * delayBetweenItemApparition);
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(JoystickTarget.GetSelectionInfo());
			EventSystem.current.SetSelectedGameObject(chooseRewardShelves[0].RewardItemSlotView.gameObject);
		}
	}

	private Navigation AddRewardToNavigation(Navigation navigation)
	{
		return new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = chooseRewardShelves[0].RewardItemSlotView.Selectable,
			selectOnRight = navigation.selectOnRight,
			selectOnLeft = navigation.selectOnLeft
		};
	}

	private void GetNewRewardItem()
	{
		if (ProductionItem.IsNightProduction)
		{
			PanicManager.Panic.PanicReward.PanicRewardController.GetReward();
			if (PanicManager.Panic.PanicReward.HasAtLeastOneItem)
			{
				ProductionNightRewardObject productionNightRewardObject = new ProductionNightRewardObjectController(PanicManager.Panic.PanicReward.Items).ProductionNightRewardObject;
				ProductionItem.Items.Clear();
				{
					foreach (TheLastStand.Model.Item.Item item2 in productionNightRewardObject.Items)
					{
						ProductionItem.Items.Add(item2);
					}
					return;
				}
			}
			TPSingleton<PanicManager>.Instance.LogError("No reward found after the RerollReward action !");
		}
		else
		{
			ProductionItem.Items.Clear();
			for (int i = 0; i < chooseRewardShelves.Count; i++)
			{
				TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(ItemSlotDefinition.E_ItemSlotId.None, ProductionItem.CreateItemDefinition, ProductionItem.LevelProbabilitiesTreeController.GenerateLevel());
				ProductionItem.Items.Add(item);
			}
		}
	}

	private void Refresh()
	{
		if (ProductionItem == null)
		{
			Close();
			return;
		}
		RefreshText();
		RefreshRerollButton();
		RefreshShelvesQuantity(ProductionItem.Items.Count);
		ProductionItem.ChosenItem = null;
		OnChosenItemChanged();
		for (int i = 0; i < ProductionItem.Items.Count; i++)
		{
			AudioClip appearClip = ((i == 0) ? appearAudioClips[0] : appearAudioClips[(i + 1) % 2 + 1]);
			chooseRewardShelves[i].ItemIndex = i;
			chooseRewardShelves[i].Refresh();
			chooseRewardShelves[i].Appear(fadeInTweenDuration + (float)i * delayBetweenItemApparition, appearClip);
		}
		SoundManager.PlayAudioClip(appearAudioClips[3], null, fadeInTweenDuration + (float)ProductionItem.Items.Count * delayBetweenItemApparition);
		confirmButton.Interactable = ProductionItem.ChosenItem != null;
	}

	public void RefreshRerollButton()
	{
		bool flag = PanicManager.Panic.PanicReward.PanicRewardController.IsInFreeRerollMode();
		rerollCanvasGroup.Display(flag);
		rerollSinkCanvasGroup.Display(!flag);
		rerollButton.Interactable = PanicManager.Panic.PanicReward.PanicRewardController.CanRerollFree();
		rerollSinkButton.Interactable = PanicManager.Panic.PanicReward.PanicRewardController.CanRerollWithEssence();
		if (flag)
		{
			remainingRerollText.text = $"x{PanicManager.Panic.PanicReward.RemainingNbRerollReward}";
			remainingRerollText.color = ((PanicManager.Panic.PanicReward.RemainingNbRerollReward > 0) ? validRemainingRerollColor._Color : invalidRemainingRerollColor._Color);
		}
		else
		{
			rerollSinkCostText.text = $"{PanicManager.Panic.PanicReward.DamnedSoulsRerollCost} <style=\"DamnedSouls\">";
			rerollSinkCostText.color = (PanicManager.Panic.PanicReward.PanicRewardController.CanRerollWithEssence() ? damnedSoulsColor._Color : invalidRemainingRerollColor._Color);
		}
		damnedSoulsCountContainer.SetActive(!flag);
		if (!flag)
		{
			damnedSoulsCountText.text = $"{ApplicationManager.Application.DamnedSouls} <style=\"DamnedSouls\">";
		}
	}

	private IEnumerator ReloadRerollRewardShelfs()
	{
		isItemChanging = true;
		if (PanicManager.Panic.PanicReward.PanicRewardController.CanRerollFree())
		{
			PanicManager.Panic.PanicReward.RemainingNbRerollReward--;
		}
		else
		{
			ApplicationManager.Application.DamnedSouls -= (uint)PanicManager.Panic.PanicReward.DamnedSoulsRerollCost;
			TPSingleton<SinkManager>.Instance.ItemRewardSinkData.Rerolls++;
		}
		GetNewRewardItem();
		RefreshRerollButton();
		if (ProductionItem.ChosenItem != null)
		{
			ProductionItem.ChosenItem = null;
			OnChosenItemChanged();
			yield return SharedYields.WaitForSeconds(delayBetweenItemApparition);
		}
		for (int i = 0; i < ProductionItem.Items.Count; i++)
		{
			AudioClip disappearClip = ((i == 0) ? disappearAudioClips[0] : disappearAudioClips[(i + 1) % 2 + 1]);
			chooseRewardShelves[i].Disappear((float)i * delayBetweenItemApparition, disappearClip);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
		yield return SharedYields.WaitForSeconds((float)ProductionItem.Items.Count * delayBetweenItemApparition);
		for (int j = 0; j < ProductionItem.Items.Count; j++)
		{
			AudioClip appearClip = ((j == 0) ? appearAudioClips[0] : appearAudioClips[(j + 1) % 2 + 1]);
			chooseRewardShelves[j].ItemIndex = j;
			chooseRewardShelves[j].Reload();
			chooseRewardShelves[j].Refresh();
			chooseRewardShelves[j].Appear((float)j * delayBetweenItemApparition, appearClip);
		}
		yield return SharedYields.WaitForSeconds((float)ProductionItem.Items.Count * delayBetweenItemApparition);
		SoundManager.PlayAudioClip(appearAudioClips[3]);
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(chooseRewardShelves[0].RewardItemSlotView.gameObject);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: true);
		}
		isItemChanging = false;
	}

	private void RefreshShelvesQuantity(int shelvesCount)
	{
		while (chooseRewardShelves.Count > shelvesCount)
		{
			UnityEngine.Object.Destroy(chooseRewardShelves[^1].gameObject);
			chooseRewardShelves.RemoveAt(chooseRewardShelves.Count - 1);
		}
		while (chooseRewardShelves.Count < shelvesCount)
		{
			chooseRewardShelves.Add(UnityEngine.Object.Instantiate(chooseRewardShelf, shelvesContainer));
		}
		HUDJoystickSimpleTarget hUDJoystickSimpleTarget = JoystickTarget as HUDJoystickSimpleTarget;
		if (hUDJoystickSimpleTarget != null)
		{
			hUDJoystickSimpleTarget.ClearSelectables();
			for (int i = 0; i < chooseRewardShelves.Count; i++)
			{
				hUDJoystickSimpleTarget.AddSelectable(chooseRewardShelves[i].RewardItemSlotView.Selectable);
				Navigation navigation = new Navigation
				{
					mode = Navigation.Mode.Explicit
				};
				if (i < chooseRewardShelves.Count - 1)
				{
					navigation.selectOnRight = chooseRewardShelves[i + 1].RewardItemSlotView.Selectable;
				}
				if (i > 0)
				{
					navigation.selectOnLeft = chooseRewardShelves[i - 1].RewardItemSlotView.Selectable;
				}
				navigation.selectOnDown = unitDropdown;
				chooseRewardShelves[i].RewardItemSlotView.Selectable.navigation = navigation;
			}
		}
		unitDropdown.navigation = AddRewardToNavigation(unitDropdown.navigation);
		previousUnitButton.navigation = AddRewardToNavigation(previousUnitButton.navigation);
		nextUnitButton.navigation = AddRewardToNavigation(nextUnitButton.navigation);
	}

	private void ToggleShelvesMask(bool toggle)
	{
		shelvesMask.enabled = toggle;
	}

	private void Update()
	{
		if (IsOpened)
		{
			if (firstFrameOpened)
			{
				firstFrameOpened = false;
			}
			else if (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(80))
			{
				Close();
			}
			else if (InputManager.GetButtonDown(79) && IsAnyRewardJoystickSelected())
			{
				OnConfirm();
			}
			else if (InputManager.GetButtonDown(102) && PanicManager.Panic.PanicReward.PanicRewardController.CanReroll())
			{
				OnRerollButtonClick();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		canvas = TPSingleton<ChooseRewardPanel>.Instance.GetComponent<Canvas>();
		canvas.enabled = false;
		canvasGroup = TPSingleton<ChooseRewardPanel>.Instance.GetComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		TPSingleton<ChooseRewardPanel>.Instance.appearAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Appear");
		TPSingleton<ChooseRewardPanel>.Instance.disappearAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Disappear");
		TPSingleton<ChooseRewardPanel>.Instance.selectAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Select");
		unitDropdownPanel.OnUnitToCompareChanged += ChangeUnitToCompare;
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}

	private void RefreshText()
	{
		if (ProductionItem != null)
		{
			productionBuildingText.text = ((ProductionItem.ProductionBuildingDefinition != null) ? ProductionItem.ProductionBuildingDefinition.Name : Localizer.Get("NightReportPanel_NightRewardObject"));
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
			if (TPSingleton<ChooseRewardPanel>.Instance.IsOpened)
			{
				return;
			}
			if (ProductionItem == null)
			{
				ProductionItem = new ProductionItemController().ProductionItem;
				List<string> list = new List<string>(ItemDatabase.ItemDefinitions.Keys);
				for (int i = 0; i < chooseRewardShelves.Count; i++)
				{
					TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(new ItemManager.ItemGenerationInfo
					{
						ItemDefinition = ItemDatabase.ItemDefinitions[list[RandomManager.GetRandomRange(this, 0, list.Count)]],
						Rarity = ItemDefinition.E_Rarity.Common
					});
					ProductionItem.Items.Add(item);
				}
			}
			TPSingleton<ChooseRewardPanel>.Instance.Open();
		}
	}

	[ContextMenu("Close")]
	public void DebugClose()
	{
		if (!UnityEngine.Application.isPlaying)
		{
			Debug.LogError("Unable to use this context menu when the application is not running");
		}
		else if (TPSingleton<ChooseRewardPanel>.Instance.IsOpened)
		{
			Close();
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Apocalypse;
using TheLastStand.Database;
using TheLastStand.Database.Meta;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Apocalypse;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Apocalypse;
using TheLastStand.Serialization.Item.ItemRestriction;
using TheLastStand.Serialization.Meta;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.Menus;
using TheLastStand.View.Tooltip;
using TheLastStand.View.WorldMap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.SaveSlots;

public class SaveSlotBox : MonoBehaviour
{
	public enum SlotState
	{
		Empty,
		Full,
		Corrupted
	}

	public static class Constants
	{
		public const string ConfirmEraseSaveLocalizationKey = "Settings_ConfirmEraseSaveText";

		public const string SaveSlotsTitle = "SaveSlots_Title";

		public const string SaveSlotsProductionCycleInfo = "SaveSlots_ProductionCycleInfo";

		public const string SaveSlotsDeploymentCycleInfo = "SaveSlots_DeploymentCycleInfo";

		public const string SaveSlotsNightCycleInfo = "SaveSlots_NightCycleInfo";

		public const string SaveSlotsBossCycleInfo = "SaveSlots_BossCycleInfo";

		public const string SaveSlotsLightShopTitle = "SaveSlots_LightShop_Title";

		public const string SaveSlotsDarkShopTitle = "SaveSlots_DarkShop_Title";

		public const string SaveSlotsMetaShopTitleUnknown = "SaveSlots_MetaShop_Title_Unknown";

		public const string SaveSlotsMetaShopValue = "SaveSlots_MetaShop_Value";

		public const string SaveSlotsSubtitleCorrupted = "SaveSlots_Subtitle_Corrupted";

		public const string SaveSlotsSubtitleObsolete = "SaveSlots_Subtitle_Obsolete";

		public const string SaveSlotsSubtitleObsoleteMissingDLC = "SaveSlots_Subtitle_Obsolete_MissingDLC";

		public const string SaveSlotsSubtitleObsoleteMissingMods = "SaveSlots_Subtitle_Obsolete_MissingMods";

		public const string SubtitleTimeFormat = "yyyy/MM/dd HH:mm";
	}

	[SerializeField]
	private TextMeshProUGUI slotIndexText;

	[SerializeField]
	private Canvas fullSlotCanvas;

	[SerializeField]
	private Canvas notCorruptedSlotCanvas;

	[SerializeField]
	private TextMeshProUGUI cityNameText;

	[SerializeField]
	private TextMeshProUGUI cityNormalSubtitleText;

	[SerializeField]
	private Image cityImage;

	[SerializeField]
	private Sprite worldMapSprite;

	[SerializeField]
	private Canvas difficultyCanvas;

	[SerializeField]
	private CityDifficultyView cityDifficultyView;

	[SerializeField]
	private GameObject cycleBoxGameObject;

	[SerializeField]
	private Canvas nightCycleCanvas;

	[SerializeField]
	private TextMeshProUGUI nightCycleText;

	[SerializeField]
	private Canvas productionCycleCanvas;

	[SerializeField]
	private TextMeshProUGUI productionCycleText;

	[SerializeField]
	private Canvas deploymentCycleCanvas;

	[SerializeField]
	private TextMeshProUGUI deploymentCycleText;

	[SerializeField]
	private Canvas bossCycleCanvas;

	[SerializeField]
	private TextMeshProUGUI bossCycleText;

	[SerializeField]
	private TextMeshProUGUI lightShopTitleText;

	[SerializeField]
	private TextMeshProUGUI lightShopValueText;

	[SerializeField]
	private TextMeshProUGUI darkShopTitleText;

	[SerializeField]
	private TextMeshProUGUI darkShopValueText;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsValueText;

	[SerializeField]
	private GameObject gameModifiersParent;

	[SerializeField]
	private GameObject apocalypseObject;

	[SerializeField]
	private Animator apocalypseFlameAnimator;

	[SerializeField]
	private TextMeshProUGUI apocalypseLevelText;

	[SerializeField]
	private JoystickSelectable apocalypseJoystickSelectable;

	[SerializeField]
	private GameObject glyphsObject;

	[SerializeField]
	private TextMeshProUGUI glyphsCustomModeText;

	[SerializeField]
	private Image glyphsImage;

	[SerializeField]
	private Sprite glyphsNormalModeSprite;

	[SerializeField]
	private Sprite glyphsCustomModeSprite;

	[SerializeField]
	private JoystickSelectable glyphsJoystickSelectable;

	[SerializeField]
	private GameObject weaponsRestrictionsObject;

	[SerializeField]
	private Image weaponsRestrictionsImage;

	[SerializeField]
	private Sprite weaponsRestrictionsNormalModeSprite;

	[SerializeField]
	private Sprite weaponsRestrictionsCustomModeSprite;

	[SerializeField]
	private JoystickSelectable weaponsRestrictionsJoystickSelectable;

	[SerializeField]
	private GlyphsTooltipDisplayer glyphsTooltipDisplayer;

	[SerializeField]
	private ApocalypseEffectsTooltipDisplayer apocalypseEffectsTooltipDisplayer;

	[SerializeField]
	private WeaponsRestrictionsTooltipDisplayer restrictionsTooltipDisplayer;

	[SerializeField]
	private Canvas corruptedSlotCanvas;

	[SerializeField]
	private TextMeshProUGUI cityCorruptedTitleText;

	[SerializeField]
	private TextMeshProUGUI cityCorruptedSubtitleText;

	[SerializeField]
	private TextMeshProUGUI missingDLCText;

	[SerializeField]
	private Canvas emptySlotCanvas;

	[SerializeField]
	private GameObject newGameButtonGameObject;

	[SerializeField]
	private GameObject deleteButtonGameObject;

	private Dictionary<string, MetaUpgradeDefinition> ownedMetaUpgrades;

	private int profileIndex;

	private bool showGlyphs;

	private bool showApocalypse;

	private bool showRestrictions;

	private StringBuilder missingDlcStringBuilder = new StringBuilder();

	public void ChangeLinkedProfileIndex(int newProfileIndex)
	{
		profileIndex = newProfileIndex;
	}

	private void InitOwnedMetaUpgrades()
	{
		ownedMetaUpgrades = new Dictionary<string, MetaUpgradeDefinition>();
		foreach (KeyValuePair<string, MetaUpgradeDefinition> metaUpgradesDefinition in MetaDatabase.MetaUpgradesDefinitions)
		{
			if (!metaUpgradesDefinition.Value.IsLinkedToDLC || TPSingleton<DLCManager>.Instance.IsDLCOwned(metaUpgradesDefinition.Value.DLCId))
			{
				ownedMetaUpgrades.Add(metaUpgradesDefinition.Key, metaUpgradesDefinition.Value);
			}
		}
	}

	private static string GetLoadFailedSubtitle((SaveManager.E_BrokenSaveReason? Reason, Exception Exception)? failedLoadsInfo)
	{
		return failedLoadsInfo?.Reason switch
		{
			SaveManager.E_BrokenSaveReason.WRONG_VERSION => Localizer.Get("MainMenu_ContinueGameInfo_Outdated"), 
			SaveManager.E_BrokenSaveReason.MISSING_MOD => Localizer.Get("SaveSlots_Subtitle_Obsolete_MissingMods"), 
			SaveManager.E_BrokenSaveReason.MISSING_DLC => Localizer.Get("SaveSlots_Subtitle_Obsolete_MissingDLC"), 
			_ => Localizer.Get("MainMenu_ContinueGameInfo_Corrupted"), 
		};
	}

	public void Refresh(SerializedApplicationState appState, SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> gameLoadingInfo)
	{
		slotIndexText.text = Localizer.Format("SaveSlots_Title", SaveManager.GetProfilePathIndex(profileIndex));
		bool flag = SaveManager.DoesGameSaveExist(profileIndex);
		SlotState slotState = SlotState.Empty;
		if (flag && gameLoadingInfo?.LoadedContainer == null)
		{
			slotState = SlotState.Corrupted;
		}
		else if (appState != null && (flag || appState.RunsCompleted != 0))
		{
			slotState = SlotState.Full;
		}
		fullSlotCanvas.enabled = slotState != SlotState.Empty;
		notCorruptedSlotCanvas.enabled = slotState == SlotState.Full;
		corruptedSlotCanvas.enabled = slotState == SlotState.Corrupted;
		emptySlotCanvas.enabled = slotState == SlotState.Empty;
		switch (slotState)
		{
		case SlotState.Full:
		{
			cityImage.sprite = ((flag && appState.Cities.SelectedCityId != null) ? ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/Cities/Portraits/WorldMap_CityPortrait_" + appState.Cities.SelectedCityId + "01") : worldMapSprite);
			difficultyCanvas.enabled = flag;
			cycleBoxGameObject.SetActive(flag);
			gameModifiersParent.SetActive(flag);
			if (flag)
			{
				SerializedGameState serializedGameState = gameLoadingInfo?.LoadedContainer;
				string serializedCityId = appState.Cities.SelectedCityId;
				if (serializedCityId != null)
				{
					CityDefinition cityDefinition = TPSingleton<WorldMapCityManager>.Instance.Cities.Find((WorldMapCity city) => city.CityDefinition.Id == serializedCityId).CityDefinition;
					SerializedCity serializedCity = appState.Cities.Cities.FirstOrDefault((SerializedCity serializedCity2) => serializedCity2.Id == serializedCityId);
					if (serializedCity != null)
					{
						RefreshCityName(cityDefinition, serializedCity);
						cityDifficultyView.RefreshDifficultySkulls(cityDefinition);
						if (serializedGameState != null)
						{
							RefreshCycle(serializedGameState, cityDefinition);
							RefreshGlyphs(serializedCity, cityDefinition);
							RefreshApocalypse(serializedGameState, appState);
							RefreshItemRestrictions(appState);
							RefreshModifierJoystickSelectable();
						}
					}
				}
			}
			else
			{
				cityNameText.text = MainMenuView.GetContinueCampaignText();
			}
			RefreshMetaUpgrades(appState);
			damnedSoulsValueText.text = $"{appState.DamnedSouls}";
			DateTime lastWriteTime = File.GetLastWriteTime(SaveManager.GetAppSaveFilePath(profileIndex));
			cityNormalSubtitleText.text = lastWriteTime.ToString("yyyy/MM/dd HH:mm");
			break;
		}
		case SlotState.Corrupted:
		{
			(SaveManager.E_BrokenSaveReason?, Exception)? tuple = ((gameLoadingInfo != null) ? new(SaveManager.E_BrokenSaveReason?, Exception)?(gameLoadingInfo.FailedLoadsInfo[0]) : (((SaveManager.E_BrokenSaveReason?, Exception)?)null));
			bool flag2 = tuple.HasValue && (tuple.Value.Item1 == SaveManager.E_BrokenSaveReason.MISSING_MOD || tuple.Value.Item1 == SaveManager.E_BrokenSaveReason.MISSING_DLC);
			cityCorruptedTitleText.text = Localizer.Get(flag2 ? "SaveSlots_Subtitle_Obsolete" : "SaveSlots_Subtitle_Corrupted");
			cityCorruptedSubtitleText.text = GetLoadFailedSubtitle((gameLoadingInfo != null) ? new(SaveManager.E_BrokenSaveReason?, Exception)?(gameLoadingInfo.FailedLoadsInfo[0]) : (((SaveManager.E_BrokenSaveReason?, Exception)?)null));
			bool flag3 = tuple.HasValue && tuple.Value.Item1 == SaveManager.E_BrokenSaveReason.MISSING_DLC;
			missingDLCText.enabled = flag3;
			missingDlcStringBuilder.Clear();
			if (!flag3 || !(tuple.Value.Item2 is SaverLoader.MissingDLCException { MissingDlcIds: not null } ex))
			{
				break;
			}
			foreach (string missingDlcId in ex.MissingDlcIds)
			{
				string textIconForPortraits = TPSingleton<DLCManager>.Instance.GetDLCFromId(missingDlcId).TextIconForPortraits;
				missingDlcStringBuilder.Append(textIconForPortraits ?? "");
			}
			missingDLCText.text = missingDlcStringBuilder.ToString();
			break;
		}
		}
	}

	private void RefreshMetaUpgrades(SerializedApplicationState appState)
	{
		bool flag = appState.MetaNarrations != null && MetaNarrationsManager.LightNarration.MetaNarrationController.CanDisplayGoddessName(appState.MetaNarrations.LightNarration.AlreadyUsedReplicasIds);
		lightShopTitleText.text = Localizer.Get(flag ? "SaveSlots_LightShop_Title" : "SaveSlots_MetaShop_Title_Unknown");
		flag = appState.MetaNarrations != null && MetaNarrationsManager.DarkNarration.MetaNarrationController.CanDisplayGoddessName(appState.MetaNarrations.DarkNarration.AlreadyUsedReplicasIds);
		darkShopTitleText.text = Localizer.Get(flag ? "SaveSlots_DarkShop_Title" : "SaveSlots_MetaShop_Title_Unknown");
		if (ownedMetaUpgrades == null)
		{
			InitOwnedMetaUpgrades();
		}
		int num = 0;
		int num2 = 0;
		if (appState.MetaUpgrades != null)
		{
			num = appState.MetaUpgrades.ActivatedUpgrades.FindAll((SerializedMetaUpgrade upgrade) => ownedMetaUpgrades.ContainsKey(upgrade.Id) && ownedMetaUpgrades[upgrade.Id].DamnedSoulsShop).Count;
			num2 = appState.MetaUpgrades.ActivatedUpgrades.FindAll((SerializedMetaUpgrade upgrade) => ownedMetaUpgrades.ContainsKey(upgrade.Id) && !ownedMetaUpgrades[upgrade.Id].DamnedSoulsShop).Count;
		}
		int num3 = ownedMetaUpgrades.Count((KeyValuePair<string, MetaUpgradeDefinition> upgrade) => upgrade.Value.DamnedSoulsShop);
		int num4 = ownedMetaUpgrades.Count - num3;
		int metaShopProgressionPercentage = AMetaShopManager<LightShopManager>.GetMetaShopProgressionPercentage(num2, num4);
		string text = $"{num2}/{num4} ({metaShopProgressionPercentage}%)";
		lightShopValueText.text = Localizer.Format("SaveSlots_MetaShop_Value", text);
		int metaShopProgressionPercentage2 = AMetaShopManager<DarkShopManager>.GetMetaShopProgressionPercentage(num, num3);
		text = $"{num}/{num3} ({metaShopProgressionPercentage2}%)";
		darkShopValueText.text = Localizer.Format("SaveSlots_MetaShop_Value", text);
	}

	private void RefreshItemRestrictions(SerializedApplicationState appState)
	{
		Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>> dictionary = new Dictionary<ItemDefinition.E_Category, List<ItemRestrictionFamilyDefinition>>();
		Dictionary<ItemDefinition.E_Category, int> dictionary2 = new Dictionary<ItemDefinition.E_Category, int>();
		showRestrictions = false;
		if (appState.ItemRestrictions != null)
		{
			List<string> lockedItemsIdsFromSave = MetaUpgradesManager.GetLockedItemsIdsFromSave(appState.MetaUpgrades);
			foreach (SerializedItemRestrictionFamily itemFamily in appState.ItemRestrictions.ItemFamilies)
			{
				if (itemFamily.IsSelected && ItemDatabase.ItemsListDefinitions.TryGetValue(itemFamily.Id, out var value) && !ItemManager.IsItemsListContentLocked(value, lockedItemsIdsFromSave.ToArray()))
				{
					ItemRestrictionFamilyDefinition itemRestrictionFamilyDefinition = ItemDatabase.ItemRestrictionFamiliesDefinitions[itemFamily.Id];
					ItemDefinition.E_Category itemCategory = itemRestrictionFamilyDefinition.ItemCategory;
					if (!dictionary.ContainsKey(itemCategory))
					{
						dictionary.Add(itemCategory, new List<ItemRestrictionFamilyDefinition>());
					}
					dictionary[itemCategory].Add(itemRestrictionFamilyDefinition);
					if (!dictionary2.ContainsKey(itemCategory))
					{
						dictionary2.Add(itemCategory, 0);
					}
					dictionary2[itemCategory]++;
				}
			}
			if (dictionary2.Count > 0)
			{
				foreach (KeyValuePair<ItemDefinition.E_Category, int> item in dictionary2)
				{
					if (IsCategoryRestrictionAvailable(item.Key, item.Value))
					{
						showRestrictions = true;
						break;
					}
				}
			}
		}
		weaponsRestrictionsObject.SetActive(showRestrictions);
		weaponsRestrictionsImage.sprite = ((showRestrictions && appState.ItemRestrictions.WeaponsCategoriesCollection.IsBoundlessActive) ? weaponsRestrictionsCustomModeSprite : weaponsRestrictionsNormalModeSprite);
		restrictionsTooltipDisplayer.SetRestrictionFamilyDefinitions(dictionary);
		restrictionsTooltipDisplayer.SetIsBoundless(showRestrictions && appState.ItemRestrictions.WeaponsCategoriesCollection.IsBoundlessActive);
	}

	private bool IsCategoryRestrictionAvailable(ItemDefinition.E_Category category, int unlockedInCategoryNb)
	{
		foreach (ItemRestrictionCategoriesCollectionDefinition value in ItemDatabase.ItemRestrictionCategoriesCollectionDefinitions.Values)
		{
			foreach (ItemRestrictionCategoryDefinition itemCategoryDefinition in value.itemCategoryDefinitions)
			{
				if (itemCategoryDefinition.ItemCategory == category)
				{
					return unlockedInCategoryNb >= itemCategoryDefinition.MinimumSelectedNb;
				}
			}
		}
		return false;
	}

	private void RefreshApocalypse(SerializedGameState gameState, SerializedApplicationState appState)
	{
		showApocalypse = gameState.Apocalypse != null && gameState.Apocalypse.ApocalypseIndex > 0;
		int num = gameState.Apocalypse.ApocalypseIndex;
		List<ApocalypseModifierStepDefinition> list = null;
		apocalypseObject.SetActive(showApocalypse);
		if (showApocalypse)
		{
			bool flag = false;
			if (gameState.SaveVersion <= 23)
			{
				ApocalypseRetroCompatibilityLevelEquivalence apocalypseLevelEquivalence = ApocalypseRetroCompatibilityController.GetApocalypseLevelEquivalence(gameState.Apocalypse.ApocalypseIndex);
				if (apocalypseLevelEquivalence != null)
				{
					num = apocalypseLevelEquivalence.CorrespondingLevel;
					list = ApocalypseRetroCompatibilityController.GetApocalypseModifierStepDefinitionsFromLevelEquivalence(apocalypseLevelEquivalence);
					flag = true;
				}
			}
			apocalypseLevelText.text = $"<style=Bad>{num}</style>";
			apocalypseFlameAnimator.Play("WorldMapFlamesIdle");
			if (appState.GlobalApocalypse?.SelectedModifierSteps != null && !flag)
			{
				list = new List<ApocalypseModifierStepDefinition>();
				foreach (SerializedApocalypseModifierStep selectedModifierStep in appState.GlobalApocalypse.SelectedModifierSteps)
				{
					if (ApocalypseDatabase.ModifierDefinitions.TryGetValue(selectedModifierStep.ModifierId, out var value) && value.StepDefinitions.Count > selectedModifierStep.StepIndex)
					{
						list.Add(value.StepDefinitions[selectedModifierStep.StepIndex]);
					}
				}
			}
		}
		apocalypseEffectsTooltipDisplayer.SetIsApocalypseSystemUnlocked(showApocalypse);
		apocalypseEffectsTooltipDisplayer.SetApocalypseModifierStepDefinitions(list);
		apocalypseEffectsTooltipDisplayer.SetDamnedSoulsPercentageModifier(ApocalypseManager.GetDamnedSoulsPercentageModifier(num));
	}

	private void RefreshGlyphs(SerializedCity selectedCity, CityDefinition selectedCityDefinition)
	{
		showGlyphs = selectedCity.SelectedGlyphs != null && selectedCity.SelectedGlyphs.Count > 0;
		glyphsObject.SetActive(showGlyphs);
		List<GlyphDefinition> list = null;
		int num = 0;
		int num2 = 0;
		bool flag = false;
		if (showGlyphs)
		{
			list = new List<GlyphDefinition>();
			for (int i = 0; i < selectedCity.SelectedGlyphs.Count; i++)
			{
				if (GlyphDatabase.GlyphDefinitions.TryGetValue(selectedCity.SelectedGlyphs[i], out var value))
				{
					list.Add(value);
					num += value.Cost;
				}
			}
			num2 = Mathf.Max(0, num - selectedCityDefinition.MaxGlyphPoints);
			flag = selectedCity.CustomModeEnabled && num2 > 0;
			glyphsCustomModeText.enabled = flag;
			glyphsImage.sprite = (flag ? glyphsCustomModeSprite : glyphsNormalModeSprite);
			if (flag)
			{
				glyphsCustomModeText.text = $"+{num2}";
			}
		}
		glyphsTooltipDisplayer.SetSelectedGlyphs(list);
		glyphsTooltipDisplayer.SetCustomModeEnabled(flag);
		glyphsTooltipDisplayer.SetCustomModePoints(num2);
	}

	private void RefreshModifierJoystickSelectable()
	{
		Selectable selectable = ((profileIndex < 4) ? TPSingleton<SaveSlotsPanel>.Instance.SaveSlotJoystickSelectables[profileIndex + 1] : null);
		Selectable selectable2 = ((profileIndex > 0) ? TPSingleton<SaveSlotsPanel>.Instance.SaveSlotJoystickSelectables[profileIndex - 1] : null);
		Selectable selectOnRight = (showApocalypse ? apocalypseJoystickSelectable : weaponsRestrictionsJoystickSelectable);
		if (!showApocalypse && !showRestrictions)
		{
			selectOnRight = selectable;
		}
		glyphsJoystickSelectable.SetSelectOnRight(selectOnRight);
		Selectable selectOnLeft = ((!showGlyphs && showApocalypse) ? selectable2 : glyphsJoystickSelectable);
		apocalypseJoystickSelectable.SetSelectOnLeft(selectOnLeft);
		Selectable selectOnRight2 = ((!showRestrictions && showApocalypse) ? selectable : weaponsRestrictionsJoystickSelectable);
		apocalypseJoystickSelectable.SetSelectOnRight(selectOnRight2);
		Selectable selectOnLeft2 = (showApocalypse ? apocalypseJoystickSelectable : glyphsJoystickSelectable);
		if (!showApocalypse && !showGlyphs)
		{
			selectOnLeft2 = selectable2;
		}
		weaponsRestrictionsJoystickSelectable.SetSelectOnLeft(selectOnLeft2);
	}

	private void RefreshCycle(SerializedGameState gameState, CityDefinition selectedCityDefinition)
	{
		bool num = gameState.Game.Cycle == Game.E_Cycle.Day;
		bool flag = !num && gameState.BossData?.BossPhase != null && gameState.Game.DayNumber >= selectedCityDefinition.VictoryDaysCount;
		bool flag2 = !num && !flag;
		bool flag3 = num && gameState.Game.DayTurn == Game.E_DayTurn.Production;
		bool flag4 = num && gameState.Game.DayTurn == Game.E_DayTurn.Deployment;
		nightCycleCanvas.enabled = flag2;
		if (flag2)
		{
			nightCycleText.text = Localizer.Format("SaveSlots_NightCycleInfo", gameState.Game.DayNumber, gameState.Game.NightHour);
		}
		productionCycleCanvas.enabled = flag3;
		if (flag3)
		{
			productionCycleText.text = Localizer.Format("SaveSlots_ProductionCycleInfo", gameState.Game.DayNumber);
		}
		deploymentCycleCanvas.enabled = flag4;
		if (flag4)
		{
			deploymentCycleText.text = Localizer.Format("SaveSlots_DeploymentCycleInfo", gameState.Game.DayNumber);
		}
		bossCycleCanvas.enabled = flag;
		if (flag)
		{
			bossCycleText.text = Localizer.Format("SaveSlots_BossCycleInfo", gameState.Game.NightHour);
		}
	}

	private void RefreshCityName(CityDefinition selectedCityDefinition, SerializedCity selectedCity)
	{
		string text = selectedCityDefinition.Name;
		string text2 = (selectedCity.NumberOfRuns + 1).ToString();
		string text3 = text + ((selectedCity.NumberOfRuns > 0) ? (" #" + text2) : string.Empty);
		cityNameText.text = text3;
	}

	public void OnLoadClick()
	{
		ApocalypseManager.SetApocalypse(null);
		GlyphManager.ResetSelectedGlyphs();
		TPSingleton<SaveSlotsPanel>.Instance.LastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		SaveManager.ChangeCurrentProfile(profileIndex);
		TPSingleton<MainMenuView>.Instance.TryContinueGame(TPSingleton<SaveSlotsPanel>.Instance.GraphicRaycaster);
	}

	public void OnDeleteClick()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
		OpenConsentPopup();
	}

	public void OnJoystickSelect(RectTransform source)
	{
		TPSingleton<SaveSlotsPanel>.Instance.OnSaveSlotJoystickSelect(source);
	}

	private void OnCancel()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(deleteButtonGameObject);
		}
	}

	private void OnConfirm()
	{
		SaveManager.EraseSave(profileIndex);
		SaveManager.ChangeCurrentProfile(profileIndex);
		TPSingleton<SaveSlotsPanel>.Instance.Refresh();
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(newGameButtonGameObject);
		}
	}

	private void OpenConsentPopup()
	{
		GenericConsent.Open("Settings_ConfirmEraseSaveText", OnConfirm, OnCancel);
	}
}

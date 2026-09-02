using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Controller.Skill;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Generic;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit.Perk;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public class ItemTooltip : TooltipBase
{
	public delegate void OnItemTooltipDisplayedChange(bool isDisplayed);

	public class Constants
	{
		public const float PanelMinWidth = 275f;

		public const float PanelWidthOffest = -20f;

		public const float PerkPanelTileAndBackgroundOffset = 50f;
	}

	[SerializeField]
	private HorizontalLayoutGroup horizontalLayoutGroup;

	[SerializeField]
	private SkillTooltip skillTooltip;

	[SerializeField]
	private RectTransform itemTooltipPanel;

	[SerializeField]
	private RectTransform spaceRectTransform;

	[SerializeField]
	private float minSpaceSize = 20f;

	[SerializeField]
	private float maxSpaceSize = 30f;

	[SerializeField]
	private RectTransform glowRectTransform;

	[SerializeField]
	private Image glowImage;

	[SerializeField]
	private DataSpriteTable glowSprites;

	[SerializeField]
	private float glowDeltaHeight = 25f;

	[SerializeField]
	private Image titleBGImage;

	[SerializeField]
	private DataSpriteTable titleBGSprites;

	[SerializeField]
	private Image itemIconImage;

	[SerializeField]
	private Image itemIconBGImage;

	[SerializeField]
	private TextMeshProUGUI itemNameText;

	[SerializeField]
	private TextMeshProUGUI subtitleText;

	[SerializeField]
	private Image categoryImage;

	[SerializeField]
	private Image handsImage;

	[SerializeField]
	private TextMeshProUGUI mainStatNameText;

	[SerializeField]
	private TextMeshProUGUI mainStatValueText;

	[SerializeField]
	private Image rarityIconImage;

	[SerializeField]
	private TextMeshProUGUI equippedText;

	[SerializeField]
	private TextMeshProUGUI sellPriceText;

	[SerializeField]
	private DataSpriteTable rarityIcons;

	[SerializeField]
	private DataColorTable rarityColors;

	[SerializeField]
	private Image itemLevelImage;

	[SerializeField]
	private DataSpriteTable itemLevelSprites;

	[SerializeField]
	private RectTransform allStatsPanel;

	[SerializeField]
	private VerticalLayoutGroup allStatsLayout;

	[SerializeField]
	private Image allStatsBG;

	[SerializeField]
	private DataSpriteTable itemTooltipBGSprites;

	[SerializeField]
	private GameObject baseAffixesPanelGameObject;

	[SerializeField]
	private RectTransform baseAffixesBG;

	[SerializeField]
	private RectTransform baseAffixesBotBG;

	[SerializeField]
	private GameObject additionalAffixesPanelGameObject;

	[SerializeField]
	private Image additionalAffixesBGImage;

	[SerializeField]
	private DataSpriteTable additionalAffixesRartityBGSprites;

	[SerializeField]
	private AffixStatView affixTextPrefab;

	[SerializeField]
	private ItemCarouselEntryListDisplay carouselEntriesListDisplay;

	[SerializeField]
	private ItemCarouselEntryIconDisplay selectedCarouselEntryDisplayPrefab;

	[SerializeField]
	private GameObject carouselPanel;

	[SerializeField]
	private Canvas carouselPanelCanvas;

	[SerializeField]
	private GameObject spaceGameObject;

	[SerializeField]
	private SkillDisplay skillDisplay;

	[SerializeField]
	private GameObject skillDisplayPanel;

	[SerializeField]
	private RectTransform skillDisplayPanelRect;

	[SerializeField]
	private RectTransform skillDetailsRect;

	[SerializeField]
	private RectTransform skillEffectsRect;

	[SerializeField]
	private UnitPerkDisplay unitPerkDisplay;

	[SerializeField]
	private GameObject perkDisplayPanel;

	[SerializeField]
	private RectTransform perkDisplayPanelRect;

	[SerializeField]
	private RectTransform perkDisplayPanelContentRectTransform;

	[SerializeField]
	private RectTransform[] perkDisplayPanelRectTransforms;

	[SerializeField]
	private GameObject perkSkillTooltipContainer;

	[SerializeField]
	private SkillWithoutCompendiumTooltip perkSkillTooltip;

	[SerializeField]
	private bool displayPerkSkillTooltipToTheRight = true;

	[SerializeField]
	private GameObject perkSkillCycleHelper;

	[SerializeField]
	private GameObject epicParticles;

	[SerializeField]
	private GameObject rareParticles;

	private int skillsNb;

	private int perksNb;

	private int carouselPanelCanvasDefaultSortingOrder;

	private VerticalLayoutGroup additionalAffixeVerticalLayoutGroup;

	private float baseAffixesBGBotOffsetInit;

	private VerticalLayoutGroup baseAffixeVerticalLayoutGroup;

	private PlayableUnit itemOwner;

	private int carouselEntryIndex;

	private ItemCarouselEntryIconDisplay selectedCarouselEntryDisplay;

	private bool useDefaultValues;

	private RectTransform AdditionalAffixesPanelRectTransform => additionalAffixesPanelGameObject.transform as RectTransform;

	private VerticalLayoutGroup AdditionalAffixeVerticalLayoutGroup
	{
		get
		{
			if (additionalAffixeVerticalLayoutGroup == null)
			{
				additionalAffixeVerticalLayoutGroup = additionalAffixesPanelGameObject.GetComponent<VerticalLayoutGroup>();
			}
			return additionalAffixeVerticalLayoutGroup;
		}
	}

	private RectTransform AffixTextPrefabRectTransform => affixTextPrefab.transform as RectTransform;

	private RectTransform BaseAffixesPanelRectTransform => baseAffixesPanelGameObject.transform as RectTransform;

	private VerticalLayoutGroup BaseAffixeVerticalLayoutGroup
	{
		get
		{
			if (baseAffixeVerticalLayoutGroup == null)
			{
				baseAffixeVerticalLayoutGroup = baseAffixesPanelGameObject.GetComponent<VerticalLayoutGroup>();
			}
			return baseAffixeVerticalLayoutGroup;
		}
	}

	public Perk CurrentPerk { get; private set; }

	public TheLastStand.Model.Skill.Skill CurrentPerkSkill { get; private set; }

	public TheLastStand.Model.Item.Item Item { get; set; }

	public ItemTooltipSkillCycle ItemTooltipSkillCycle { get; set; }

	protected Dictionary<ItemDefinition.E_Rarity, GameObject> RarityParticles { get; private set; }

	public event OnItemTooltipDisplayedChange ItemTooltipDisplayedChangeEvent;

	public void RefreshCarousel()
	{
		if (Item == null)
		{
			return;
		}
		skillsNb = 0;
		perksNb = 0;
		if ((Item.Skills != null && Item.Skills.Count > 0) || (Item.Perks != null && Item.Perks.Count > 0))
		{
			StartCoroutine(RefreshCarouselPanelCanvasSortingOrder());
			carouselPanel.SetActive(value: true);
			spaceGameObject.SetActive(value: true);
			carouselEntriesListDisplay.SetContent(Item.Skills, Item.Perks.Values.ToList());
			if (Item.Skills != null)
			{
				skillsNb = Item.Skills.Count;
			}
			if (Item.Perks != null)
			{
				perksNb = Item.Perks.Count;
			}
			int num = skillsNb + perksNb;
			if (selectedCarouselEntryDisplay == null)
			{
				selectedCarouselEntryDisplay = UnityEngine.Object.Instantiate(selectedCarouselEntryDisplayPrefab, carouselEntriesListDisplay.transform);
			}
			if (carouselEntryIndex < num)
			{
				carouselEntriesListDisplay.DisplayElement(carouselEntryIndex, show: true);
			}
			carouselEntryIndex = Mathf.Clamp(ItemTooltipSkillCycle.SkillTabIndex, 0, num - 1);
			selectedCarouselEntryDisplay.transform.SetSiblingIndex(carouselEntryIndex);
			if (carouselEntryIndex < skillsNb)
			{
				selectedCarouselEntryDisplay.SetContent(Item.Skills[carouselEntryIndex].SkillDefinition);
			}
			else
			{
				selectedCarouselEntryDisplay.SetContent(Item.Perks.Values.ToList()[carouselEntryIndex - skillsNb].PerkDefinition);
			}
			bool isNextEntryASkill = skillsNb > 0 && (carouselEntryIndex + 1 < skillsNb || carouselEntryIndex + 1 >= num);
			selectedCarouselEntryDisplay.Refresh();
			selectedCarouselEntryDisplay.ToggleNextElementLabel(num > 1, isNextEntryASkill);
			float y = ((num > 1) ? maxSpaceSize : minSpaceSize);
			spaceRectTransform.sizeDelta = new Vector2(spaceRectTransform.sizeDelta.x, y);
			carouselEntriesListDisplay.DisplayElement(carouselEntryIndex, show: false);
		}
		else
		{
			carouselPanel.SetActive(value: false);
			spaceGameObject.SetActive(value: false);
		}
	}

	public void RefreshSkill()
	{
		if (Item == null)
		{
			skillDisplay.Skill = null;
			return;
		}
		TheLastStand.Model.Skill.Skill skill = skillDisplay.Skill;
		if (Item.Skills != null && Item.Skills.Count > 0 && carouselEntryIndex < Item.Skills.Count)
		{
			skillDisplayPanel.SetActive(value: true);
			skillDisplay.SkillOwner = itemOwner;
			skillDisplay.Skill = Item.Skills[carouselEntryIndex];
			skillDisplay.Init(skillTooltip);
			skillDisplay.Refresh();
			float num = 0f;
			skillEffectsRect.localPosition = new Vector2(skillEffectsRect.localPosition.x, skillDetailsRect.localPosition.y - skillDetailsRect.sizeDelta.y);
			num += 0f - skillDetailsRect.localPosition.y + skillDetailsRect.sizeDelta.y + skillEffectsRect.sizeDelta.y;
			skillDisplayPanelRect.sizeDelta = new Vector2(skillDisplayPanelRect.sizeDelta.x, num);
		}
		else
		{
			skillDisplay.Skill = null;
			skillDisplayPanel.SetActive(value: false);
		}
		if (skillDisplay.Skill != skill)
		{
			ItemTooltipSkillCycle.UpdateCompendium();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(skillDisplay.SkillAreaOfEffectGrid.RectTransform);
	}

	public void RefreshPerk()
	{
		if (Item == null)
		{
			RemovePerkLink();
		}
		else if (perksNb > 0 && carouselEntryIndex >= skillsNb)
		{
			RefreshTooltipWidth();
			RemovePerkLink();
			int index = carouselEntryIndex - skillsNb;
			perkDisplayPanel.SetActive(value: true);
			CurrentPerkSkill = null;
			CurrentPerk = Item.Perks.Values.ToList()[index];
			if (CurrentPerk != null)
			{
				PerkDefinition perkDefinition = CurrentPerk.PerkDefinition;
				if (itemOwner != null)
				{
					if (itemOwner.Perks.ContainsKey(perkDefinition.Id))
					{
						unitPerkDisplay.SetContent(itemOwner.Perks[perkDefinition.Id]);
					}
					else
					{
						CurrentPerk.PerkController.ChangeOwner(itemOwner);
						unitPerkDisplay.SetContent(CurrentPerk);
					}
				}
				else
				{
					unitPerkDisplay.SetContent(null, perkDefinition);
				}
				perkSkillCycleHelper.SetActive(unitPerkDisplay.PerkDefinition != null && unitPerkDisplay.PerkDefinition.SkillsToShow.Count > 1);
				int num = Mathf.Min(ItemTooltipSkillCycle.PerkSkillTabIndex, (unitPerkDisplay.PerkDefinition != null) ? (unitPerkDisplay.PerkDefinition.SkillsToShow.Count - 1) : 0);
				if (CurrentPerk.PerkDefinition.SkillsToShow.Count > 0 && num < CurrentPerk.PerkDefinition.SkillsToShow.Count)
				{
					string item = unitPerkDisplay.PerkDefinition.SkillsToShow[num].Item1;
					if (SkillDatabase.SkillDefinitions.TryGetValue(item, out var value))
					{
						CurrentPerkSkill = new SkillController(value, unitPerkDisplay.Perk, unitPerkDisplay.PerkDefinition.SkillsToShow[num].Item2).Skill;
						perkSkillTooltip.SetContent(CurrentPerkSkill, TileObjectSelectionManager.SelectedPlayableUnit);
						perkSkillTooltip.DisplayInvalidityPanel = false;
						SetPerkSkillTooltipVisible(isVisible: true);
					}
				}
				else
				{
					SetPerkSkillTooltipVisible(isVisible: false);
				}
			}
			unitPerkDisplay.Init();
			ItemTooltipSkillCycle.UpdateCompendium();
			LayoutRebuilder.ForceRebuildLayoutImmediate(perkDisplayPanelContentRectTransform);
			float num2 = 0f;
			RectTransform[] array = perkDisplayPanelRectTransforms;
			foreach (RectTransform rectTransform in array)
			{
				if (rectTransform.gameObject.activeSelf)
				{
					num2 += rectTransform.sizeDelta.y;
				}
			}
			num2 += 50f;
			perkDisplayPanelRect.sizeDelta = new Vector2(skillDisplayPanelRect.sizeDelta.x, num2);
		}
		else
		{
			CurrentPerkSkill = null;
			RemovePerkLink();
			perkSkillCycleHelper.SetActive(value: false);
			SetPerkSkillTooltipVisible(isVisible: false);
			unitPerkDisplay.SetContent(null);
			perkDisplayPanel.SetActive(value: false);
		}
	}

	public void SetContent(TheLastStand.Model.Item.Item item, PlayableUnit itemOwner = null, bool newUseDefaultValues = false)
	{
		Item = item;
		this.itemOwner = itemOwner;
		useDefaultValues = newUseDefaultValues;
		ItemTooltipSkillCycle.Reset();
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		ItemTooltipDisplayedChangeEvent += InputManager.OnItemTooltipDisplayedChange;
		baseAffixesBGBotOffsetInit = baseAffixesBG.offsetMin.y;
		RarityParticles = new Dictionary<ItemDefinition.E_Rarity, GameObject>(default(ItemDefinition.RarityComparer))
		{
			{
				ItemDefinition.E_Rarity.Epic,
				epicParticles
			},
			{
				ItemDefinition.E_Rarity.Rare,
				rareParticles
			}
		};
		if (displayPerkSkillTooltipToTheRight)
		{
			perkSkillTooltipContainer.transform.SetAsLastSibling();
		}
		else
		{
			perkSkillTooltipContainer.transform.SetAsFirstSibling();
		}
		carouselPanelCanvasDefaultSortingOrder = carouselPanelCanvas.sortingOrder;
	}

	protected override bool CanBeDisplayed()
	{
		return Item != null;
	}

	protected override void RefreshContent()
	{
		if (Item == null)
		{
			return;
		}
		allStatsBG.sprite = itemTooltipBGSprites.GetSpriteAt((int)(Item.Rarity - 1));
		titleBGImage.sprite = titleBGSprites.GetSpriteAt((int)(Item.Rarity - 1));
		itemIconImage.sprite = ItemView.GetUiSprite(Item.ItemDefinition.ArtId);
		itemIconBGImage.sprite = ItemView.GetUiSprite(Item.ItemDefinition.ArtId, isBG: true);
		itemIconBGImage.color = rarityColors.GetColorAt((int)(Item.Rarity - 1));
		subtitleText.color = rarityColors.GetColorAt((int)(Item.Rarity - 1));
		itemLevelImage.sprite = itemLevelSprites.GetSpriteAt(Item.Level);
		itemLevelImage.enabled = itemLevelImage.sprite != null;
		RefreshText();
		categoryImage.sprite = ResourcePooler.LoadOnce<Sprite>(string.Format("{0}{1}_On", "View/Sprites/UI/Items/Categories/Icon_ItemCategory_", ItemDefinition.E_Category.Usable.HasFlag(Item.ItemDefinition.Category) ? ItemDefinition.E_Category.Usable : Item.ItemDefinition.Category));
		handsImage.sprite = ((Item.ItemDefinition.Hands != ItemDefinition.E_Hands.None) ? ResourcePooler.LoadOnce<Sprite>(string.Format("{0}{1}_On", "View/Sprites/UI/Items/Hands/Icon_ItemCategory_", Item.ItemDefinition.Hands)) : null);
		handsImage.enabled = Item.ItemDefinition.Hands != ItemDefinition.E_Hands.None;
		rarityIconImage.sprite = rarityIcons.GetSpriteAt((int)(Item.Rarity - 1));
		sellPriceText.text = (useDefaultValues ? Item.DefaultSellingPrice.ToString() : Item.SellingPrice.ToString());
		equippedText.enabled = Item.ItemSlot is EquipmentSlot;
		for (int num = BaseAffixesPanelRectTransform.childCount - 1; num >= 0; num--)
		{
			if (BaseAffixesPanelRectTransform.GetChild(num) != baseAffixesBG && BaseAffixesPanelRectTransform.GetChild(num) != baseAffixesBotBG)
			{
				UnityEngine.Object.Destroy(BaseAffixesPanelRectTransform.GetChild(num).gameObject);
			}
		}
		if (Item.BaseStatBonuses != null)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> baseStatBonuse in Item.BaseStatBonuses)
			{
				UnityEngine.Object.Instantiate(affixTextPrefab, baseAffixesPanelGameObject.transform).Init(baseStatBonuse.Key, baseStatBonuse.Value);
			}
		}
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = Item.ItemController.MergeAffixes(Item.AdditionalAffixesMalus);
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item in dictionary)
		{
			UnityEngine.Object.Instantiate(affixTextPrefab, baseAffixesPanelGameObject.transform).Init(item.Key, 0f - item.Value);
		}
		Vector2 sizeDelta = BaseAffixesPanelRectTransform.sizeDelta;
		if ((Item.BaseStatBonuses == null || Item.BaseStatBonuses.Count == 0) && dictionary.Count == 0)
		{
			sizeDelta.y = 0f;
			baseAffixesPanelGameObject.SetActive(value: false);
		}
		else
		{
			int num2 = Item.BaseStatBonuses?.Count ?? 0;
			sizeDelta.y = (float)(num2 + dictionary.Count) * AffixTextPrefabRectTransform.sizeDelta.y + (float)BaseAffixeVerticalLayoutGroup.padding.top + (float)BaseAffixeVerticalLayoutGroup.padding.bottom;
			BaseAffixesPanelRectTransform.sizeDelta = sizeDelta;
			baseAffixesPanelGameObject.SetActive(value: true);
		}
		for (int num3 = AdditionalAffixesPanelRectTransform.transform.childCount - 1; num3 >= 0; num3--)
		{
			if (AdditionalAffixesPanelRectTransform.GetChild(num3) != additionalAffixesBGImage.transform)
			{
				UnityEngine.Object.Destroy(AdditionalAffixesPanelRectTransform.GetChild(num3).gameObject);
			}
		}
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary2 = Item.ItemController.MergeAffixes(Item.AdditionalAffixes);
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item2 in dictionary2)
		{
			UnityEngine.Object.Instantiate(affixTextPrefab, additionalAffixesPanelGameObject.transform).Init(item2.Key, item2.Value);
		}
		Vector2 sizeDelta2 = AdditionalAffixesPanelRectTransform.sizeDelta;
		bool flag = (dictionary2.Count == 0 && Item.BaseStatBonuses != null && Item.BaseStatBonuses.Count > 0) || dictionary.Count > 0;
		baseAffixesBotBG.gameObject.SetActive(flag);
		if (flag)
		{
			baseAffixesBG.offsetMin = new Vector2(baseAffixesBG.offsetMin.x, baseAffixesBGBotOffsetInit - baseAffixesBotBG.anchoredPosition.y);
		}
		else
		{
			baseAffixesBG.offsetMin = new Vector2(baseAffixesBG.offsetMin.x, baseAffixesBGBotOffsetInit);
		}
		if (dictionary2.Count == 0)
		{
			sizeDelta2.y = 0f;
			additionalAffixesPanelGameObject.SetActive(value: false);
		}
		else
		{
			sizeDelta2.y = (float)dictionary2.Count * AffixTextPrefabRectTransform.sizeDelta.y + (float)AdditionalAffixeVerticalLayoutGroup.padding.top + (float)AdditionalAffixeVerticalLayoutGroup.padding.bottom;
			AdditionalAffixesPanelRectTransform.sizeDelta = sizeDelta2;
			additionalAffixesPanelGameObject.SetActive(value: true);
		}
		additionalAffixesBGImage.sprite = additionalAffixesRartityBGSprites.GetSpriteAt((int)(Item.Rarity - 1));
		if (RarityParticles != null)
		{
			foreach (KeyValuePair<ItemDefinition.E_Rarity, GameObject> rarityParticle in RarityParticles)
			{
				rarityParticle.Value?.SetActive(rarityParticle.Key == Item.Rarity);
			}
		}
		skillDisplay.SkillAreaOfEffectGridPlacedEvent += RefreshTooltipWidth;
		RefreshCarousel();
		RefreshSkill();
		RefreshPerk();
		if (Item != null)
		{
			ResizeGlow();
		}
	}

	private void RefreshTooltipWidth()
	{
		skillDisplay.SkillAreaOfEffectGridPlacedEvent -= RefreshTooltipWidth;
		float x = 275f;
		if (skillDisplayPanel.activeSelf && skillDisplay.SkillAreaOfEffectGrid.Displayed)
		{
			x = Mathf.Max(skillDisplay.SkillParametersContainer.localPosition.x + skillDisplay.SkillAreaOfEffectGrid.RectTransform.localPosition.x + skillDisplay.SkillAreaOfEffectGrid.RectTransform.sizeDelta.x + -20f, 275f);
		}
		itemTooltipPanel.sizeDelta = new Vector2(x, itemTooltipPanel.sizeDelta.y);
		RectTransform layoutRoot = horizontalLayoutGroup.transform as RectTransform;
		LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
		layoutRoot.ClampToParent();
	}

	public void AddAttackTypeEntries()
	{
		if (skillDisplay.Skill?.SkillAction is AttackSkillAction attackSkillAction)
		{
			ItemTooltipSkillCycle.CompendiumPanel.AddDamageType(attackSkillAction);
		}
	}

	public void AddSkillEffectEntries()
	{
		if (skillDisplay.Skill != null && skillDisplay.Skill.SkillAction.SkillActionDefinition.SkillEffectDefinitions != null)
		{
			ItemTooltipSkillCycle.CompendiumPanel.AddSkillEffectIds(skillDisplay.Skill.SkillAction.SkillActionDefinition.SkillEffectDefinitions);
		}
	}

	public void AddPerkEffectEntries()
	{
		if (!(unitPerkDisplay != null) || unitPerkDisplay.PerkDefinition == null)
		{
			return;
		}
		foreach (CompendiumEntryDefinition compendiumEntry in unitPerkDisplay.PerkDefinition.CompendiumEntries)
		{
			ItemTooltipSkillCycle.CompendiumPanel.AddCompendiumEntry(compendiumEntry.Id, compendiumEntry.DisplayLinkedEntries);
		}
		if (unitPerkDisplay.PerkDefinition.SkillsToShow.Count > 0)
		{
			string item = unitPerkDisplay.PerkDefinition.SkillsToShow[Mathf.Min(ItemTooltipSkillCycle.PerkSkillTabIndex, unitPerkDisplay.PerkDefinition.SkillsToShow.Count - 1)].Item1;
			if (SkillDatabase.SkillDefinitions.TryGetValue(item, out var value) && value.SkillActionDefinition.SkillEffectDefinitions != null)
			{
				ItemTooltipSkillCycle.CompendiumPanel.AddSkillEffectIds(value.SkillActionDefinition.SkillEffectDefinitions);
			}
			if (CurrentPerkSkill?.SkillAction is AttackSkillAction attackSkillAction)
			{
				ItemTooltipSkillCycle.CompendiumPanel.AddDamageType(attackSkillAction);
			}
		}
	}

	protected override void OnDisplay()
	{
		base.OnDisplay();
		this.ItemTooltipDisplayedChangeEvent?.Invoke(isDisplayed: true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.ItemTooltipDisplayedChangeEvent?.Invoke(isDisplayed: false);
		Item = null;
		RefreshSkill();
		RefreshPerk();
		SetPerkSkillTooltipVisible(isVisible: false);
		if (ItemTooltipSkillCycle != null)
		{
			ItemTooltipSkillCycle.UpdateCompendium();
		}
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		ItemTooltipDisplayedChangeEvent -= InputManager.OnItemTooltipDisplayedChange;
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
		if (Item != null)
		{
			itemNameText.text = Item.Name;
			subtitleText.text = ((Item.ItemDefinition.Hands != ItemDefinition.E_Hands.None) ? (Item.ItemDefinition.HandsName + " - ") : string.Empty) + ((Item.ItemDefinition.Category != ItemDefinition.E_Category.None) ? (Item.ItemDefinition.CategoryName + " - ") : string.Empty) + Item.RarityName;
			if (Item.MainStatBonusByLevel != null)
			{
				float item = Item.MainStatBonusByLevel.Item2;
				mainStatNameText.text = "<size=16><sprite name=\"" + Item.MainStatBonusByLevel.Item1.ToString() + "\"></size>" + UnitDatabase.UnitStatDefinitions[Item.MainStatBonusByLevel.Item1].ShortName;
				mainStatValueText.text = string.Format("<style=\"{0}\">{1}{2}{3}</style>", (item >= 0f) ? "GoodNbOutlined" : "BadNbOutlined", (item >= 0f) ? "+" : string.Empty, item, Item.MainStatBonusByLevel.Item1.ShownAsPercentage() ? "%" : string.Empty);
			}
			else if (Item.BaseDamages != Vector2.zero)
			{
				mainStatNameText.text = Localizer.Get("ItemTooltip_BaseDamageName");
				mainStatValueText.text = Localizer.Format("ItemTooltip_BaseDamageValue", Item.BaseDamages.x, Item.BaseDamages.y);
			}
			else
			{
				mainStatNameText.text = "<color=#999999>-";
				mainStatValueText.text = string.Empty;
			}
		}
	}

	private void ResizeGlow()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(allStatsPanel);
		float num = 0f;
		for (int i = 0; i < tooltipPanel.childCount; i++)
		{
			if (tooltipPanel.GetChild(i) != glowImage.transform)
			{
				num += (tooltipPanel.GetChild(i).transform as RectTransform).sizeDelta.y;
			}
		}
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, num - (float)allStatsLayout.padding.top);
		glowImage.sprite = glowSprites.GetSpriteAt((int)(Item.Rarity - 1));
		glowRectTransform.sizeDelta = new Vector2(glowRectTransform.sizeDelta.x, tooltipPanel.sizeDelta.y + glowDeltaHeight);
	}

	private void RemovePerkLink()
	{
		if (CurrentPerk != null)
		{
			CurrentPerk.PerkController.ChangeOwner(null);
			CurrentPerk = null;
		}
	}

	private void SetPerkSkillTooltipVisible(bool isVisible)
	{
		perkSkillTooltipContainer.SetActive(isVisible);
		if (isVisible)
		{
			perkSkillTooltip.Display();
		}
		else
		{
			perkSkillTooltip.Hide();
		}
	}

	private IEnumerator RefreshCarouselPanelCanvasSortingOrder()
	{
		carouselPanelCanvas.sortingOrder = carouselPanelCanvasDefaultSortingOrder + 1;
		yield return SharedYields.WaitForEndOfFrame;
		carouselPanelCanvas.sortingOrder = carouselPanelCanvasDefaultSortingOrder;
	}
}

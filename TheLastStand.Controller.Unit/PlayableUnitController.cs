using System;
using System.Collections.Generic;
using System.Linq;
using PortraitAPI;
using PortraitAPI.Misc;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Meta;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.PlayableUnitGeneration;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Movement;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Serialization.Perk;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;
using TheLastStand.View.TileMap;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.Controller.Unit;

public class PlayableUnitController : UnitController
{
	private readonly List<TheLastStand.Model.Skill.Skill> contextualSkills = new List<TheLastStand.Model.Skill.Skill>();

	private readonly List<TheLastStand.Model.Skill.Skill> equipmentSkills = new List<TheLastStand.Model.Skill.Skill>();

	private readonly List<TheLastStand.Model.Skill.Skill> weaponSkills = new List<TheLastStand.Model.Skill.Skill>();

	public PlayableUnit PlayableUnit => base.Unit as PlayableUnit;

	public List<int> DebuffedByGhosts { get; } = new List<int>();

	public PlayableUnitController(SerializedPlayableUnit serializedPlayableUnit, int saveVersion = -1, bool isDead = false)
	{
		base.Unit = new PlayableUnit(PlayableUnitDatabase.PlayableUnitTemplateDefinition, serializedPlayableUnit, this, saveVersion, isDead);
		base.Unit.DeserializeAfterInit(serializedPlayableUnit, saveVersion);
		ComputeExperienceNeededToNextLevel();
		GenerateNativeSkills(onLoad: true);
		GenerateNativePerks(serializedPlayableUnit.NativePerks, isDead);
		GenerateRacePerks(serializedPlayableUnit.RacePerks, isDead);
		GenerateDynamicPerks(serializedPlayableUnit.DynamicPerks, isDead);
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				if (equipmentSlot.Value[i].Item != null)
				{
					EquipItem(equipmentSlot.Value[i].Item, equipmentSlot.Value[i], shouldRefreshMetaCondition: true, onLoad: true);
				}
			}
		}
		PlayableUnitView.GeneratePortrait(PlayableUnit, serializedPlayableUnit);
		UpdateInjuryStage();
	}

	public PlayableUnitController(string archetypeId, int traitPoints, UnitView view = null, Tile tile = null, int level = 1, string raceDefinitionId = null, bool isStartingUnit = false)
	{
		base.Unit = new PlayableUnit(PlayableUnitDatabase.PlayableUnitTemplateDefinition, this, view, archetypeId, isStartingUnit)
		{
			State = TheLastStand.Model.Unit.Unit.E_State.Ready
		};
		SetTile(tile);
		GenerateRace(raceDefinitionId);
		if (PlayableUnit.RaceDefinition.GenderGenerationWeights.Count > 0)
		{
			int max = PlayableUnit.RaceDefinition.GenderGenerationWeights.Sum((KeyValuePair<string, int> weight) => weight.Value);
			int randomRange = RandomManager.GetRandomRange(this, 0, max);
			int num = 0;
			foreach (KeyValuePair<string, int> genderGenerationWeight in PlayableUnit.RaceDefinition.GenderGenerationWeights)
			{
				num += genderGenerationWeight.Value;
				if (randomRange < num)
				{
					PlayableUnit.Gender = genderGenerationWeight.Key;
					break;
				}
			}
		}
		else
		{
			PlayableUnit.Gender = (RandomManager.GetRandomBool(this) ? "Male" : "Female");
		}
		PlayableUnit.PlayableUnitName = RandomManager.GetRandomElement(this, PlayableUnit.RaceDefinition.GetNamesForGender(PlayableUnit.Gender));
		if (base.Unit.UnitView != null)
		{
			base.Unit.UnitView.name = base.Unit.Id;
		}
		PlayableUnit.FaceId = GetRandomFaceId(PlayableUnit.Gender, PlayableUnit.RaceDefinition.Id);
		PlayableUnitView.GenerateRandomPortrait(PlayableUnit);
		PlayableUnit.LevelUp = new UnitLevelUpController(PlayableUnitDatabase.UnitLevelUpDefinition, TPSingleton<UnitLevelUpView>.Instance).UnitLevelUp;
		PlayableUnit.LevelUp.PlayableUnit = PlayableUnit;
		PlayableUnit.PerkTree = new UnitPerkTreeController(TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView, PlayableUnit).UnitPerkTree;
		PlayableUnit.PerkTree.UnitPerkTreeController.GeneratePerkTree(PlayableUnit);
		Generate(traitPoints, level, isStartingUnit);
		ComputeExperienceNeededToNextLevel();
		GenerateContextualSkills();
		GenerateNativeSkills(onLoad: false);
		GenerateNativePerks();
		GenerateRacePerks();
	}

	public static void GenerateEquipment(PlayableUnitGenerationDefinition unitGenerationDefinition, PlayableUnit playableUnit, ItemSlotDefinition.E_ItemSlotId availableSlots = ItemSlotDefinition.E_ItemSlotId.EquipmentSlot, bool generateItemsToInventory = false)
	{
		List<string> list = new List<string>();
		string[] array = ItemManager.GetAllLockedItemsIds().ToArray();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockEquipmentGenerationMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				list.Add(effects[i].Id);
			}
		}
		List<string> list2 = ItemManager.GetAllItemsIds(TPSingleton<GlyphManager>.Instance.StartingGearGenerationPriorityItemLists).ToList();
		List<ItemDefinition> list3 = new List<ItemDefinition>();
		if (list2.Count > 0 && playableUnit.IsStartingUnit)
		{
			foreach (string item2 in list2)
			{
				if (ItemDatabase.ItemDefinitions.TryGetValue(item2, out var value) && !array.Contains(item2) && !list3.Contains(value))
				{
					list3.Add(value);
				}
			}
		}
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, EquipmentGenerationDefinition> equipmentGenerationDefinition in unitGenerationDefinition.EquipmentGenerationDefinitions)
		{
			ItemSlotDefinition.E_ItemSlotId key = equipmentGenerationDefinition.Key;
			if (!playableUnit.EquipmentSlots.ContainsKey(key) || !availableSlots.HasFlag(key))
			{
				continue;
			}
			EquipmentGenerationDefinition value2 = equipmentGenerationDefinition.Value;
			int num = value2.TotalWeight;
			List<Tuple<int, EquipmentGenerationDefinition.ItemGenerationData>> list4 = new List<Tuple<int, EquipmentGenerationDefinition.ItemGenerationData>>(value2.ItemsPerWeight);
			for (int j = 0; j < list4.Count; j++)
			{
				if (list4[j].Item2.ItemId != string.Empty && (!list.Contains(list4[j].Item2.ItemId) || IsItemGenerationDataLocked(list4[j].Item2, array)))
				{
					num -= list4[j].Item1;
					list4.RemoveAt(j--);
				}
			}
			int num2 = RandomManager.GetRandomRange(playableUnit, 0, num);
			foreach (Tuple<int, EquipmentGenerationDefinition.ItemGenerationData> item3 in list4)
			{
				num2 -= item3.Item1;
				if (num2 >= 0)
				{
					continue;
				}
				string itemsList = item3.Item2.ItemsList;
				List<string> list5 = new List<string>();
				if (list3.Count > 0)
				{
					List<string> list6 = new List<string>();
					ItemDefinition.E_Category e_Category = ItemDefinition.E_Category.None;
					if (playableUnit.EquipmentSlots.TryGetValue(key, out var value3) && value3.Count > 0)
					{
						e_Category = value3[0].ItemSlotDefinition.Categories;
					}
					foreach (ItemDefinition item4 in list3)
					{
						if (e_Category.HasFlag(item4.Category) && !list6.Contains(item4.Id))
						{
							list6.Add(item4.Id);
						}
					}
					if (list6.Count > 0)
					{
						HashSet<string> allItemsIds = ItemManager.GetAllItemsIds(new List<string> { itemsList });
						if (allItemsIds.Count > 0)
						{
							foreach (string item5 in list6)
							{
								if (allItemsIds.Contains(item5))
								{
									list5.Add(item5);
								}
							}
						}
					}
				}
				int level = new LevelProbabilitiesTreeController(unitGenerationDefinition.BaseGenerationLevel, ItemDatabase.ItemGenerationModifierListDefinitions[item3.Item2.ItemLevelModifiersList]).GenerateLevel();
				ItemsListDefinition itemsListDefinition = ItemDatabase.ItemsListDefinitions[itemsList];
				int num3 = 0;
				bool flag = false;
				foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in playableUnit.EquipmentSlots)
				{
					if (equipmentSlot.Key == ItemSlotDefinition.E_ItemSlotId.LeftHand)
					{
						flag = true;
						break;
					}
				}
				Func<ItemDefinition, bool> func = (ItemDefinition itemDefinition2) => itemDefinition2.Hands == ItemDefinition.E_Hands.OneHand;
				bool flag2;
				do
				{
					flag2 = true;
					ItemDefinition itemDefinition = ItemManager.TakeRandomItemInList(itemsListDefinition, (key == ItemSlotDefinition.E_ItemSlotId.RightHand && !flag) ? func : null, null, list5);
					if (itemDefinition == null)
					{
						continue;
					}
					if (itemDefinition.Hands == ItemDefinition.E_Hands.TwoHands && !flag)
					{
						flag2 = false;
					}
					if (!flag2)
					{
						continue;
					}
					int minRarityIndexFromItemDefinition = RarityProbabilitiesTreeController.GetMinRarityIndexFromItemDefinition(itemDefinition);
					ItemManager.ItemGenerationInfo generationInfo = new ItemManager.ItemGenerationInfo
					{
						Destination = (generateItemsToInventory ? ItemSlotDefinition.E_ItemSlotId.Inventory : key),
						ItemDefinition = itemDefinition,
						Level = itemDefinition.GetHigherExistingLevelFromInitValue(level),
						Rarity = RarityProbabilitiesTreeController.GenerateRarity(ItemDatabase.ItemRaritiesListDefinitions[item3.Item2.ItemRaritiesList], minRarityIndexFromItemDefinition),
						SkipMalusAffixes = true
					};
					if (generationInfo.Level == -1)
					{
						flag2 = false;
						continue;
					}
					TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(generationInfo);
					if (!generateItemsToInventory)
					{
						playableUnit.PlayableUnitController.EquipItem(item, null, shouldRefreshMetaCondition: false, onLoad: true);
					}
				}
				while (!flag2 && ++num3 < 1000);
				if (num3 == 1000)
				{
					TPSingleton<PlayableUnitManager>.Instance.LogWarning("The generation of " + playableUnit.PlayableUnitName + "'s equipment took way longer than expected and couldn't find a suitable item.");
				}
				break;
			}
		}
	}

	public static ColorSwapPaletteDefinition GetRandomHairColorSwapPalette(string skinName, Dictionary<string, ColorSwapPaletteDefinition> colorSwapPalettes)
	{
		string randomHairColorId = PlayableUnitDatabase.UnitLinkHairSkin.GetRandomHairColorId(skinName);
		return colorSwapPalettes[randomHairColorId];
	}

	public static ColorSwapPaletteDefinition RandomizeColorSwapPaletteDefinition(Dictionary<string, ColorSwapPaletteDefinition> colorSwapPalettes, out string colorSwapName)
	{
		int num = 0;
		for (int i = 0; i < colorSwapPalettes.Values.Count; i++)
		{
			num += colorSwapPalettes.Values.ElementAt(i).Weight;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, num);
		int num2 = 0;
		for (int j = 0; j < colorSwapPalettes.Values.Count; j++)
		{
			if (j == 0)
			{
				if (randomRange >= 0 && randomRange < colorSwapPalettes.Values.ElementAt(j).Weight)
				{
					colorSwapName = colorSwapPalettes.Values.ElementAt(j).Id;
					return colorSwapPalettes.Values.ElementAt(j);
				}
				num2 += colorSwapPalettes.Values.ElementAt(j).Weight;
			}
			else
			{
				if (randomRange >= num2 && randomRange < colorSwapPalettes.Values.ElementAt(j).Weight + num2)
				{
					colorSwapName = colorSwapPalettes.Values.ElementAt(j).Id;
					return colorSwapPalettes.Values.ElementAt(j);
				}
				num2 += colorSwapPalettes.Values.ElementAt(j).Weight;
			}
		}
		colorSwapName = colorSwapPalettes.Values.ElementAt(0).Id;
		return colorSwapPalettes.Values.ElementAt(0);
	}

	private static bool IsItemGenerationDataLocked(EquipmentGenerationDefinition.ItemGenerationData itemGenerationData, string[] lockedItemIds)
	{
		if (ItemDatabase.ItemsListDefinitions.TryGetValue(itemGenerationData.ItemsList, out var value))
		{
			return ItemManager.IsItemsListContentLocked(value, lockedItemIds);
		}
		return true;
	}

	public void AddCrossedTiles(int amount, bool ignoreMomentum = false)
	{
		PlayableUnit.TilesCrossedThisTurn += amount;
		if (!ignoreMomentum)
		{
			PlayableUnit.TotalMomentumTilesCrossedThisTurn += amount;
			PlayableUnit.MomentumTilesActive += amount;
		}
	}

	public bool AddTrait(string traitId, bool forceAdd = false)
	{
		UnitTraitDefinition unitTraitDefinition = PlayableUnitDatabase.UnitTraitDefinitions[traitId];
		bool flag = false;
		if (!forceAdd)
		{
			bool flag2 = false;
			for (int i = 0; i < PlayableUnit.UnitTraitDefinitions.Count; i++)
			{
				if (PlayableUnit.UnitTraitDefinitions[i].Incompatibilities.Contains(traitId))
				{
					flag2 = true;
					break;
				}
			}
			if (!PlayableUnit.UnitTraitDefinitions.Contains(unitTraitDefinition) && !flag2)
			{
				PlayableUnit.UnitTraitDefinitions.Add(unitTraitDefinition);
				flag = true;
			}
		}
		else
		{
			PlayableUnit.UnitTraitDefinitions.Add(unitTraitDefinition);
			flag = true;
		}
		if (flag)
		{
			AddSlots(unitTraitDefinition.AddSlots);
			RemoveSlots(unitTraitDefinition.RemoveSlots);
			PlayableUnit.PlayableUnitStatsController.OnTraitGenerated(unitTraitDefinition);
		}
		return flag;
	}

	public void AssignItemSlotViewsToUnit()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in CharacterSheetPanel.EquipmentSlots)
		{
			if (ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot.Key))
			{
				equipmentSlot.Value[0].ItemSlot = ((PlayableUnit.EquipmentSlots.ContainsKey(equipmentSlot.Key) && PlayableUnit.EquipmentSlots[equipmentSlot.Key].Count > PlayableUnit.EquippedWeaponSetIndex) ? PlayableUnit.EquipmentSlots[equipmentSlot.Key][PlayableUnit.EquippedWeaponSetIndex] : null);
				equipmentSlot.Value[1].ItemSlot = ((PlayableUnit.EquipmentSlots.ContainsKey(equipmentSlot.Key) && PlayableUnit.EquipmentSlots[equipmentSlot.Key].Count > ((PlayableUnit.EquippedWeaponSetIndex == 0) ? 1 : 0)) ? PlayableUnit.EquipmentSlots[equipmentSlot.Key][(PlayableUnit.EquippedWeaponSetIndex == 0) ? 1 : 0] : null);
				if (TPSingleton<CharacterSheetPanel>.Instance.IsOpened)
				{
					equipmentSlot.Value[0].Refresh();
					equipmentSlot.Value[1].Refresh();
				}
			}
			else
			{
				for (int i = 0; i < equipmentSlot.Value.Count; i++)
				{
					equipmentSlot.Value[i].ItemSlot = ((PlayableUnit.EquipmentSlots.ContainsKey(equipmentSlot.Key) && PlayableUnit.EquipmentSlots[equipmentSlot.Key].Count > i) ? PlayableUnit.EquipmentSlots[equipmentSlot.Key][i] : null);
					equipmentSlot.Value[i].Refresh();
				}
			}
		}
	}

	public void ComputeReachableTiles()
	{
		if (TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution == null && TileObjectSelectionManager.HasPlayableUnitSelected && base.MoveTask == null)
		{
			int movePoints = (int)PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).FinalClamped;
			if (PlayableUnit.OriginTile.Building != null && PlayableUnit.OriginTile.Building.IsWatchtower)
			{
				movePoints = 0;
			}
			else if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
			{
				PathfindingManager.Pathfinding.PathfindingController.AddAllReachableTilesToPath(PlayableUnit);
				return;
			}
			PathfindingManager.Pathfinding.PathfindingController.ComputeReachableTiles(base.Unit, movePoints);
		}
	}

	public override void EndTurn()
	{
		base.EndTurn();
		PlayableUnit.LastTurnHealth = PlayableUnit.Health;
		PlayableUnit.ActionPointsSpentThisTurn = 0;
		EffectManager.DisplayEffects();
	}

	public void EquipItem(TheLastStand.Model.Item.Item item, EquipmentSlot targetEquipmentSlot = null, bool shouldRefreshMetaCondition = true, bool onLoad = false)
	{
		if (targetEquipmentSlot == null)
		{
			targetEquipmentSlot = GetBestItemSlot(item);
			if (targetEquipmentSlot == null)
			{
				return;
			}
		}
		if ((item.IsTwoHandedWeapon && targetEquipmentSlot.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.RightHand && !targetEquipmentSlot.EquipmentSlotController.CanEquipTwoHandedWeapon(item)) || targetEquipmentSlot.BlockedByOtherSlot != null)
		{
			return;
		}
		if (item.ItemSlot != null)
		{
			if (!(item.ItemSlot is EquipmentSlot equipmentSlot) || targetEquipmentSlot.Item == null || equipmentSlot.ItemSlotController.IsItemCompatible(targetEquipmentSlot.Item))
			{
				targetEquipmentSlot.ItemSlotController.SwapItems(item.ItemSlot, onLoad);
			}
		}
		else
		{
			targetEquipmentSlot.ItemSlotController.SetItem(item, onLoad);
		}
		if (item.IsTwoHandedWeapon && targetEquipmentSlot.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.RightHand)
		{
			int num = PlayableUnit.EquipmentSlots[targetEquipmentSlot.ItemSlotDefinition.Id].IndexOf(targetEquipmentSlot);
			bool flag = false;
			foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot3 in PlayableUnit.EquipmentSlots)
			{
				for (int i = 0; i < equipmentSlot3.Value.Count; i++)
				{
					EquipmentSlot equipmentSlot2 = equipmentSlot3.Value[i];
					if (equipmentSlot2.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.LeftHand && i == num)
					{
						equipmentSlot2.ItemSlotController.SwapItems(null, onLoad);
						targetEquipmentSlot.BlockOtherSlot = equipmentSlot2;
						equipmentSlot2.BlockedByOtherSlot = targetEquipmentSlot;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (!onLoad)
		{
			RefreshStats();
			PlayableUnit.PlayableUnitView?.RefreshBodyParts();
			PlayableUnit.PlayableUnitView?.RefreshHealth();
			PlayableUnit.PlayableUnitView?.RefreshArmor();
		}
		if (!onLoad && shouldRefreshMetaCondition)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item2 in item.GetAllStatBonusesMerged())
			{
				TPSingleton<MetaConditionManager>.Instance.RefreshMaxHeroStatReached(item2.Key, PlayableUnit.UnitStatsController.GetStat(item2.Key).FinalClamped);
			}
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet && TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened)
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.IsDirty = true;
		}
	}

	public override void FilterTilesInRange(TilesInRangeInfos tilesInRangeInfos, List<Tile> skillSourceTiles)
	{
		base.FilterTilesInRange(tilesInRangeInfos, skillSourceTiles);
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> item in tilesInRangeInfos.Range)
		{
			if (item.Key.HasAnyFog)
			{
				item.Value.HasLineOfSight = false;
				item.Value.TileColor = TileMapView.SkillHiddenRangeTilesColorInvalidOrientation._Color;
			}
		}
	}

	public void GainExperience(float amount)
	{
		if (amount < 0f)
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Negative experience !", CLogLevel.MAJOR);
			return;
		}
		amount = amount * PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ExperienceGainMultiplier).FinalClamped / 100f;
		PlayableUnit.Experience += amount;
		PlayableUnit.ExperienceInCurrentLevel += amount;
		while (PlayableUnit.ExperienceInCurrentLevel >= PlayableUnit.ExperienceNeededToNextLevel)
		{
			LevelUp();
		}
	}

	public void LevelUp()
	{
		PlayableUnit.ExperienceInCurrentLevel -= PlayableUnit.ExperienceNeededToNextLevel;
		PlayableUnit.Level++;
		PlayableUnit.UnitLevelUpPoints.Add(new UnitLevelUpPoint());
		PlayableUnit.LevelUp.CommonNbReroll += PlayableUnit.LevelUp.UnitLevelUpDefinition.MaxAmountOfReroll + TPSingleton<GlyphManager>.Instance.BonusLevelupRerolls;
		TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaxHeroLevelReached, PlayableUnit.Level);
		if (PlayableUnitDatabase.PerksPointsPerLevel.TryGetValue((int)PlayableUnit.Level, out var value))
		{
			PlayableUnit.PerksPoints += value;
		}
		ComputeExperienceNeededToNextLevel();
	}

	public override float GainHealth(float amount, bool refreshHud = true)
	{
		float num = base.GainHealth(amount, refreshHud);
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightHpLost -= num;
		}
		return num;
	}

	public float GainMana(float amount)
	{
		float num = PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).Base;
		PlayableUnit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.Mana, amount, includeChildStat: false);
		(PlayableUnit.PlayableUnitView.UnitHUD as PlayableUnitHUD).PlayManaGainAnim(amount, base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana));
		return PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).Base - num;
	}

	public List<TheLastStand.Model.Skill.Skill> GetAllSkillsNoCheck(bool sort = false)
	{
		List<TheLastStand.Model.Skill.Skill> list = new List<TheLastStand.Model.Skill.Skill>();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			foreach (EquipmentSlot item in equipmentSlot.Value)
			{
				if (item.Item == null)
				{
					continue;
				}
				foreach (TheLastStand.Model.Skill.Skill skill in item.Item.Skills)
				{
					list.Add(GetItemSkillReplacementIfIsValid(item.Item, skill));
				}
			}
		}
		list.AddRange(PlayableUnit.ContextualSkills);
		list.AddRange(PlayableUnit.NativeSkills);
		if (sort)
		{
			list.Sort(TheLastStand.Model.Skill.Skill.SharedSkillBarIndexComparer);
		}
		return list;
	}

	public List<EquipmentSlot> GetCompatibleSlots(TheLastStand.Model.Item.Item item)
	{
		List<EquipmentSlot> list = new List<EquipmentSlot>();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				if (equipmentSlot.Value[i].EquipmentSlotController.IsItemCompatible(item))
				{
					list.Add(equipmentSlot.Value[i]);
				}
			}
		}
		return list;
	}

	public EquipmentSlot GetBestItemSlotToCompare(TheLastStand.Model.Item.Item item)
	{
		if (item.ItemDefinition.IsHandItem)
		{
			List<EquipmentSlot> value2;
			if (item.ItemDefinition.Hands == ItemDefinition.E_Hands.OffHand)
			{
				if (PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.LeftHand, out var value))
				{
					EquipmentSlot equipmentSlot = value[PlayableUnit.EquippedWeaponSetIndex];
					return equipmentSlot.BlockedByOtherSlot ?? equipmentSlot;
				}
			}
			else if (PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.RightHand, out value2))
			{
				return value2[PlayableUnit.EquippedWeaponSetIndex];
			}
			return null;
		}
		List<EquipmentSlot> list = new List<EquipmentSlot>();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot3 in PlayableUnit.EquipmentSlots)
		{
			EquipmentSlot equipmentSlot2 = equipmentSlot3.Value.FirstOrDefault();
			if (equipmentSlot2 != null && equipmentSlot2.ItemSlotController.IsItemCompatible(item))
			{
				list.AddRange(equipmentSlot3.Value);
			}
		}
		return list.FirstOrDefault((EquipmentSlot slot) => slot.Item != null);
	}

	public List<TheLastStand.Model.Skill.Skill> GetEquipmentSkills(bool dontCheckPhase = false)
	{
		equipmentSkills.Clear();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			if (ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot.Key))
			{
				continue;
			}
			foreach (EquipmentSlot item in equipmentSlot.Value)
			{
				if (item.Item == null)
				{
					continue;
				}
				foreach (TheLastStand.Model.Skill.Skill skill in item.Item.Skills)
				{
					if (skill.SkillController.CheckConditions(PlayableUnit, dontCheckPhase))
					{
						equipmentSkills.Add(GetItemSkillReplacementIfIsValid(item.Item, skill));
					}
				}
			}
		}
		return equipmentSkills;
	}

	public List<TheLastStand.Model.Skill.Skill> GetContextualSkills()
	{
		contextualSkills.Clear();
		foreach (TheLastStand.Model.Skill.Skill contextualSkill in PlayableUnit.ContextualSkills)
		{
			contextualSkill.SkillAction.SkillActionExecution.Caster = PlayableUnit;
			contextualSkill.SkillAction.SkillActionExecution.SkillSourceTileObject = PlayableUnit;
			if (contextualSkill.SkillController.CheckConditions(PlayableUnit))
			{
				contextualSkill.SkillAction.SkillActionExecution.SkillExecutionController.ComputeSkillRangeTiles(updateView: false);
				if (contextualSkill.SkillController.ComputeTargetsAndValidity(PlayableUnit) || contextualSkill.SkillDefinition.InvalidCastDisplayBehaviour == SkillDefinition.E_InvalidCastDisplayBehaviour.DisplayedUnavailable)
				{
					contextualSkills.Add(contextualSkill);
				}
			}
		}
		contextualSkills.Sort(TheLastStand.Model.Skill.Skill.SharedSkillBarIndexComparer);
		return contextualSkills;
	}

	public TheLastStand.Model.Skill.Skill GetItemSkillReplacementIfIsValid(TheLastStand.Model.Item.Item item, TheLastStand.Model.Skill.Skill itemSkill, bool skipConditions = false)
	{
		if (item == null || PlayableUnit.PlayableUnitPerksController.PlayableUnitPerks.ReplaceItemSkillEffects.Count == 0)
		{
			return itemSkill;
		}
		ReplaceItemSkillEffect replaceItemSkillEffect = PlayableUnit.PlayableUnitPerksController.PlayableUnitPerks.ReplaceItemSkillEffects.Find((ReplaceItemSkillEffect replaceSkillEffect) => replaceSkillEffect.ReplaceItemSkillEffectDefinition.SkillIdToReplace == itemSkill.Id);
		TheLastStand.Model.Skill.Skill skill = itemSkill;
		if (replaceItemSkillEffect == null || (!replaceItemSkillEffect.PerkDataConditions.IsValid(new PerkDataContainer()) && !skipConditions))
		{
			return skill;
		}
		string skillIdReplacement = replaceItemSkillEffect.ReplaceItemSkillEffectDefinition.SkillIdReplacement;
		SkillDefinition value;
		if (item.HasReplacementSkill(skillIdReplacement))
		{
			skill = item.GetReplacementSkill(skillIdReplacement);
		}
		else if (!SkillDatabase.SkillDefinitions.TryGetValue(skillIdReplacement, out value))
		{
			TPSingleton<PlayableUnitManager>.Instance.LogError("Skill replacement " + skillIdReplacement + " not found!");
		}
		else
		{
			TheLastStand.Model.Skill.Skill skill2 = new SkillController(value, item, replaceItemSkillEffect.ReplaceItemSkillEffectDefinition.OverallUses, value.UsesPerTurnCount).Skill;
			item.ItemController.PerkAddReplacementSkill(skill2);
			skill = skill2;
		}
		if (replaceItemSkillEffect.ReplaceItemSkillEffectDefinition.HasUsesPerTurnLinked)
		{
			TheLastStand.Model.Skill.Skill skillOrReplacementSkillFromId = item.GetSkillOrReplacementSkillFromId(replaceItemSkillEffect.ReplaceItemSkillEffectDefinition.LinkedSkillIdForUsesPerTurn);
			if (skillOrReplacementSkillFromId != null)
			{
				skill.SkillController.SetLinkedSkillForUses(skillOrReplacementSkillFromId);
			}
		}
		return skill;
	}

	public List<TheLastStand.Model.Skill.Skill> GetWeaponSkills(bool dontCheckPhase = false, bool getBaseAndReplacementSkills = false)
	{
		weaponSkills.Clear();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			if (!ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot.Key))
			{
				continue;
			}
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				if (PlayableUnit.EquippedWeaponSetIndex != i || equipmentSlot.Value[i].Item == null)
				{
					continue;
				}
				foreach (TheLastStand.Model.Skill.Skill skill in equipmentSlot.Value[i].Item.Skills)
				{
					if (skill.SkillController.CheckConditions(PlayableUnit, dontCheckPhase))
					{
						weaponSkills.Add(GetItemSkillReplacementIfIsValid(equipmentSlot.Value[i].Item, skill));
						if (getBaseAndReplacementSkills && !weaponSkills.Contains(skill))
						{
							weaponSkills.Add(skill);
						}
					}
				}
				if (!getBaseAndReplacementSkills)
				{
					continue;
				}
				foreach (TheLastStand.Model.Skill.Skill perkReplacementSkill in equipmentSlot.Value[i].Item.PerkReplacementSkills)
				{
					if (!weaponSkills.Contains(perkReplacementSkill))
					{
						weaponSkills.Add(perkReplacementSkill);
					}
				}
			}
		}
		foreach (TheLastStand.Model.Skill.Skill nativeSkill in PlayableUnit.NativeSkills)
		{
			if (nativeSkill.SkillController.CheckConditions(PlayableUnit, dontCheckPhase))
			{
				weaponSkills.Add(nativeSkill);
			}
		}
		return weaponSkills;
	}

	public List<TheLastStand.Model.Skill.Skill> GetSkills(bool avoidContextualSkills = false, bool dontCheckPhase = false)
	{
		List<TheLastStand.Model.Skill.Skill> list = new List<TheLastStand.Model.Skill.Skill>();
		list.AddRange(GetWeaponSkills(dontCheckPhase));
		list.AddRange(GetEquipmentSkills(dontCheckPhase));
		if (!avoidContextualSkills)
		{
			list.AddRange(GetContextualSkills());
		}
		return list;
	}

	public List<TheLastStand.Model.Skill.Skill> GetSkillsFromSlotType(ItemSlotDefinition.E_ItemSlotId slotTypes, bool getBaseAndReplacementSkills = false)
	{
		List<TheLastStand.Model.Skill.Skill> list = new List<TheLastStand.Model.Skill.Skill>();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				if (!slotTypes.HasFlag(equipmentSlot.Key) || (ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot.Key) && PlayableUnit.EquippedWeaponSetIndex != i) || equipmentSlot.Value[i].ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.Usables || equipmentSlot.Value[i].Item == null)
				{
					continue;
				}
				foreach (TheLastStand.Model.Skill.Skill skill in equipmentSlot.Value[i].Item.Skills)
				{
					if (skill.SkillController.CheckConditions(PlayableUnit))
					{
						list.Add(GetItemSkillReplacementIfIsValid(equipmentSlot.Value[i].Item, skill));
						if (getBaseAndReplacementSkills && !list.Contains(skill))
						{
							list.Add(skill);
						}
					}
				}
				if (!getBaseAndReplacementSkills)
				{
					continue;
				}
				foreach (TheLastStand.Model.Skill.Skill perkReplacementSkill in equipmentSlot.Value[i].Item.PerkReplacementSkills)
				{
					if (!list.Contains(perkReplacementSkill))
					{
						list.Add(perkReplacementSkill);
					}
				}
			}
		}
		return list;
	}

	public override void LoseArmor(float amount, ISkillCaster attacker = null, bool refreshHud = true)
	{
		if (base.Unit.State != TheLastStand.Model.Unit.Unit.E_State.Dead)
		{
			base.LoseArmor(amount, attacker, refreshHud);
			PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesTakenOnArmor(amount);
		}
	}

	public override void LoseHealth(float amount, ISkillCaster attacker = null, bool refreshHud = true, string skillName = null)
	{
		if (base.Unit.State != TheLastStand.Model.Unit.Unit.E_State.Dead)
		{
			base.LoseHealth(amount, attacker, refreshHud, skillName);
			float num = base.Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.Health, amount, includeChildStat: false, refreshHud);
			PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseHealthLost(num);
			TrophyManager.AppendValueToTrophiesConditions<NoHealthLostTrophyConditionController>(new object[2] { PlayableUnit.RandomId, num });
			TrophyManager.AppendValueToTrophiesConditions<HealthLostTrophyConditionController>(new object[2] { PlayableUnit.RandomId, num });
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
			{
				TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightHpLost += num;
			}
			if (base.Unit.Health <= 0f)
			{
				PrepareForDeath(attacker, skillName);
			}
			else
			{
				UpdatePoisonFeedbackCondition();
			}
		}
	}

	public void OverrideBodyParts(Dictionary<string, BodyPartDefinition> overridingBodyParts, bool clear = false)
	{
		if (overridingBodyParts == null)
		{
			return;
		}
		BodyPart value = null;
		BodyPartDefinition bodyPartDefinition = null;
		BodyPartView bodyPartView = null;
		foreach (KeyValuePair<string, BodyPartDefinition> overridingBodyPart in overridingBodyParts)
		{
			bodyPartDefinition = (clear ? null : overridingBodyPart.Value);
			if (PlayableUnit.BodyParts.TryGetValue(overridingBodyPart.Key, out value) && bodyPartDefinition != value.BodyPartDefinitionOverride)
			{
				value.BodyPartDefinitionOverride = bodyPartDefinition;
				if ((bodyPartView = value.GetBodyPartView(BodyPartDefinition.E_Orientation.Front)) != null)
				{
					bodyPartView.IsDirty = true;
				}
				if ((bodyPartView = value.GetBodyPartView(BodyPartDefinition.E_Orientation.Back)) != null)
				{
					bodyPartView.IsDirty = true;
				}
			}
		}
	}

	public override void PaySkillCost(TheLastStand.Model.Skill.Skill skill)
	{
		base.Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.Mana, skill.ManaCost, includeChildStat: false, refreshHud: false);
		base.Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.Health, skill.HealthCost, includeChildStat: false, refreshHud: false);
		base.Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.ActionPoints, skill.ActionPointsCost, includeChildStat: false, refreshHud: false);
		TrophyManager.AppendValueToTrophiesConditions<ManaSpentTrophyConditionController>(new object[2] { PlayableUnit.RandomId, skill.ManaCost });
		PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseManaSpent(skill.ManaCost);
		if (skill.ActionPointsCost > 0)
		{
			TPSingleton<ToDoListView>.Instance.RefreshActionPointsNotification();
			PlayableUnit.ActionPointsSpentThisTurn += skill.ActionPointsCost;
		}
		PlayableUnit.PlayableUnitView.PlayableUnitHUD.PlayManaLossAnim(skill.ManaCost, base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana));
		PlayableUnit.PlayableUnitView.UnitHUD.PlayHealthLossAnim(skill.HealthCost, base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Health));
		SpendMovePoints(skill.MovePointsCost);
		if ((float)skill.HealthCost > 0f)
		{
			PlayableUnit.PlayableUnitController.UpdateInjuryStage();
		}
		if ((float)skill.ManaCost > 0f && PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana) == 0f)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_OUT_OF_MANA);
		}
	}

	public override void PrepareForDeath(ISkillCaster killer = null, string skillName = null)
	{
		if (Analytics.AllowedToSendData)
		{
			Analytics.SendDeathHeroEvent(PlayableUnit.AnalyticsIdentifier, skillName, killer?.Id);
		}
		base.PrepareForDeath(killer, skillName);
		TPSingleton<PlayableUnitManager>.Instance.InvokeDiedPlayableUnit(PlayableUnit);
	}

	public Task PrepareForMovement(int movePointsSpent, bool forceInstant = false)
	{
		MoveUnitCommand moveUnitCommand = new MoveUnitCommand(PlayableUnit, movePointsSpent, forceInstant);
		if (base.Unit.WillDieByPoison)
		{
			base.Unit.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: false);
		}
		PlayableUnitManager.UnitsConversation.Execute(moveUnitCommand);
		if (base.Unit.WillDieByPoison)
		{
			Task moveUnitTask = moveUnitCommand.MoveUnitTask;
			moveUnitTask.OnCompleteAction = (UnityAction)Delegate.Combine(moveUnitTask.OnCompleteAction, (UnityAction)delegate
			{
				base.Unit.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: true);
			});
		}
		Task moveUnitTask2 = moveUnitCommand.MoveUnitTask;
		moveUnitTask2.OnCompleteAction = (UnityAction)Delegate.Combine(moveUnitTask2.OnCompleteAction, (UnityAction)delegate
		{
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnPlayableUnitMovement);
		});
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseTilesCrossed(movePointsSpent);
		}
		if (PlayableUnit.Path.Count > 0)
		{
			TPSingleton<PlayableUnitManager>.Instance.InvokeMovedPlayableUnit(PlayableUnit, PlayableUnit.Path[^1]);
		}
		return moveUnitCommand.MoveUnitTask;
	}

	public override Task PrepareForMovement(bool playWalkAnim = true, bool followPathOrientation = true, float moveSpeed = -1f, float delay = 0f, bool isMovementInstant = false)
	{
		Task result = base.PrepareForMovement(playWalkAnim, followPathOrientation, moveSpeed, delay, isMovementInstant);
		if (PlayableUnit.Path.Count > 0)
		{
			TPSingleton<PlayableUnitManager>.Instance.InvokeMovedPlayableUnit(PlayableUnit, PlayableUnit.Path[^1]);
		}
		return result;
	}

	public void RandomizeColors(ref CodeGenerator.CodeData codeData)
	{
		RandomizeColorSwapPaletteDefinition(PlayableUnitDatabase.PlayableUnitSkinColorDefinitions, out var colorSwapName);
		string id = GetRandomHairColorSwapPalette(colorSwapName, PlayableUnitDatabase.PlayableUnitHairColorDefinitions).Id;
		RandomizeColorSwapPaletteDefinition(PlayableUnitDatabase.PlayableUnitEyesColorDefinitions, out var colorSwapName2);
		DataColor randomPortraitBGColor = PlayableUnitView.GetRandomPortraitBGColor(PlayableUnit);
		CodeGenerator.EncodeColorDatas(new KeyValuePair<Commons.E_ColorTypes, int>[4]
		{
			new KeyValuePair<Commons.E_ColorTypes, int>(Commons.E_ColorTypes.Skin, PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.IndexOf(colorSwapName)),
			new KeyValuePair<Commons.E_ColorTypes, int>(Commons.E_ColorTypes.Hair, PlayableUnitDatabase.PlayableUnitHairColorDefinitions.IndexOf(id)),
			new KeyValuePair<Commons.E_ColorTypes, int>(Commons.E_ColorTypes.Eyes, PlayableUnitDatabase.PlayableUnitEyesColorDefinitions.IndexOf(colorSwapName2)),
			new KeyValuePair<Commons.E_ColorTypes, int>(Commons.E_ColorTypes.Background, PlayableUnitDatabase.PortraitBackgroundColors.IndexOf(randomPortraitBGColor))
		}, ref codeData);
	}

	public void ReceiveDailyExperience(float experienceShare)
	{
		float num = 0f;
		int i = 0;
		for (int count = TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight.Count; i < count; i++)
		{
			num += TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight[i].GetTotalExperienceForEntity(PlayableUnit);
		}
		PlayableUnit.AdditionalNightExperience = Mathf.Round(PlayableUnit.AdditionalNightExperience);
		float num2 = experienceShare + num + PlayableUnit.AdditionalNightExperience;
		GainExperience(num2);
		TPSingleton<PlayableUnitManager>.Instance.Log($"Daily XP for {PlayableUnit.Name}: {experienceShare} shared + {num} from kills + {PlayableUnit.AdditionalNightExperience} additional.", CLogLevel.DETAILED);
		if (PlayableUnit.PlayableUnitView != null && num2 > 0f)
		{
			GainExperienceDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainExperienceDisplay", ResourcePooler.LoadOnce<GainExperienceDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainExperienceDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init((int)num2);
			PlayableUnit.UnitController.AddEffectDisplay(pooledComponent);
		}
		PlayableUnit.AdditionalNightExperience = 0f;
	}

	public override void RefreshStats()
	{
		base.RefreshStats();
		base.Unit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.Mana, base.Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).Base);
		base.Unit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.ActionPoints, base.Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ActionPoints).Base);
	}

	public void RemoveTrait(UnitTraitDefinition trait)
	{
		AddSlots(trait.RemoveSlots);
		RemoveSlots(trait.AddSlots);
		PlayableUnit.PlayableUnitStatsController.OnTraitRemoved(trait);
		PlayableUnit.UnitTraitDefinitions.Remove(trait);
	}

	public void ResetContextualSkillsTurnUses()
	{
		if (PlayableUnit.ContextualSkills == null)
		{
			return;
		}
		for (int i = 0; i < PlayableUnit.ContextualSkills.Count; i++)
		{
			if (PlayableUnit.ContextualSkills[i].SkillDefinition.UsesPerTurnCount != -1)
			{
				PlayableUnit.ContextualSkills[i].SetUsesPerTurnRemaining(PlayableUnit.ContextualSkills[i].SkillDefinition.UsesPerTurnCount);
			}
		}
	}

	public void RefillContextualSkillsOverallUses()
	{
		foreach (TheLastStand.Model.Skill.Skill contextualSkill in PlayableUnit.ContextualSkills)
		{
			if (contextualSkill.OverallUsesRemaining != -1)
			{
				contextualSkill.OverallUsesRemaining = contextualSkill.ComputeTotalUses();
			}
		}
	}

	public void DecreaseActiveMomentumTiles(int amount)
	{
		PlayableUnit.MomentumTilesActive -= amount;
	}

	public void ResetPerksData(PerkDataContainer perkDataContainer = null)
	{
		foreach (KeyValuePair<string, TheLastStand.Model.Unit.Perk.Perk> perk in PlayableUnit.Perks)
		{
			perk.Value.TargetObject = perkDataContainer;
		}
	}

	public void SendEquipmentsToCityStash()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			foreach (EquipmentSlot item in equipmentSlot.Value)
			{
				if (item.Item != null)
				{
					TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.AddItem(new ItemController(item.Item).Item);
				}
			}
		}
	}

	public void StartEquipmentTurn()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			for (int num = equipmentSlot.Value.Count - 1; num >= 0; num--)
			{
				equipmentSlot.Value[num].Item?.ItemController.StartTurn();
			}
		}
	}

	public override void StartTurn()
	{
		if (base.Unit.IsDead)
		{
			return;
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.MovePoints, UnitStatDefinition.E_Stat.MovePointsTotal);
			PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.ActionPoints, UnitStatDefinition.E_Stat.ActionPointsTotal);
			StartEquipmentTurn();
			switch (TPSingleton<GameManager>.Instance.Game.Cycle)
			{
			case Game.E_Cycle.Day:
				if (TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Production)
				{
					break;
				}
				if (base.Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthRegen).FinalClamped > 0f)
				{
					float num2 = GainHealth(PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthRegen).FinalClamped, refreshHud: false);
					if (num2 > 0f)
					{
						HealFeedback healFeedback = base.Unit.DamageableView.HealFeedback;
						healFeedback.AddHealInstance(num2, base.Unit.Health);
						AddEffectDisplay(healFeedback);
					}
				}
				if (PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ManaRegen).FinalClamped > 0f)
				{
					float num3 = GainMana(PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ManaRegen).FinalClamped);
					if (num3 > 0f)
					{
						RestoreStatDisplay pooledComponent = ObjectPooler.GetPooledComponent("RestoreStatDisplay", ResourcePooler.LoadOnce<RestoreStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/RestoreStatDisplay"), EffectManager.EffectDisplaysParent);
						pooledComponent.Init(UnitStatDefinition.E_Stat.Mana, (int)num3);
						AddEffectDisplay(pooledComponent);
					}
				}
				RefillContextualSkillsOverallUses();
				RefillNativeSkillsOverallUses();
				RemoveAllStatuses();
				base.Unit.UnitView.RefreshInjuryStage();
				PlayableUnit.LevelUp.UnitLevelUpController.ResetSinkNbReroll();
				PlayableUnit.MovedThisDay = false;
				break;
			case Game.E_Cycle.Night:
			{
				int num = GetAdjacentTilesWithDiagonals().Count((Tile tile) => tile.Unit is EnemyUnit);
				TrophyManager.AppendValueToTrophiesConditions<HeroSurroundedByEnemiesTrophyConditionController>(new object[3] { PlayableUnit.RandomId, 1, num });
				TrophyManager.SetValueToTrophiesConditions<CriticalsInflictedSingleTurnTrophyConditionController>(new object[2] { PlayableUnit.RandomId, 0 });
				break;
			}
			}
			ResetContextualSkillsTurnUses();
			ResetNativeSkillsTurnUses();
			ResetCrossedTiles();
			ResetActiveMomentumTiles();
		}
		base.StartTurn();
		EffectManager.DisplayEffects();
	}

	public void SwitchWeaponSet()
	{
		if (!PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.RightHand, out var value) || value.Count > 1)
		{
			int equippedWeaponSetIndex = PlayableUnit.EquippedWeaponSetIndex;
			PlayableUnit.EquippedWeaponSetIndex = ((PlayableUnit.EquippedWeaponSetIndex == 0) ? 1 : 0);
			PlayableUnitManager.SelectedSkill = null;
			if (value != null)
			{
				EquipmentSlotView equipmentSlotView = CharacterSheetPanel.EquipmentSlots[ItemSlotDefinition.E_ItemSlotId.RightHand][0];
				value[PlayableUnit.EquippedWeaponSetIndex].EquipmentSlotView = equipmentSlotView;
				equipmentSlotView.EquipmentSlot = value[PlayableUnit.EquippedWeaponSetIndex];
				equipmentSlotView.Refresh();
				EquipmentSlotView equipmentSlotView2 = CharacterSheetPanel.EquipmentSlots[ItemSlotDefinition.E_ItemSlotId.RightHand][1];
				value[equippedWeaponSetIndex].EquipmentSlotView = equipmentSlotView2;
				equipmentSlotView2.EquipmentSlot = value[equippedWeaponSetIndex];
				equipmentSlotView2.Refresh();
				OverrideBodyParts(value[equippedWeaponSetIndex].Item?.ItemDefinition.BodyPartsDefinitions, clear: true);
				OverrideBodyParts(value[PlayableUnit.EquippedWeaponSetIndex].Item?.ItemDefinition.BodyPartsDefinitions);
			}
			if (PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.LeftHand, out var value2))
			{
				EquipmentSlotView equipmentSlotView3 = CharacterSheetPanel.EquipmentSlots[ItemSlotDefinition.E_ItemSlotId.LeftHand][0];
				value2[PlayableUnit.EquippedWeaponSetIndex].EquipmentSlotView = equipmentSlotView3;
				equipmentSlotView3.EquipmentSlot = value2[PlayableUnit.EquippedWeaponSetIndex];
				equipmentSlotView3.Refresh();
				EquipmentSlotView equipmentSlotView4 = CharacterSheetPanel.EquipmentSlots[ItemSlotDefinition.E_ItemSlotId.LeftHand][1];
				value2[equippedWeaponSetIndex].EquipmentSlotView = equipmentSlotView4;
				equipmentSlotView4.EquipmentSlot = value2[equippedWeaponSetIndex];
				equipmentSlotView4.Refresh();
				OverrideBodyParts(value2[equippedWeaponSetIndex].Item?.ItemDefinition.BodyPartsDefinitions, clear: true);
				OverrideBodyParts(value2[PlayableUnit.EquippedWeaponSetIndex].Item?.ItemDefinition.BodyPartsDefinitions);
			}
			for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
			{
				TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].UnitView.ToggleSkillTargeting(show: false);
			}
			for (int j = 0; j < TPSingleton<BuildingManager>.Instance.Buildings.Count; j++)
			{
				TPSingleton<BuildingManager>.Instance.Buildings[j].BuildingView.ToggleSkillTargeting(display: false);
			}
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill)
			{
				UnitManagementView<PlayableUnitManagementView>.Refresh();
			}
		}
	}

	protected override Vector2Int ReduceIncomingDamageWithBlock(Vector2Int incomingDamage, bool updateLifetimeStats, out int blockValue, out int blockedDamageValue)
	{
		incomingDamage = base.ReduceIncomingDamageWithBlock(incomingDamage, updateLifetimeStats, out blockValue, out blockedDamageValue);
		if (updateLifetimeStats)
		{
			PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesBlocked(blockedDamageValue);
		}
		return incomingDamage;
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		List<string> list = new List<string>();
		foreach (string key in PlayableUnit.Perks.Keys)
		{
			list.Add(key);
		}
		foreach (string item in list)
		{
			PlayableUnit.Perks[item].PerkController.LockAndClearUnlockers(removePerkFromOwner: true);
		}
		TrophyManager.AppendValueToTrophiesConditions<HeroDeadTrophyConditionController>(new object[1] { 1 });
		if (!TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.ContainsKey(TPSingleton<GameManager>.Instance.DayNumber))
		{
			TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.Add(TPSingleton<GameManager>.Instance.DayNumber, new List<PlayableUnit>());
		}
		TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber].Add(PlayableUnit);
		SendEquipmentsToCityStash();
		PlayableUnitManager.DestroyUnit(PlayableUnit);
	}

	private void AddGeneratedTrait(string traitId, ref int currentTraitPoints, bool forceAdd = false)
	{
		if (AddTrait(traitId, forceAdd))
		{
			currentTraitPoints -= PlayableUnitDatabase.UnitTraitDefinitions[traitId].Cost;
		}
	}

	private void AddSlots(List<UnitTraitDefinition.SlotModifier> addedSlots)
	{
		foreach (UnitTraitDefinition.SlotModifier addedSlot in addedSlots)
		{
			ItemSlotDefinition.E_ItemSlotId name = addedSlot.Name;
			for (int num = addedSlot.Amount - 1; num >= 0; num--)
			{
				List<EquipmentSlotView> list = CharacterSheetPanel.EquipmentSlots[name];
				if (!PlayableUnit.EquipmentSlots.TryGetValue(name, out var _))
				{
					TPSingleton<PlayableUnitManager>.Instance.LogError($"Slot {name} not found");
				}
				else
				{
					int count = PlayableUnit.EquipmentSlots[name].Count;
					if (PlayableUnit.EquipmentSlots.ContainsKey(name))
					{
						if (list.Count - 1 >= count)
						{
							PlayableUnit.EquipmentSlots[name].Add(new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[name], list[count], PlayableUnit).EquipmentSlot);
						}
					}
					else if (list.Count - 1 >= count)
					{
						PlayableUnit.EquipmentSlots.Add(name, new List<EquipmentSlot>());
						PlayableUnit.EquipmentSlots[name].Add(new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[name], list[count], PlayableUnit).EquipmentSlot);
					}
					if (count == 1 && name == ItemSlotDefinition.E_ItemSlotId.LeftHand)
					{
						PlayableUnit.BodyParts["Arm_L"].ChangeAdditionalConstraint("Hide", add: false);
					}
				}
			}
		}
	}

	private bool CanOnlyHaveTwoHandsWeapons(PlayableUnitGenerationDefinition unitGenerationDefinition, bool isStartingUnit)
	{
		List<string> list = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockEquipmentGenerationMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				list.Add(effects[i].Id);
			}
		}
		string[] lockedItemsIds = ItemManager.GetAllLockedItemsIds().ToArray();
		List<string> priorityItemIds = new List<string>();
		if (isStartingUnit)
		{
			List<string> list2 = ItemManager.GetAllItemsIds(TPSingleton<GlyphManager>.Instance.StartingGearGenerationPriorityItemLists).ToList();
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				string text = list2[num];
				if (!lockedItemsIds.Contains(text))
				{
					priorityItemIds.Add(text);
				}
			}
		}
		List<Tuple<int, EquipmentGenerationDefinition.ItemGenerationData>> itemsPerWeight = unitGenerationDefinition.EquipmentGenerationDefinitions[ItemSlotDefinition.E_ItemSlotId.RightHand].ItemsPerWeight;
		for (int num2 = itemsPerWeight.Count - 1; num2 >= 0; num2--)
		{
			string itemId = itemsPerWeight[num2].Item2.ItemId;
			string itemsList = itemsPerWeight[num2].Item2.ItemsList;
			if (string.IsNullOrEmpty(itemId) || (list.Contains(itemId) && !lockedItemsIds.Contains(itemId)))
			{
				if (!ItemDatabase.ItemsListDefinitions.ContainsKey(itemsList))
				{
					TPSingleton<PlayableUnitManager>.Instance.LogError("During hero generation, we failed to access the itemsList with Id " + itemsList + ". This id comes from the equipment generation definition, archetype : " + unitGenerationDefinition.ArchetypeId, CLogLevel.MAJOR);
				}
				else
				{
					ItemsListDefinition itemsListDefinition = ItemDatabase.ItemsListDefinitions[itemsList];
					bool arePriorityItemsAvailable = false;
					if (priorityItemIds.Count > 0)
					{
						foreach (string allItemsIn in ItemManager.GetAllItemsInList(itemsListDefinition))
						{
							if (!lockedItemsIds.Contains(allItemsIn) && priorityItemIds.Contains(allItemsIn))
							{
								arePriorityItemsAvailable = true;
								break;
							}
						}
					}
					if (ItemManager.AnyItemMatchingCondition(itemsListDefinition, (ItemDefinition item) => item.Hands == ItemDefinition.E_Hands.OneHand && !lockedItemsIds.Contains(item.Id) && (!arePriorityItemsAvailable || priorityItemIds.Contains(item.Id))))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private void ComputeExperienceNeededToNextLevel()
	{
		PlayableUnit.ExperienceNeededToNextLevel = PlayableUnitDatabase.ExperienceNeededToNextLevel.EvalToFloat(PlayableUnit);
	}

	private void Generate(int traitPoints, int level = 1, bool isStartingUnit = false)
	{
		string text = "--- PlayableUnit generation: " + PlayableUnit.Name + " ---";
		PlayableUnitGenerationDefinition playableUnitGenerationDefinition = PlayableUnitDatabase.PlayableUnitGenerationDefinitions[PlayableUnit.ArchetypeId];
		text += "\n---- Unit stats generation ----";
		base.Unit.UnitStatsController = new PlayableUnitStatsController(PlayableUnit);
		text += "\n---- End of unit stats generation ----";
		text += "\n---- Unit race stats modifiers ----";
		PlayableUnit.PlayableUnitStatsController.OnRaceGenerated(PlayableUnit.RaceDefinition);
		text += "\n---- End of unit race stats modifiers ----";
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, UnitEquipmentSlotDefinition> unitEquipmentSlotDefinition in PlayableUnitDatabase.UnitEquipmentSlotDefinitions)
		{
			int i = 0;
			for (int num = unitEquipmentSlotDefinition.Value.Base; i < num; i++)
			{
				if (!PlayableUnit.EquipmentSlots.ContainsKey(unitEquipmentSlotDefinition.Key))
				{
					PlayableUnit.EquipmentSlots.Add(unitEquipmentSlotDefinition.Key, new List<EquipmentSlot>());
				}
				PlayableUnit.EquipmentSlots[unitEquipmentSlotDefinition.Key].Add(new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[unitEquipmentSlotDefinition.Key], CharacterSheetPanel.EquipmentSlots[unitEquipmentSlotDefinition.Key][i], PlayableUnit).EquipmentSlot);
			}
		}
		text += "\n---- Unit traits generation ----";
		text += $"\nStarts with {traitPoints} trait points";
		bool flag = false;
		List<string> list = new List<string>(playableUnitGenerationDefinition.BackgroundTraitAvailableIds);
		bool flag2 = true;
		if (ApocalypseManager.CurrentApocalypse != null && ApocalypseManager.CurrentApocalypse.RemoveStartingPlayableUnitAmount > 0 && isStartingUnit)
		{
			flag2 = !TPSingleton<PlayableUnitManager>.Exist() || TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count != 0;
		}
		bool flag3 = CanOnlyHaveTwoHandsWeapons(playableUnitGenerationDefinition, isStartingUnit);
		if (flag3 || !flag2)
		{
			list.Remove("One-Armed");
		}
		string[] lockedTraitsIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedTraitsIds();
		foreach (string item in lockedTraitsIds)
		{
			if (list.Contains(item))
			{
				list.Remove(item);
			}
		}
		RemoveIncompatibleTraitsWithRace(list);
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			string backgroundTraitId = RandomManager.GetRandomElement(this, list);
			text += $"\nTrying to add background trait: {backgroundTraitId} which costs {PlayableUnitDatabase.UnitTraitDefinitions[backgroundTraitId].Cost} points";
			List<string> list2 = new List<string>(PlayableUnitDatabase.SecondaryTraitIds);
			lockedTraitsIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedTraitsIds();
			foreach (string item2 in lockedTraitsIds)
			{
				if (list2.Contains(item2))
				{
					list2.Remove(item2);
				}
			}
			if (flag3 || !flag2)
			{
				list2.Remove("One-Armed");
			}
			RemoveIncompatibleTraitsWithRace(list2);
			for (int num3 = list2.Count - 1; num3 >= 0; num3--)
			{
				string secondTraitId = RandomManager.GetRandomElement(this, list2);
				text += $"\nTrying to add second trait: {secondTraitId} which costs {PlayableUnitDatabase.UnitTraitDefinitions[secondTraitId].Cost} points";
				if (!PlayableUnitDatabase.UnitTraitDefinitions[backgroundTraitId].Incompatibilities.Contains(secondTraitId))
				{
					int remainingCost = traitPoints - PlayableUnitDatabase.UnitTraitDefinitions[backgroundTraitId].Cost - PlayableUnitDatabase.UnitTraitDefinitions[secondTraitId].Cost;
					if (!PlayableUnitDatabase.SecondaryTraitCost.Contains(remainingCost))
					{
						text = text + "\nFAILED --> Can't find any matching third trait for " + backgroundTraitId + " & " + secondTraitId + " because there are all in first traits Incompatibilities or the trait points will not be all spent";
					}
					else
					{
						List<string> list3 = list2.FindAll((string id) => id != secondTraitId && !PlayableUnitDatabase.UnitTraitDefinitions[id].Incompatibilities.Contains(backgroundTraitId) && !PlayableUnitDatabase.UnitTraitDefinitions[id].Incompatibilities.Contains(secondTraitId) && PlayableUnitDatabase.UnitTraitDefinitions[id].Cost == remainingCost);
						if (list3.Count != 0)
						{
							string randomElement = RandomManager.GetRandomElement(this, list3);
							AddGeneratedTrait(backgroundTraitId, ref traitPoints, forceAdd: true);
							AddGeneratedTrait(secondTraitId, ref traitPoints, forceAdd: true);
							AddGeneratedTrait(randomElement, ref traitPoints, forceAdd: true);
							text += $"\nSUCCESS --> Third trait picked: {randomElement} which costs {PlayableUnitDatabase.UnitTraitDefinitions[randomElement].Cost} points";
							flag = true;
							break;
						}
						text = text + "\nFAILED --> Can't find any matching third trait for " + backgroundTraitId + " & " + secondTraitId + " because there are all in first traits Incompatibilities or the trait points will not be all spent";
					}
				}
				else
				{
					text = text + "\nFAILED --> " + secondTraitId + " is in " + backgroundTraitId + " Incompatibilities";
				}
				list2.Remove(backgroundTraitId);
			}
			if (flag)
			{
				break;
			}
			list.Remove(backgroundTraitId);
		}
		text += "\n---- End of unit traits generation ----";
		GenerateEquipment(playableUnitGenerationDefinition, PlayableUnit);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Health, UnitStatDefinition.E_Stat.HealthTotal);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Armor, UnitStatDefinition.E_Stat.ArmorTotal);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Mana, UnitStatDefinition.E_Stat.ManaTotal);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.ActionPoints, UnitStatDefinition.E_Stat.ActionPointsTotal);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.MovePoints, UnitStatDefinition.E_Stat.MovePointsTotal);
		UpdateInjuryStage();
		if (PlayableUnit.EquippedWeaponSetIndex != 0)
		{
			SwitchWeaponSet();
		}
		while ((double)level > PlayableUnit.Level)
		{
			LevelUp();
			PlayableUnit.ExperienceInCurrentLevel = 0f;
		}
		text += "\n--- End of playableUnit generation ---\n";
		TPSingleton<PlayableUnitManager>.Instance.Log(text);
	}

	private void GenerateContextualSkills()
	{
		PlayableUnit.ContextualSkills = new List<TheLastStand.Model.Skill.Skill>();
		foreach (string contextualSkill in SkillDatabase.ContextualSkills)
		{
			if (!SkillDatabase.SkillDefinitions.TryGetValue(contextualSkill, out var value))
			{
				TPSingleton<PlayableUnitManager>.Instance.LogError("Skill " + contextualSkill + " not found!");
			}
			else if (!value.IsLockedByPerk && (!value.IsBrazierSpecific || TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.BrazierDefinition != null))
			{
				TheLastStand.Model.Skill.Skill skill = new SkillController(value, PlayableUnit, -1, value.UsesPerTurnCount).Skill;
				PlayableUnit.ContextualSkills.Add(skill);
			}
		}
	}

	private void GenerateNativePerks(List<SerializedPerk> serializedNativePerks = null, bool isDead = false)
	{
		if (serializedNativePerks != null)
		{
			foreach (SerializedPerk serializedNativePerk in serializedNativePerks)
			{
				if (PlayableUnitDatabase.PerkDefinitions.TryGetValue(serializedNativePerk.Id, out var value))
				{
					_ = new PerkController(serializedNativePerk, value, null, PlayableUnit, null, string.Empty, isDead, isNative: true, isFromRace: false).Perk;
				}
			}
			return;
		}
		foreach (PerkDefinition value2 in TPSingleton<GlyphManager>.Instance.NativePerksToUnlock.Values)
		{
			new PerkController(value2, null, PlayableUnit, null, string.Empty, isNative: true, isFromRace: false).Perk.PerkController.Unlock(PlayableUnit);
		}
		PlayableUnit.PerksPoints += TPSingleton<GlyphManager>.Instance.NativePerkPointsBonus;
	}

	private void GenerateRacePerks(List<SerializedPerk> serializedRacePerks = null, bool isDead = false)
	{
		if (serializedRacePerks != null)
		{
			foreach (SerializedPerk serializedRacePerk in serializedRacePerks)
			{
				if (PlayableUnit.RaceDefinition.PerksIds.Contains(serializedRacePerk.Id) && PlayableUnitDatabase.PerkDefinitions.TryGetValue(serializedRacePerk.Id, out var value))
				{
					_ = new PerkController(serializedRacePerk, value, null, PlayableUnit, null, string.Empty, isDead, isNative: false, isFromRace: true).Perk;
				}
			}
			GenerateRacePerks(null, isDead);
			return;
		}
		foreach (string perksId in PlayableUnit.RaceDefinition.PerksIds)
		{
			if (!PlayableUnit.Perks.Keys.Contains(perksId) && PlayableUnitDatabase.PerkDefinitions.TryGetValue(perksId, out var value2))
			{
				new PerkController(value2, null, PlayableUnit, null, string.Empty, isNative: false, isFromRace: true).Perk.PerkController.Unlock(PlayableUnit);
			}
		}
	}

	private void GenerateDynamicPerks(List<SerializedPerk> serializedDynamicPerks = null, bool isDead = false)
	{
		if (serializedDynamicPerks == null)
		{
			return;
		}
		foreach (SerializedPerk serializedDynamicPerk in serializedDynamicPerks)
		{
			if (PlayableUnitDatabase.PerkDefinitions.TryGetValue(serializedDynamicPerk.Id, out var value))
			{
				_ = new PerkController(serializedDynamicPerk, value, null, PlayableUnit, null, string.Empty, isDead, isNative: false, isFromRace: false).Perk;
			}
		}
	}

	private void GenerateNativeSkills(bool onLoad)
	{
		if (SkillDatabase.SkillDefinitions.TryGetValue("Punch", out var value))
		{
			PlayableUnit.NativeSkills.Add(new SkillController(value, PlayableUnit).Skill);
		}
	}

	private void GenerateRace(string raceDefinitionId = null)
	{
		RaceDefinition raceDefinition = null;
		if (!string.IsNullOrEmpty(raceDefinitionId) && PlayableUnitDatabase.RaceDefinitions.TryGetValue(raceDefinitionId, out var value))
		{
			raceDefinition = value;
		}
		if (raceDefinition == null)
		{
			List<string> availableRacesIds = PlayableUnitManager.GetAvailableRacesIds();
			int randomRange = RandomManager.GetRandomRange(this, 0, availableRacesIds.Count);
			raceDefinition = PlayableUnitDatabase.RaceDefinitions[availableRacesIds[randomRange]];
		}
		PlayableUnit.RaceDefinition = raceDefinition;
	}

	private EquipmentSlot GetBestItemSlot(TheLastStand.Model.Item.Item item)
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot2 in PlayableUnit.EquipmentSlots)
		{
			EquipmentSlot equipmentSlot = null;
			for (int i = 0; i < equipmentSlot2.Value.Count; i++)
			{
				if (!equipmentSlot2.Value[i].EquipmentSlotController.IsItemCompatible(item))
				{
					continue;
				}
				if ((ItemSlotDefinition.E_ItemSlotId.Trinket | ItemSlotDefinition.E_ItemSlotId.Usables).HasFlag(equipmentSlot2.Value[i].ItemSlotDefinition.Id) && equipmentSlot2.Value[i].Item != null)
				{
					if (equipmentSlot == null)
					{
						equipmentSlot = equipmentSlot2.Value[i];
					}
				}
				else if (!ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot2.Value[i].ItemSlotDefinition.Id) || (PlayableUnit.EquippedWeaponSetIndex == i && (equipmentSlot2.Value[i].ItemSlotDefinition.Id != ItemSlotDefinition.E_ItemSlotId.LeftHand || equipmentSlot2.Value[i].BlockedByOtherSlot == null) && (equipmentSlot2.Value[i].ItemSlotDefinition.Id != ItemSlotDefinition.E_ItemSlotId.RightHand || !item.IsTwoHandedWeapon || equipmentSlot2.Value[i].EquipmentSlotController.CanEquipTwoHandedWeapon(item))))
				{
					return equipmentSlot2.Value[i];
				}
			}
			if (equipmentSlot != null)
			{
				return equipmentSlot;
			}
		}
		return null;
	}

	private string GetRandomFaceId(string gender, string raceId)
	{
		UnitFaceIdDefinitions unitFaceIdDefinitions = ((gender == "Female") ? PlayableUnitDatabase.PlayableFemaleUnitFaceIds : PlayableUnitDatabase.PlayableMaleUnitFaceIds);
		string result = string.Empty;
		int num = 0;
		List<UnitFaceIdDefinition> list = unitFaceIdDefinitions.FindAll((UnitFaceIdDefinition unitFaceDefinition) => unitFaceDefinition.RestrictedToRaceId == raceId).ToList();
		if (list.Count == 0)
		{
			list = unitFaceIdDefinitions.FindAll((UnitFaceIdDefinition unitFaceDefinition) => unitFaceDefinition.RestrictedToRaceId == "Human").ToList();
		}
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			num += list[num2].Weight;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, num);
		int num3 = 0;
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			if (randomRange >= num3 && randomRange < list[num4].Weight + num3)
			{
				result = list[num4].FaceId;
				break;
			}
			num3 += list[num4].Weight;
		}
		return result;
	}

	private void RefillNativeSkillsOverallUses()
	{
		foreach (TheLastStand.Model.Skill.Skill nativeSkill in PlayableUnit.NativeSkills)
		{
			if (nativeSkill.OverallUsesRemaining != -1)
			{
				nativeSkill.OverallUsesRemaining = nativeSkill.ComputeTotalUses();
			}
		}
	}

	private void ResetNativeSkillsTurnUses()
	{
		foreach (TheLastStand.Model.Skill.Skill nativeSkill in PlayableUnit.NativeSkills)
		{
			if (nativeSkill.SkillDefinition.UsesPerTurnCount != -1)
			{
				nativeSkill.SetUsesPerTurnRemaining(nativeSkill.SkillDefinition.UsesPerTurnCount);
			}
		}
	}

	private void ResetCrossedTiles()
	{
		PlayableUnit.TilesCrossedThisTurn = 0;
		PlayableUnit.TotalMomentumTilesCrossedThisTurn = 0;
	}

	private void ResetActiveMomentumTiles()
	{
		PlayableUnit.MomentumTilesActive = 0;
	}

	private void RemoveSlots(List<UnitTraitDefinition.SlotModifier> removedSlots)
	{
		foreach (UnitTraitDefinition.SlotModifier removedSlot in removedSlots)
		{
			ItemSlotDefinition.E_ItemSlotId name = removedSlot.Name;
			for (int num = removedSlot.Amount - 1; num >= 0; num--)
			{
				if (PlayableUnit.EquipmentSlots.ContainsKey(name))
				{
					PlayableUnit.EquipmentSlots[name].RemoveAt(PlayableUnit.EquipmentSlots[name].Count - 1);
					if (PlayableUnit.EquipmentSlots[name].Count == 0)
					{
						PlayableUnit.EquipmentSlots.Remove(name);
						if (name == ItemSlotDefinition.E_ItemSlotId.LeftHand)
						{
							PlayableUnit.BodyParts["Arm_L"].ChangeAdditionalConstraint("Hide", add: true);
						}
					}
				}
			}
		}
	}

	private void RemoveIncompatibleTraitsWithRace(List<string> traitsIds)
	{
		if (PlayableUnit.RaceDefinition == null)
		{
			return;
		}
		for (int num = traitsIds.Count - 1; num >= 0; num--)
		{
			string key = traitsIds[num];
			if (PlayableUnitDatabase.UnitTraitDefinitions.TryGetValue(key, out var value) && value.RaceIncompatibilities.Contains(PlayableUnit.RaceDefinition.Id))
			{
				traitsIds.Remove(traitsIds[num]);
			}
		}
	}

	public void DebugGenerateRacePerks()
	{
		GenerateRacePerks();
	}

	public void DebugSetColorPalette(ColorSwapPaletteDefinition paletteDefinition, bool isHairPalette)
	{
	}

	public void DebugSetFaceId(string faceId)
	{
		if (!string.IsNullOrEmpty(faceId))
		{
			PlayableUnit.FaceId = faceId;
			PlayableUnit.PlayableUnitView.RefreshBodyParts(forceFullRefresh: true);
			DebugSetPortraitSprite();
		}
	}

	public void DebugSetPortraitSprite(string portraitId = null)
	{
		PlayableUnitView.RemoveUsedPortrait(PlayableUnit.PortraitSprite);
		if (portraitId == null)
		{
			PlayableUnitView.GenerateRandomPortrait(PlayableUnit);
		}
		else
		{
			Sprite sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Units/Portaits/Playable Unit/Foreground/" + PlayableUnit.Gender + "/" + PlayableUnit.FaceId + "/" + portraitId);
			Sprite portraitBackgroundSprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Units/Portaits/Playable Unit/Background/" + PlayableUnit.Gender + "/" + PlayableUnit.FaceId + "/" + portraitId);
			PlayableUnit.PortraitSprite = sprite;
			PlayableUnit.PortraitBackgroundSprite = portraitBackgroundSprite;
			PlayableUnitView.AddUsedPortrait(sprite);
		}
		PlayableUnitManagementView.UnitPortraitView.RefreshPortrait();
		GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraits();
	}
}

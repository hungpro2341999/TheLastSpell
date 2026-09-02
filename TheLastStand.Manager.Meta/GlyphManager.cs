using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Meta;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Meta.Glyphs.GlyphEffects;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Meta;
using TheLastStand.View.WorldMap.Glyphs;
using UnityEngine;

namespace TheLastStand.Manager.Meta;

public class GlyphManager : Manager<GlyphManager>
{
	[Flags]
	public enum E_SkillProgressionFlag
	{
		None = 0,
		SpecializedTraps = 1,
		SpecializedFiringDefenses = 2,
		ExplosiveBonePiles = 4
	}

	private class StringToGlyphIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(GlyphDatabase.GlyphDefinitions.Keys);
	}

	[SerializeField]
	private int maxGlyphsDisplayed;

	[SerializeField]
	private List<Sprite> lockedIcons = new List<Sprite>();

	public GlyphTooltip GlyphTooltip;

	private Dictionary<string, List<BuildingPassiveDefinition>> additionalBuildingPassives_cache = new Dictionary<string, List<BuildingPassiveDefinition>>();

	private bool Debug_glyphsForceUnlock;

	public List<GlyphAddBuildingPassiveEffectDefinition> AdditionalBuildingPassives { get; } = new List<GlyphAddBuildingPassiveEffectDefinition>();

	public int AdditionRewardReroll { get; private set; }

	public List<GlyphModifyBuildingActionsCostEffect> BuildingActionsCostModifiers { get; private set; }

	public Dictionary<string, int> BuildingHealthModifiers { get; private set; }

	public Dictionary<string, int> BuildLimitModifiers { get; private set; }

	public int BonusLevelupRerolls { get; private set; }

	public int BonusSellingRatio { get; private set; }

	public Dictionary<ResourceManager.E_PriceModifierType, int> CostsModifiers { get; private set; }

	public uint DamnedSoulsPercentageModifier { get; private set; }

	public int DamnedSoulsScavengingPercentageModifier { get; private set; }

	public int DefensesDamagePercentageModifier { get; private set; }

	public int FreeTrapUsageChances { get; private set; }

	public int GoldScavengingPercentageModifier { get; private set; }

	public Dictionary<string, GlyphModifyItemRarityEffectDefinition.ItemRarityModifier> ItemByTagRarityModifier { get; private set; }

	public Dictionary<string, Dictionary<string, float>> ItemWeightMultipliers { get; private set; }

	public Dictionary<string, Dictionary<int, int>> LevelProbabilityTreeModifiers { get; private set; }

	public int MaterialScavengingPercentageModifier { get; private set; }

	public int MaxGlyphsDisplayed => maxGlyphsDisplayed;

	public int NativePerkPointsBonus { get; private set; }

	public Dictionary<string, PerkDefinition> NativePerksToUnlock { get; private set; }

	public HashSet<string> NewlyUnlockedGlyphIds { get; } = new HashSet<string>();

	public int NightRewardsCountModifier { get; private set; }

	public Dictionary<UnitStatDefinition.E_Stat, float> PlayableUnitsStatsToModify { get; private set; }

	public int ProdRewardsCountModifier { get; private set; }

	public Dictionary<string, Dictionary<int, int>> RarityProbabilityTreeModifiers { get; private set; }

	public E_SkillProgressionFlag SkillProgressionFlag { get; private set; }

	public List<string> StartingGearGenerationPriorityItemLists { get; private set; }

	public Dictionary<string, Dictionary<int, int>> StartingGearLevelModifiers { get; private set; }

	public Dictionary<string, Dictionary<string, int>> MaxApoPassedByCityByGlyph { get; private set; }

	[DevConsoleCommand(Name = "GlyphsForceUnlock", Options = (DevConsoleCommandOptions.CanReadWrite | DevConsoleCommandOptions.ForceStatic))]
	public static bool Debug_GlyphsForceUnlock
	{
		get
		{
			return TPSingleton<GlyphManager>.Instance.Debug_glyphsForceUnlock;
		}
		private set
		{
			TPSingleton<GlyphManager>.Instance.Debug_glyphsForceUnlock = value;
			if (TPSingleton<GlyphSelectionPanel>.Exist())
			{
				TPSingleton<GlyphSelectionPanel>.Instance.RefreshAllGlyphs();
			}
		}
	}

	public static bool TryGetGlyphEffects<TGlyphEffect>(out List<TGlyphEffect> glyphEffects) where TGlyphEffect : GlyphEffectDefinition
	{
		glyphEffects = new List<TGlyphEffect>();
		foreach (GlyphDefinition selectedGlyph in TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs)
		{
			foreach (GlyphEffectDefinition glyphEffectDefinition in selectedGlyph.GlyphEffectDefinitions)
			{
				if (glyphEffectDefinition.GetType() == typeof(TGlyphEffect))
				{
					glyphEffects.Add(glyphEffectDefinition as TGlyphEffect);
				}
			}
		}
		return glyphEffects.Count > 0;
	}

	public static bool TryGetGlyphEffects<TGlyphEffect>(out Dictionary<string, List<TGlyphEffect>> glyphEffects) where TGlyphEffect : GlyphEffectDefinition
	{
		glyphEffects = new Dictionary<string, List<TGlyphEffect>>();
		foreach (GlyphDefinition selectedGlyph in TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs)
		{
			foreach (GlyphEffectDefinition glyphEffectDefinition in selectedGlyph.GlyphEffectDefinitions)
			{
				if (glyphEffectDefinition.GetType() == typeof(TGlyphEffect))
				{
					glyphEffects.AddAtKey(selectedGlyph.Id, glyphEffectDefinition as TGlyphEffect);
				}
			}
		}
		return glyphEffects.Count > 0;
	}

	public static void ResetSelectedGlyphs()
	{
		foreach (WorldMapCity city in TPSingleton<WorldMapCityManager>.Instance.Cities)
		{
			city.GlyphsConfig.SelectedGlyphs.Clear();
			city.CurrentGlyphPoints = 0;
		}
	}

	public List<BuildingPassiveDefinition> GetModifiedBuildingPassives(string buildingId, List<BuildingPassiveDefinition> nativePassives)
	{
		if (additionalBuildingPassives_cache.ContainsKey(buildingId))
		{
			return additionalBuildingPassives_cache[buildingId];
		}
		List<BuildingPassiveDefinition> list = new List<BuildingPassiveDefinition>();
		for (int i = 0; i < AdditionalBuildingPassives.Count; i++)
		{
			if (AdditionalBuildingPassives[i].BuildingIds.Contains(buildingId))
			{
				list.Add(AdditionalBuildingPassives[i].BuildingPassiveDefinition);
			}
		}
		list.AddRange(nativePassives);
		additionalBuildingPassives_cache[buildingId] = list;
		return list;
	}

	public int GetModifiedWorkersCost(BuildingActionDefinition buildingActionDefinition, bool updateDailyLimit = false)
	{
		int num = buildingActionDefinition.WorkersCost;
		foreach (GlyphModifyBuildingActionsCostEffect buildingActionsCostModifier in BuildingActionsCostModifiers)
		{
			if (buildingActionsCostModifier.GlyphModifyBuildingActionsCostEffectDefinition.BuildingActionCostModifiers.TryGetValue(buildingActionDefinition.Id, out var value) && buildingActionsCostModifier.CanUseModifiers())
			{
				if (updateDailyLimit)
				{
					buildingActionsCostModifier.UseModifiers();
				}
				num += value;
			}
		}
		return num;
	}

	public Sprite GetRandomLockedIcon()
	{
		return lockedIcons[UnityEngine.Random.Range(0, lockedIcons.Count)];
	}

	public void HandleGameOver(Game.E_GameOverCause cause)
	{
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		if (cause != Game.E_GameOverCause.MagicSealsCompleted || AchievementManager.IsAnyBoundlessModeActive || selectedCity.GlyphsConfig.SelectedGlyphs.Count == 0)
		{
			return;
		}
		foreach (GlyphDefinition selectedGlyph in selectedCity.GlyphsConfig.SelectedGlyphs)
		{
			if (!MaxApoPassedByCityByGlyph.ContainsKey(selectedGlyph.Id))
			{
				MaxApoPassedByCityByGlyph.Add(selectedGlyph.Id, new Dictionary<string, int>());
			}
			if (MaxApoPassedByCityByGlyph[selectedGlyph.Id].ContainsKey(selectedCity.CityDefinition.Id))
			{
				if (MaxApoPassedByCityByGlyph[selectedGlyph.Id][selectedCity.CityDefinition.Id] < ApocalypseManager.CurrentApocalypseLevel)
				{
					MaxApoPassedByCityByGlyph[selectedGlyph.Id][selectedCity.CityDefinition.Id] = ApocalypseManager.CurrentApocalypseLevel;
				}
			}
			else
			{
				MaxApoPassedByCityByGlyph[selectedGlyph.Id].Add(selectedCity.CityDefinition.Id, ApocalypseManager.CurrentApocalypseLevel);
			}
		}
		int achievementProgression = TPSingleton<AchievementManager>.Instance.GetAchievementProgression(StatContainer.STAT_COMPLETED_GLYPHS_AMOUNT);
		TPSingleton<AchievementManager>.Instance.SetAchievementProgression(StatContainer.STAT_COMPLETED_GLYPHS_AMOUNT, MaxApoPassedByCityByGlyph.Count, refreshAchievements: true, achievementProgression);
	}

	public void InitGlyphEffects()
	{
		AdditionalBuildingPassives.Clear();
		additionalBuildingPassives_cache.Clear();
		if (TryGetGlyphEffects(out List<GlyphAddBuildingPassiveEffectDefinition> glyphEffects))
		{
			AdditionalBuildingPassives.AddRange(glyphEffects);
		}
		AdditionRewardReroll = 0;
		if (TryGetGlyphEffects(out List<GlyphAdditionalRewardRerollEffectDefinition> glyphEffects2))
		{
			foreach (GlyphAdditionalRewardRerollEffectDefinition item in glyphEffects2)
			{
				AdditionRewardReroll += item.Value;
			}
		}
		BonusLevelupRerolls = 0;
		if (TryGetGlyphEffects(out List<GlyphIncreaseLevelupRerollsEffectDefinition> glyphEffects3))
		{
			foreach (GlyphIncreaseLevelupRerollsEffectDefinition item2 in glyphEffects3)
			{
				BonusLevelupRerolls += item2.RerollsBonusExpression.EvalToInt();
			}
		}
		BonusSellingRatio = 0;
		if (TryGetGlyphEffects(out List<GlyphBonusSellingRatioEffectDefinition> glyphEffects4))
		{
			foreach (GlyphBonusSellingRatioEffectDefinition item3 in glyphEffects4)
			{
				BonusSellingRatio += item3.Value;
			}
		}
		BuildingActionsCostModifiers = new List<GlyphModifyBuildingActionsCostEffect>();
		if (TryGetGlyphEffects(out Dictionary<string, List<GlyphModifyBuildingActionsCostEffectDefinition>> glyphEffects5))
		{
			foreach (KeyValuePair<string, List<GlyphModifyBuildingActionsCostEffectDefinition>> item4 in glyphEffects5)
			{
				for (int i = 0; i < item4.Value.Count; i++)
				{
					BuildingActionsCostModifiers.Add(new GlyphModifyBuildingActionsCostEffect(item4.Value[i], item4.Key, i));
				}
			}
		}
		BuildingHealthModifiers = new Dictionary<string, int>();
		if (TryGetGlyphEffects(out List<GlyphIncreaseBuildingHealthEffectDefinition> glyphEffects6))
		{
			foreach (GlyphIncreaseBuildingHealthEffectDefinition item5 in glyphEffects6)
			{
				BuildingHealthModifiers.AddValueOrCreateKey(item5.IdList, item5.Value, (int a, int b) => a + b);
			}
		}
		BuildLimitModifiers = new Dictionary<string, int>();
		if (TryGetGlyphEffects(out List<GlyphModifyBuildLimitEffectDefinition> glyphEffects7))
		{
			foreach (GlyphModifyBuildLimitEffectDefinition item6 in glyphEffects7)
			{
				BuildLimitModifiers.AddValueOrCreateKey(item6.BuildLimitId, item6.Value, (int a, int b) => a + b);
			}
		}
		CostsModifiers = new Dictionary<ResourceManager.E_PriceModifierType, int>();
		if (TryGetGlyphEffects(out List<GlyphModifyCostsEffectDefinition> glyphEffects8))
		{
			foreach (GlyphModifyCostsEffectDefinition item7 in glyphEffects8)
			{
				foreach (ResourceManager.E_PriceModifierType value in Enum.GetValues(typeof(ResourceManager.E_PriceModifierType)))
				{
					if (item7.Type.HasFlag(value))
					{
						CostsModifiers.AddValueOrCreateKey(value, item7.Value, (int a, int b) => a + b);
					}
				}
			}
		}
		DamnedSoulsPercentageModifier = 0u;
		if (TryGetGlyphEffects(out List<GlyphDamnedSoulsPercentageModifierEffectDefinition> glyphEffects9))
		{
			foreach (GlyphDamnedSoulsPercentageModifierEffectDefinition item8 in glyphEffects9)
			{
				DamnedSoulsPercentageModifier = (uint)Mathf.Clamp(DamnedSoulsPercentageModifier + item8.Value, 0f, 4.2949673E+09f);
			}
		}
		DamnedSoulsScavengingPercentageModifier = 0;
		if (TryGetGlyphEffects(out List<GlyphDamnedSoulsScavengingPercentageModifierEffectDefinition> glyphEffects10))
		{
			foreach (GlyphDamnedSoulsScavengingPercentageModifierEffectDefinition item9 in glyphEffects10)
			{
				DamnedSoulsScavengingPercentageModifier = Mathf.Clamp(DamnedSoulsScavengingPercentageModifier + item9.Value, 0, int.MaxValue);
			}
		}
		DefensesDamagePercentageModifier = 0;
		if (TryGetGlyphEffects(out List<GlyphIncreaseDefensesDamagesEffectDefinition> glyphEffects11))
		{
			foreach (GlyphIncreaseDefensesDamagesEffectDefinition item10 in glyphEffects11)
			{
				DefensesDamagePercentageModifier += item10.Value;
			}
		}
		FreeTrapUsageChances = 0;
		if (TryGetGlyphEffects(out List<GlyphFreeTrapUsageChancesEffectDefinition> glyphEffects12))
		{
			foreach (GlyphFreeTrapUsageChancesEffectDefinition item11 in glyphEffects12)
			{
				FreeTrapUsageChances = Mathf.Clamp(FreeTrapUsageChances + item11.Value, 0, 100);
			}
		}
		GoldScavengingPercentageModifier = 0;
		if (TryGetGlyphEffects(out List<GlyphGoldScavengingPercentageModifierEffectDefinition> glyphEffects13))
		{
			foreach (GlyphGoldScavengingPercentageModifierEffectDefinition item12 in glyphEffects13)
			{
				GoldScavengingPercentageModifier = Mathf.Clamp(GoldScavengingPercentageModifier + item12.Value, 0, int.MaxValue);
			}
		}
		ItemByTagRarityModifier = new Dictionary<string, GlyphModifyItemRarityEffectDefinition.ItemRarityModifier>();
		if (TryGetGlyphEffects(out List<GlyphModifyItemRarityEffectDefinition> glyphEffects14))
		{
			foreach (GlyphModifyItemRarityEffectDefinition item13 in glyphEffects14)
			{
				if (!ItemByTagRarityModifier.ContainsKey(item13.ItemTag))
				{
					ItemByTagRarityModifier.Add(item13.ItemTag, new GlyphModifyItemRarityEffectDefinition.ItemRarityModifier
					{
						MinRarityIndex = item13.MinRarityIndex
					});
				}
				else if (ItemByTagRarityModifier[item13.ItemTag].MinRarityIndex < item13.MinRarityIndex)
				{
					ItemByTagRarityModifier[item13.ItemTag].MinRarityIndex = item13.MinRarityIndex;
				}
			}
		}
		ItemWeightMultipliers = new Dictionary<string, Dictionary<string, float>>();
		Dictionary<string, Dictionary<string, float>> dictionary = new Dictionary<string, Dictionary<string, float>>();
		if (TryGetGlyphEffects(out List<GlyphMultiplyItemWeightEffectDefinition> glyphEffects15))
		{
			foreach (GlyphMultiplyItemWeightEffectDefinition item14 in glyphEffects15)
			{
				if (item14.IsCumulative)
				{
					AddItemWeightMultiplier(item14.ItemListId, item14.ItemId, item14.WeightMultiplier);
					continue;
				}
				if (!dictionary.ContainsKey(item14.ItemListId))
				{
					dictionary[item14.ItemListId] = new Dictionary<string, float>();
				}
				if (!dictionary[item14.ItemListId].ContainsKey(item14.ItemId) || dictionary[item14.ItemListId][item14.ItemId] < item14.WeightMultiplier)
				{
					dictionary[item14.ItemListId][item14.ItemId] = item14.WeightMultiplier;
				}
			}
			foreach (KeyValuePair<string, Dictionary<string, float>> item15 in dictionary)
			{
				foreach (KeyValuePair<string, float> item16 in item15.Value)
				{
					AddItemWeightMultiplier(item15.Key, item16.Key, item16.Value);
				}
			}
		}
		LevelProbabilityTreeModifiers = new Dictionary<string, Dictionary<int, int>>();
		if (TryGetGlyphEffects(out List<GlyphModifyLevelProbabilityTreeEffectDefinition> glyphEffects16))
		{
			foreach (GlyphModifyLevelProbabilityTreeEffectDefinition item17 in glyphEffects16)
			{
				if (!LevelProbabilityTreeModifiers.ContainsKey(item17.TreeId))
				{
					LevelProbabilityTreeModifiers[item17.TreeId] = new Dictionary<int, int>();
				}
				LevelProbabilityTreeModifiers[item17.TreeId] = LevelProbabilityTreeModifiers[item17.TreeId].Add(item17.ProbabilityModifiers);
			}
		}
		MaterialScavengingPercentageModifier = 0;
		if (TryGetGlyphEffects(out List<GlyphMaterialScavengingPercentageModifierEffectDefinition> glyphEffects17))
		{
			foreach (GlyphMaterialScavengingPercentageModifierEffectDefinition item18 in glyphEffects17)
			{
				MaterialScavengingPercentageModifier = Mathf.Clamp(MaterialScavengingPercentageModifier + item18.Value, 0, int.MaxValue);
			}
		}
		NativePerkPointsBonus = 0;
		if (TryGetGlyphEffects(out List<GlyphNativePerkPointsBonusEffectDefinition> glyphEffects18))
		{
			foreach (GlyphNativePerkPointsBonusEffectDefinition item19 in glyphEffects18)
			{
				NativePerkPointsBonus += item19.Value;
			}
		}
		NativePerksToUnlock = new Dictionary<string, PerkDefinition>();
		if (TryGetGlyphEffects(out List<GlyphNativePerkEffectDefinition> glyphEffects19))
		{
			foreach (GlyphNativePerkEffectDefinition item20 in glyphEffects19)
			{
				NativePerksToUnlock.Add(item20.PerkDefinition.Id, item20.PerkDefinition);
			}
		}
		NightRewardsCountModifier = 0;
		ProdRewardsCountModifier = 0;
		if (TryGetGlyphEffects(out List<GlyphModifyRewardsCountEffectDefinition> glyphEffects20))
		{
			foreach (GlyphModifyRewardsCountEffectDefinition item21 in glyphEffects20)
			{
				NightRewardsCountModifier += item21.NightRewardsModifier;
				ProdRewardsCountModifier += item21.ProdRewardsModifier;
			}
		}
		PlayableUnitsStatsToModify = new Dictionary<UnitStatDefinition.E_Stat, float>();
		if (TryGetGlyphEffects(out List<GlyphModifyPlayableUnitsStatsEffectDefinition> glyphEffects21))
		{
			foreach (GlyphModifyPlayableUnitsStatsEffectDefinition item22 in glyphEffects21)
			{
				foreach (KeyValuePair<UnitStatDefinition.E_Stat, Node> item23 in item22.StatsToModify)
				{
					PlayableUnitsStatsToModify[item23.Key] = PlayableUnitsStatsToModify.GetValueOrDefault(item23.Key) + item23.Value.EvalToFloat();
				}
			}
		}
		RarityProbabilityTreeModifiers = new Dictionary<string, Dictionary<int, int>>();
		if (TryGetGlyphEffects(out List<GlyphModifyRarityProbabilityTreeEffectDefinition> glyphEffects22))
		{
			foreach (GlyphModifyRarityProbabilityTreeEffectDefinition item24 in glyphEffects22)
			{
				if (!RarityProbabilityTreeModifiers.ContainsKey(item24.TreeId))
				{
					RarityProbabilityTreeModifiers[item24.TreeId] = new Dictionary<int, int>();
				}
				RarityProbabilityTreeModifiers[item24.TreeId] = RarityProbabilityTreeModifiers[item24.TreeId].Add(item24.ProbabilityModifiers);
			}
		}
		SkillProgressionFlag = E_SkillProgressionFlag.None;
		if (TryGetGlyphEffects(out List<GlyphToggleSkillProgressionFlagEffectDefinition> glyphEffects23))
		{
			foreach (GlyphToggleSkillProgressionFlagEffectDefinition item25 in glyphEffects23)
			{
				SkillProgressionFlag |= item25.SkillProgressionFlag;
			}
		}
		StartingGearLevelModifiers = new Dictionary<string, Dictionary<int, int>>();
		if (TryGetGlyphEffects(out List<GlyphIncreaseStartingGearLevelEffectDefinition> glyphEffects24))
		{
			foreach (GlyphIncreaseStartingGearLevelEffectDefinition item26 in glyphEffects24)
			{
				Dictionary<int, int> valueOrDefault = StartingGearLevelModifiers.GetValueOrDefault(item26.LevelTreeId);
				if (valueOrDefault != null)
				{
					foreach (KeyValuePair<int, int> item27 in item26.WeightBonusByLevelProbability)
					{
						valueOrDefault.AddValueOrCreateKey(item27.Key, item27.Value, (int a, int b) => a + b);
					}
				}
				else
				{
					StartingGearLevelModifiers[item26.LevelTreeId] = item26.WeightBonusByLevelProbability;
				}
			}
		}
		StartingGearGenerationPriorityItemLists = new List<string>();
		if (!TryGetGlyphEffects(out List<GlyphAddStartingGearGenerationPriorityEffectDefinition> glyphEffects25))
		{
			return;
		}
		foreach (GlyphAddStartingGearGenerationPriorityEffectDefinition item28 in glyphEffects25)
		{
			StartingGearGenerationPriorityItemLists.Add(item28.ItemListId);
		}
	}

	public void StartTurn()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			ResetBuildingActionsCostModifiersLimit();
		}
	}

	public void ToggleCustomMode(bool toggle)
	{
		TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled = toggle;
		if (!toggle)
		{
			RemoveOverflowingGlyphs(TPSingleton<WorldMapCityManager>.Instance.SelectedCity);
		}
	}

	public void ToggleGlyphSelection(GlyphDefinition glyph, bool select)
	{
		if (select)
		{
			TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Add(glyph);
		}
		else
		{
			TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Remove(glyph);
		}
	}

	public void RemoveOverflowingGlyphs()
	{
		foreach (WorldMapCity city in TPSingleton<WorldMapCityManager>.Instance.Cities)
		{
			RemoveMetaLockedGlyphs(city);
			if (!city.GlyphsConfig.CustomModeEnabled)
			{
				RemoveOverflowingGlyphs(city);
			}
		}
	}

	public void RemoveOverflowingGlyphs(WorldMapCity city)
	{
		while (city.GlyphsConfig.SelectedGlyphs.Count > 0 && city.CurrentGlyphPoints > city.CityDefinition.MaxGlyphPoints)
		{
			city.WorldMapCityController.RemoveGlyphAt(city.GlyphsConfig.SelectedGlyphs.Count - 1);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		MetaUpgradesManager.MetaUpgradeActivated += OnMetaUpgradeActivated;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MetaUpgradesManager.MetaUpgradeActivated -= OnMetaUpgradeActivated;
	}

	private void AddItemWeightMultiplier(string itemListId, string itemId, float weightMultiplier)
	{
		if (!ItemWeightMultipliers.ContainsKey(itemListId))
		{
			ItemWeightMultipliers[itemListId] = new Dictionary<string, float>();
		}
		if (!ItemWeightMultipliers[itemListId].ContainsKey(itemId))
		{
			ItemWeightMultipliers[itemListId][itemId] = weightMultiplier;
		}
		else
		{
			ItemWeightMultipliers[itemListId][itemId] += weightMultiplier;
		}
	}

	private void OnMetaUpgradeActivated(MetaUpgrade metaUpgrade)
	{
		RegisterNewlyUnlockedGlyph(metaUpgrade);
	}

	private void RegisterNewlyUnlockedGlyph(MetaUpgrade metaUpgrade)
	{
		foreach (MetaEffectDefinition upgradeEffectDefinition in metaUpgrade.MetaUpgradeDefinition.UpgradeEffectDefinitions)
		{
			if (upgradeEffectDefinition is UnlockGlyphsMetaEffectDefinition unlockGlyphsMetaEffectDefinition)
			{
				NewlyUnlockedGlyphIds.AddRange(unlockGlyphsMetaEffectDefinition.GlyphIds);
			}
		}
	}

	private void RemoveMetaLockedGlyphs(WorldMapCity city)
	{
		string[] lockedGlyphIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedGlyphIds();
		for (int num = city.GlyphsConfig.SelectedGlyphs.Count - 1; num >= 0; num--)
		{
			if (lockedGlyphIds.Contains(city.GlyphsConfig.SelectedGlyphs[num].Id))
			{
				city.WorldMapCityController.RemoveGlyphAt(num);
			}
		}
	}

	private void ResetBuildingActionsCostModifiersLimit()
	{
		foreach (GlyphModifyBuildingActionsCostEffect buildingActionsCostModifier in BuildingActionsCostModifiers)
		{
			buildingActionsCostModifier.ResetCurrentModifiersDailyLimit();
		}
	}

	public void Deserialize(SerializedApplicationState container = null, int saveVersion = -1)
	{
		MaxApoPassedByCityByGlyph = new Dictionary<string, Dictionary<string, int>>();
		if (container?.Glyphs == null)
		{
			return;
		}
		foreach (SerializedGlyphData glyph in container.Glyphs)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (SerializedMaxApoPassed item in glyph.MaxApoPassedByCity)
			{
				dictionary.Add(item.CityId, item.MaxApoPassed);
			}
			MaxApoPassedByCityByGlyph.Add(glyph.GlyphId, dictionary);
		}
	}

	public void Deserialize(SerializedGlyphsContainer container = null, int saveVersion = -1)
	{
		if (saveVersion < 19 || container == null || container.SerializedModifyBuildingActionsCostGlyphs == null)
		{
			return;
		}
		foreach (SerializedModifyBuildingActionsCostGlyph serializedModifyBuildingActionsCostGlyph in container.SerializedModifyBuildingActionsCostGlyphs)
		{
			foreach (GlyphModifyBuildingActionsCostEffect buildingActionsCostModifier in BuildingActionsCostModifiers)
			{
				if (buildingActionsCostModifier.GlyphParentId == serializedModifyBuildingActionsCostGlyph.GlyphParentId && buildingActionsCostModifier.EffectIndex == serializedModifyBuildingActionsCostGlyph.EffectIndex)
				{
					buildingActionsCostModifier.Deserialize(serializedModifyBuildingActionsCostGlyph);
				}
			}
		}
	}

	public List<SerializedGlyphData> Serialize()
	{
		List<SerializedGlyphData> list = new List<SerializedGlyphData>();
		foreach (KeyValuePair<string, Dictionary<string, int>> item in MaxApoPassedByCityByGlyph)
		{
			List<SerializedMaxApoPassed> list2 = new List<SerializedMaxApoPassed>();
			foreach (KeyValuePair<string, int> item2 in item.Value)
			{
				list2.Add(new SerializedMaxApoPassed
				{
					CityId = item2.Key,
					MaxApoPassed = item2.Value
				});
			}
			list.Add(new SerializedGlyphData
			{
				GlyphId = item.Key,
				MaxApoPassedByCity = list2
			});
		}
		return list;
	}

	public SerializedGlyphsContainer SerializeGlyphs()
	{
		List<SerializedModifyBuildingActionsCostGlyph> list = null;
		if (BuildingActionsCostModifiers.Count > 0)
		{
			list = new List<SerializedModifyBuildingActionsCostGlyph>();
			foreach (GlyphModifyBuildingActionsCostEffect buildingActionsCostModifier in BuildingActionsCostModifiers)
			{
				list.Add((SerializedModifyBuildingActionsCostGlyph)buildingActionsCostModifier.Serialize());
			}
		}
		return new SerializedGlyphsContainer
		{
			SerializedModifyBuildingActionsCostGlyphs = list
		};
	}

	[DevConsoleCommand("GlyphEnabled")]
	private static string Debug_EnabledGlyphs()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Count == 0)
		{
			return "There are no glyphs enabled for " + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + ".";
		}
		string text = "Enabled glyph for " + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + " :\n";
		foreach (GlyphDefinition selectedGlyph in TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs)
		{
			text = text + "\t- " + selectedGlyph.Id + "\n";
		}
		return text;
	}

	[DevConsoleCommand("GlyphSelect")]
	private static void Debug_SelectGlyph([StringConverter(typeof(StringToGlyphIdConverter))] string glyphId)
	{
		TPSingleton<GlyphManager>.Instance.ToggleGlyphSelection(GlyphDatabase.GlyphDefinitions[glyphId], select: true);
	}

	[DevConsoleCommand("GlyphUnselect")]
	private static void Debug_UnselectGlyph([StringConverter(typeof(StringToGlyphIdConverter))] string glyphId)
	{
		TPSingleton<GlyphManager>.Instance.ToggleGlyphSelection(GlyphDatabase.GlyphDefinitions[glyphId], select: false);
	}
}

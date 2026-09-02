using System;
using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Controller.Meta;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Definition;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Meta;
using TheLastStand.Serialization;
using TheLastStand.View;
using TheLastStand.View.Resource;
using TheLastStand.View.ToDoList;
using UnityEngine;

namespace TheLastStand.Manager;

[StringConverter(typeof(StringToTPSingletonConverter<ResourceManager>))]
public class ResourceManager : Manager<ResourceManager>, ISerializable, IDeserializable
{
	[Flags]
	public enum E_PriceModifierType
	{
		ProductionBuildings = 1,
		DefensiveBuildings = 2,
		BuildingUpgrades = 4,
		Items = 8,
		Recruitment = 0x10,
		BuildingsConstruction = 3,
		Buildings = 7,
		All = 0x1F
	}

	[Flags]
	public enum E_ResourceType
	{
		Gold = 1,
		Materials = 2,
		TaintedEssences = 4,
		GoldAndMaterials = 3,
		All = 7
	}

	[SerializeField]
	private DataColorDictionary resourceColors;

	[SerializeField]
	private ResourceTooltip resourceTooltip;

	private int materials;

	private int workers;

	private int workersBuffer;

	public int ScavengeWorkersThisTurn;

	public int BackwardCompatibilityMaterials;

	public int BackwardCompatibilityGold;

	public static ResourceTooltip ResourceTooltip => TPSingleton<ResourceManager>.Instance.resourceTooltip;

	public static ResourceDefinition SelectedCityResourceDefinition => ResourceDatabase.ResourceDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.InitResourceDefinitionId];

	public int Gold { get; private set; }

	public bool HasSpentGold { get; private set; }

	public bool HasSpentMaterials { get; private set; }

	[DevConsoleCommand(Options = (DevConsoleCommandOptions.CanReadWrite | DevConsoleCommandOptions.ForceStatic))]
	public int Materials
	{
		get
		{
			return materials;
		}
		set
		{
			int num = value - materials;
			if (num > 0)
			{
				TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaterialsObtained, num);
			}
			else
			{
				if (num < 0)
				{
					HasSpentMaterials = true;
				}
				TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaterialsSpent, num);
			}
			materials = Mathf.Max(value, 0);
			this.OnMaterialsChange?.Invoke(materials);
		}
	}

	public int MaxWorkers { get; private set; }

	public int Workers
	{
		get
		{
			return workers;
		}
		set
		{
			workers = Mathf.Clamp(value, 0, MaxWorkers);
			this.OnWorkersChange?.Invoke(workers, MaxWorkers);
			if (TPSingleton<ToDoListView>.Instance.IsDisplayed)
			{
				TPSingleton<ToDoListView>.Instance.RefreshWorkersNotification();
			}
		}
	}

	[DevConsoleCommand("Workers", Options = DevConsoleCommandOptions.CanReadWrite)]
	public static int Debug_Workers
	{
		get
		{
			return TPSingleton<ResourceManager>.Instance.Workers;
		}
		set
		{
			TPSingleton<ResourceManager>.Instance.Workers = Mathf.Clamp(value, 0, TPSingleton<ResourceManager>.Instance.MaxWorkers);
			TPSingleton<ResourceManager>.Instance.workersBuffer = Mathf.Clamp(value, 0, TPSingleton<ResourceManager>.Instance.MaxWorkers);
		}
	}

	[DevConsoleCommand(Name = "Gold", Options = (DevConsoleCommandOptions.CanReadWrite | DevConsoleCommandOptions.ForceStatic))]
	public static int Debug_Gold
	{
		get
		{
			return TPSingleton<ResourceManager>.Instance.Gold;
		}
		private set
		{
			TPSingleton<ResourceManager>.Instance.SetGold(value, updateGoldMetaConditions: false);
		}
	}

	public event Action<int> OnGoldChange;

	public event Action<int> OnMaterialsChange;

	public event Action<int, int> OnWorkersChange;

	public static int ComputeExtraPercentageForCost(E_PriceModifierType type, E_ResourceType resourceCostType)
	{
		return ApocalypseManager.CurrentApocalypse.ExtraPercentageForCosts(type, resourceCostType) + TPSingleton<GlyphManager>.Instance.CostsModifiers.GetValueOrDefault(type);
	}

	public static int GetModifiedWorkersCost(BuildingActionDefinition buildingActionDefinition, bool updateGlyphLimits = false)
	{
		return Mathf.Max(0, TPSingleton<GlyphManager>.Instance.GetModifiedWorkersCost(buildingActionDefinition, updateGlyphLimits));
	}

	public static void OnTurnStart()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			TPSingleton<ResourceManager>.Instance.HasSpentGold = false;
			TPSingleton<ResourceManager>.Instance.HasSpentMaterials = false;
			TPSingleton<ResourceManager>.Instance.ScavengeWorkersThisTurn = 0;
		}
	}

	public void DecreaseMaxWorkers(int maxWorkersToRemove)
	{
		MaxWorkers -= maxWorkersToRemove;
		workersBuffer -= maxWorkersToRemove;
		Workers = Mathf.Max(0, workers - maxWorkersToRemove);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedResources serializedResources)
		{
			SetGold(serializedResources.Gold, updateGoldMetaConditions: false);
			SetGold(Gold + BackwardCompatibilityGold);
			materials = serializedResources.Materials;
			Materials += BackwardCompatibilityMaterials;
			MaxWorkers = serializedResources.MaxWorkers;
			workersBuffer = serializedResources.WorkersBuffer;
			Workers = serializedResources.Workers;
			HasSpentGold = serializedResources.HasSpentGold;
			HasSpentMaterials = serializedResources.HasSpentMaterials;
			ScavengeWorkersThisTurn = serializedResources.ScavengeWorkersThisTurn;
		}
		else
		{
			SetGold(SelectedCityResourceDefinition.Gold, updateGoldMetaConditions: false);
			materials = SelectedCityResourceDefinition.Materials;
			if (GlyphManager.TryGetGlyphEffects(out List<GlyphIncreaseStartingResourcesEffectDefinition> glyphEffects))
			{
				foreach (GlyphIncreaseStartingResourcesEffectDefinition item in glyphEffects)
				{
					if (item.GoldBonusExpression != null)
					{
						Gold += item.GoldBonusExpression.EvalToInt();
					}
					if (item.MaterialsBonusExpression != null)
					{
						materials += item.MaterialsBonusExpression.EvalToInt();
					}
				}
			}
			if (MetaUpgradeEffectsController.TryGetEffectsOfType<InitResourcesBonusMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
			{
				for (int num = effects.Length - 1; num >= 0; num--)
				{
					Gold += effects[num].GoldBonus;
					materials += effects[num].MaterialsBonus;
				}
			}
		}
		GameView.TopScreenPanel.TurnPanel.PhasePanel.RefreshResourcesPhasePanel();
	}

	public Color GetResourceColor(string resourceId)
	{
		Color? colorById = resourceColors.GetColorById(resourceId);
		if (colorById.HasValue)
		{
			return colorById.Value;
		}
		TPSingleton<ResourceManager>.Instance.LogError("GetResourceColor(): no color found for id '" + resourceId + "'");
		return TPHelpers.__COLOR_ERROR;
	}

	public void IncreaseMaxWorkers(int maxWorkersToAdd)
	{
		MaxWorkers += maxWorkersToAdd;
		workersBuffer += maxWorkersToAdd;
		Workers = Mathf.Min(workers + maxWorkersToAdd, workersBuffer);
		if (MaxWorkers >= 12)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_HAVE_12_WORKERS);
		}
	}

	public void RefillWorkers()
	{
		TPSingleton<ResourceManager>.Instance.workersBuffer = TPSingleton<ResourceManager>.Instance.MaxWorkers;
		TPSingleton<ResourceManager>.Instance.Workers = TPSingleton<ResourceManager>.Instance.MaxWorkers;
	}

	public ISerializedData Serialize()
	{
		return new SerializedResources
		{
			Gold = Gold,
			Materials = Materials,
			Workers = Workers,
			MaxWorkers = MaxWorkers,
			WorkersBuffer = workersBuffer,
			HasSpentGold = HasSpentGold,
			HasSpentMaterials = HasSpentMaterials,
			ScavengeWorkersThisTurn = ScavengeWorkersThisTurn
		};
	}

	public void SetGold(int newValue, bool updateGoldMetaConditions = true)
	{
		int num = newValue - Gold;
		if (num > 0 && updateGoldMetaConditions)
		{
			TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.GoldObtained, Mathf.Abs(num));
		}
		else if (num < 0 && updateGoldMetaConditions)
		{
			HasSpentGold = true;
			TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.GoldSpent, Mathf.Abs(num));
		}
		Gold = Mathf.Max(newValue, 0);
		this.OnGoldChange?.Invoke(Gold);
	}

	public void UseWorkers(int workersToUse)
	{
		workersToUse = Mathf.Min(workers, workersToUse);
		workersBuffer -= workersToUse;
		Workers -= workersToUse;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.WorkersSpent, workersToUse);
	}

	public static void Debug_GainResources()
	{
		TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + 10000);
		TPSingleton<ResourceManager>.Instance.Materials += 10000;
		TPSingleton<ResourceManager>.Instance.RefillWorkers();
	}

	[DevConsoleCommand("WorkersDecreaseMax")]
	public static void Debug_DecreaseMaxWorkers(int workersMaxToDecrease)
	{
		TPSingleton<ResourceManager>.Instance.DecreaseMaxWorkers(workersMaxToDecrease);
	}

	[DevConsoleCommand("WorkersIncreaseMax")]
	public static void Debug_IncreaseMaxWorkers(int workersMaxToIncrease)
	{
		TPSingleton<ResourceManager>.Instance.IncreaseMaxWorkers(workersMaxToIncrease);
	}
}

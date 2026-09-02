using System.Collections.Generic;
using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.CastFx;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class BuildingUpgrade
{
	public class Constants
	{
		public const string BuildingUpgradeNameUpgradeShopLevel = "UpgradeShopLevel";
	}

	public class LeveledBuildingUpgrade
	{
		public List<BuildingUpgradeEffect> Effects { get; private set; }

		public TheLastStand.Model.CastFx.CastFx OverrideCastFx { get; set; }

		public LeveledBuildingUpgrade(List<BuildingUpgradeEffect> effects)
		{
			Effects = effects;
		}
	}

	public Building Building { get; private set; }

	public BuildingUpgradeController BuildingUpgradeController { get; private set; }

	public BuildingUpgradeDefinition BuildingUpgradeDefinition { get; private set; }

	public List<LeveledBuildingUpgrade> BuildingUpgradeLevels { get; private set; } = new List<LeveledBuildingUpgrade>();

	public TheLastStand.Model.CastFx.CastFx CastFx { get; set; }

	public virtual int UpgradeLevel { get; set; } = -1;

	public bool IsUnlocked => UpgradeLevel > -1;

	public BuildingUpgrade(SerializedUpgrade container, BuildingUpgradeDefinition definition, BuildingUpgradeController controller, Building building)
		: this(definition, controller, building)
	{
		Deserialize(container);
		ExecuteUpgradeEffects();
	}

	public BuildingUpgrade(BuildingUpgradeDefinition definition, BuildingUpgradeController controller, Building building)
	{
		BuildingUpgradeDefinition = definition;
		BuildingUpgradeController = controller;
		Building = building;
		InitializeUpgradeEffects();
	}

	public void ExecuteUpgradeEffects()
	{
		if (UpgradeLevel < 0)
		{
			return;
		}
		for (int i = 0; i <= UpgradeLevel; i++)
		{
			int j = 0;
			for (int count = BuildingUpgradeLevels[i].Effects.Count; j < count; j++)
			{
				BuildingUpgradeLevels[i].Effects[j].BuildingUpgradeEffectController.TriggerEffect(onLoad: true);
			}
		}
	}

	public virtual void Deserialize(SerializedUpgrade buildingActionElement)
	{
		UpgradeLevel = Mathf.Min(buildingActionElement.UpgradeLevel, BuildingDatabase.BuildingUpgradeDefinitions[buildingActionElement.Id].LeveledBuildingUpgradeDefinitions.Count - 1);
	}

	public virtual ISerializedData Serialize()
	{
		return new SerializedUpgrade
		{
			Id = BuildingUpgradeDefinition.Id,
			UpgradeLevel = UpgradeLevel
		};
	}

	private void InitializeUpgradeEffects()
	{
		int i = 0;
		for (int count = BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions.Count; i < count; i++)
		{
			BuildingUpgradeLevels.Add(new LeveledBuildingUpgrade(new List<BuildingUpgradeEffect>()));
			int j = 0;
			for (int count2 = BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[i].BuildingUpgradeEffectDefinitions.Count; j < count2; j++)
			{
				BuildingUpgradeEffectDefinition buildingUpgradeEffectDefinition = BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[i].BuildingUpgradeEffectDefinitions[j];
				BuildingUpgradeEffect item = null;
				switch (buildingUpgradeEffectDefinition.Id)
				{
				case "UnlockAction":
					item = new UnlockActionController(buildingUpgradeEffectDefinition as UnlockActionDefinition, this).BuildingUpgradeEffect;
					break;
				case "SwapAction":
					item = new SwapActionController(buildingUpgradeEffectDefinition as SwapActionDefinition, this).BuildingUpgradeEffect;
					break;
				case "ImproveGaugeEffect":
					item = new ImproveGaugeEffectController(buildingUpgradeEffectDefinition as ImproveGaugeEffectDefinition, this).BuildingUpgradeEffect;
					break;
				case "ImproveGuessWho":
					item = new ImproveGuessWhoController(buildingUpgradeEffectDefinition as ImproveGuessWhoDefinition, this).BuildingUpgradeEffect;
					break;
				case "ImprovePassive":
					item = new ImprovePassiveController(buildingUpgradeEffectDefinition as ImprovePassiveDefinition, this).BuildingUpgradeEffect;
					break;
				case "ImproveLevel":
					item = new ImproveLevelController(buildingUpgradeEffectDefinition as ImproveLevelDefinition, this).BuildingUpgradeEffect;
					break;
				case "ImproveUnitLimit":
					item = new ImproveUnitLimitController(buildingUpgradeEffectDefinition as ImproveUnitLimitDefinition, this).BuildingUpgradeEffect;
					break;
				case "ImproveSellRatio":
					item = new ImproveSellRatioController(buildingUpgradeEffectDefinition as ImproveSellRatioDefinition, this).BuildingUpgradeEffect;
					break;
				case "ReplaceBuilding":
					item = new ReplaceBuildingController(buildingUpgradeEffectDefinition as ReplaceBuildingDefinition, this).BuildingUpgradeEffect;
					break;
				case "SwapSkill":
					item = new SwapSkillController(buildingUpgradeEffectDefinition as SwapSkillDefinition, this).BuildingUpgradeEffect;
					break;
				case "IncreasePlaysPerTurn":
					item = new IncreasePlaysPerTurnController(buildingUpgradeEffectDefinition as IncreasePlaysPerTurnDefinition, this).BuildingUpgradeEffect;
					break;
				}
				BuildingUpgradeLevels[i].Effects.Add(item);
			}
		}
	}
}

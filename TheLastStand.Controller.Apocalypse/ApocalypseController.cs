using System;
using System.Collections.Generic;
using TheLastStand.Controller.Apocalypse.ApocalypseEffects;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model.Apocalypse;
using TheLastStand.Model.Apocalypse.ApocalypseEffects;
using UnityEngine;

namespace TheLastStand.Controller.Apocalypse;

public class ApocalypseController
{
	public TheLastStand.Model.Apocalypse.Apocalypse Apocalypse { get; }

	public event Action OnApocalypseLevelComputed = delegate
	{
	};

	public ApocalypseController(List<ApocalypseModifierStepDefinition> stepDefinitions = null)
	{
		Apocalypse = new TheLastStand.Model.Apocalypse.Apocalypse(this, stepDefinitions);
		ComputeAllData();
	}

	public void AddSelectedModifierStep(ApocalypseModifierStepDefinition modifierStepDefinition, bool computeLevel = true, bool computeEffects = false, bool removeExistingModifier = true)
	{
		if (Apocalypse.ModifierStepDefinitions.Contains(modifierStepDefinition))
		{
			return;
		}
		if (removeExistingModifier && ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.TryGetValue(modifierStepDefinition, out var value) && value != null)
		{
			foreach (ApocalypseModifierStepDefinition stepDefinition in value.StepDefinitions)
			{
				RemoveSelectedModifierStep(stepDefinition, computeLevel: false);
			}
		}
		Apocalypse.ModifierStepDefinitions.Add(modifierStepDefinition);
		ComputeAllData(computeLevel, computeEffects);
	}

	public void CheckEffectsActivationOnTurnCondition()
	{
		foreach (AddEnemiesStatModifierFromTurnApocalypseEffect addEnemiesStatModifierFromTurnEffect in Apocalypse.AddEnemiesStatModifierFromTurnEffects)
		{
			if (!addEnemiesStatModifierFromTurnEffect.IsActive)
			{
				addEnemiesStatModifierFromTurnEffect.StatModifierEffectController.CheckIfCanActivate(onLoad: true);
			}
		}
	}

	public void ClearAllSelectedModifierSteps(bool computeLevel = true, bool computeEffects = false)
	{
		Apocalypse.ModifierStepDefinitions.Clear();
		ComputeAllData(computeLevel, computeEffects);
	}

	public void ComputeAllData(bool computeLevel = true, bool computeEffects = true)
	{
		if (computeLevel)
		{
			ComputeCurrentLevel();
		}
		if (computeEffects)
		{
			ComputeEffectsAndModifiers();
		}
	}

	public void RemoveSelectedModifierStep(ApocalypseModifierStepDefinition modifierStepDefinition, bool computeLevel = true, bool computeEffects = false)
	{
		if (Apocalypse.ModifierStepDefinitions.Contains(modifierStepDefinition))
		{
			Apocalypse.ModifierStepDefinitions.Remove(modifierStepDefinition);
			ComputeAllData(computeLevel, computeEffects);
		}
	}

	public void SetSelectedModifierSteps(List<ApocalypseModifierStepDefinition> stepDefinitions, bool computeLevel = true, bool computeEffects = true)
	{
		Apocalypse.ModifierStepDefinitions.Clear();
		if (stepDefinitions != null)
		{
			foreach (ApocalypseModifierStepDefinition stepDefinition in stepDefinitions)
			{
				AddSelectedModifierStep(stepDefinition, computeLevel: false);
			}
		}
		ComputeAllData(computeLevel, computeEffects);
	}

	private void ComputeAllEffects()
	{
		Apocalypse.AllEffects.Clear();
		foreach (ApocalypseModifierStepDefinition modifierStepDefinition in Apocalypse.ModifierStepDefinitions)
		{
			if (modifierStepDefinition.Effects != null && modifierStepDefinition.Effects.Count > 0)
			{
				Apocalypse.AllEffects.AddRange(modifierStepDefinition.Effects);
			}
		}
	}

	private void ComputeCurrentLevel()
	{
		Apocalypse.CurrentLevel = 0;
		if (Apocalypse.ModifierStepDefinitions == null || Apocalypse.ModifierStepDefinitions.Count == 0)
		{
			this.OnApocalypseLevelComputed();
			return;
		}
		int count = Apocalypse.ModifierStepDefinitions.Count;
		for (int i = 0; i < count; i++)
		{
			Apocalypse.CurrentLevel += Apocalypse.ModifierStepDefinitions[i].ApocalypseLevel;
		}
		this.OnApocalypseLevelComputed();
	}

	private void ComputeEffectsAndModifiers()
	{
		ComputeAllEffects();
		ComputeEffectsModifiers();
	}

	private void ComputeEffectsModifiers()
	{
		ComputeAffixesFlags();
		ComputeBuildingsDeadZoneRangeModifiers();
		ComputeBuildingsNotDemolishable();
		ComputeEnemiesInjuryStageStatModifiers();
		ComputeEnemiesProgressionOffset();
		ComputeEnemiesSpawnWaveWeightMultiplier();
		ComputeEnemiesStatsBaseValueModifiers();
		ComputeEnemiesStatModifierAfterTurn();
		ComputeFogSpawnersMultiplier();
		ComputeForbiddenBuildingCategoryAroundMagicCircle();
		ComputeInnSlotsRecruitmentLevel();
		ComputeMagicCircleStartingHealthMultiplier();
		ComputePanicGainMultiplier();
		ComputePlayableUnitBlockLineOfSight();
		ComputeRemoveStartingPlayableUnit();
		ComputeSkillProgressionFlags();
	}

	private void ComputeAffixesFlags()
	{
		Apocalypse.AffixesFlags.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is AddAffixFlagApocalypseEffectDefinition addAffixFlagApocalypseEffectDefinition && !Apocalypse.AffixesFlags.Contains(addAffixFlagApocalypseEffectDefinition.Flag))
			{
				Apocalypse.AffixesFlags.Add(addAffixFlagApocalypseEffectDefinition.Flag);
			}
		}
	}

	private void ComputeBuildingsDeadZoneRangeModifiers()
	{
		Apocalypse.BuildingsModifiedDeadZoneRange.Clear();
		int num = 1;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (!(allEffect is ModifyBuildingsDeadZoneRangeApocalypseEffectDefinition modifyBuildingsDeadZoneRangeApocalypseEffectDefinition))
			{
				continue;
			}
			foreach (string buildingsId in modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.BuildingsIds)
			{
				if (Apocalypse.BuildingsModifiedDeadZoneRange.ContainsKey(buildingsId))
				{
					Apocalypse.BuildingsModifiedDeadZoneRange[buildingsId] = modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range;
				}
				else
				{
					Apocalypse.BuildingsModifiedDeadZoneRange.Add(buildingsId, modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range);
				}
			}
			if (num < modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range)
			{
				num = modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range;
			}
		}
		Apocalypse.BuildingsMaxDeadZoneRange = num;
	}

	private void ComputeBuildingsNotDemolishable()
	{
		Apocalypse.BuildingsNotDemolishableAnymore.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (!(allEffect is SetBuildingsNotDemolishableApocalypseEffectDefinition setBuildingsNotDemolishableApocalypseEffectDefinition))
			{
				continue;
			}
			foreach (string buildingsId in setBuildingsNotDemolishableApocalypseEffectDefinition.BuildingsIds)
			{
				if (!Apocalypse.BuildingsNotDemolishableAnymore.Contains(buildingsId))
				{
					Apocalypse.BuildingsNotDemolishableAnymore.Add(buildingsId);
				}
			}
		}
	}

	private void ComputeEnemiesInjuryStageStatModifiers()
	{
		Apocalypse.EnemiesInjuryStageInjuryStatModifiers.Clear();
		for (int num = Apocalypse.AllEffects.Count - 1; num >= 0; num--)
		{
			if (Apocalypse.AllEffects[num] is ModifyEnemiesInjuryStageApocalypseEffectDefinition modifyEnemiesInjuryStageApocalypseEffectDefinition)
			{
				if (!Apocalypse.EnemiesInjuryStageInjuryStatModifiers.ContainsKey(modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStage))
				{
					Apocalypse.EnemiesInjuryStageInjuryStatModifiers.Add(modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStage, new List<ModifyEnemiesInjuryStageApocalypseEffectDefinition.ApocalypseInjuryStatModifier>());
				}
				for (int num2 = modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStatModifiers.Count - 1; num2 >= 0; num2--)
				{
					Apocalypse.EnemiesInjuryStageInjuryStatModifiers[modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStage].Add(modifyEnemiesInjuryStageApocalypseEffectDefinition.InjuryStatModifiers[num2]);
				}
			}
		}
	}

	private void ComputeEnemiesProgressionOffset()
	{
		Apocalypse.EnemiesProgressionOffset = 0;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is IncreaseEnemiesProgressionOffsetApocalypseEffectDefinition increaseEnemiesProgressionOffsetApocalypseEffectDefinition)
			{
				Apocalypse.EnemiesProgressionOffset += increaseEnemiesProgressionOffsetApocalypseEffectDefinition.Value;
			}
		}
	}

	private void ComputeEnemiesSpawnWaveWeightMultiplier()
	{
		Apocalypse.EnemiesSpawnWaveWeightMultiplier.Clear();
		for (int num = Apocalypse.AllEffects.Count - 1; num >= 0; num--)
		{
			if (Apocalypse.AllEffects[num] is MultiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition)
			{
				for (int num2 = multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition.EnemyUnitIds.Count - 1; num2 >= 0; num2--)
				{
					string key = multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition.EnemyUnitIds[num2];
					if (!Apocalypse.EnemiesSpawnWaveWeightMultiplier.ContainsKey(key))
					{
						Apocalypse.EnemiesSpawnWaveWeightMultiplier.Add(key, multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition.Multiplier);
					}
					else
					{
						Apocalypse.EnemiesSpawnWaveWeightMultiplier[key] = multiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition.Multiplier;
					}
				}
			}
		}
	}

	private void ComputeEnemiesStatsBaseValueModifiers()
	{
		Apocalypse.EnemiesStatsBaseValueModifiers.Clear();
		for (int num = Apocalypse.AllEffects.Count - 1; num >= 0; num--)
		{
			if (Apocalypse.AllEffects[num] is EnemiesStatBaseValueModifierApocalypseEffectDefinition enemiesStatBaseValueModifierApocalypseEffectDefinition)
			{
				for (int num2 = enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies.Count - 1; num2 >= 0; num2--)
				{
					if (!Apocalypse.EnemiesStatsBaseValueModifiers.ContainsKey(enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2]))
					{
						Apocalypse.EnemiesStatsBaseValueModifiers.Add(enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2], new Dictionary<UnitStatDefinition.E_Stat, float>());
					}
					if (!Apocalypse.EnemiesStatsBaseValueModifiers[enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2]].ContainsKey(enemiesStatBaseValueModifierApocalypseEffectDefinition.Stat))
					{
						Apocalypse.EnemiesStatsBaseValueModifiers[enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2]].Add(enemiesStatBaseValueModifierApocalypseEffectDefinition.Stat, enemiesStatBaseValueModifierApocalypseEffectDefinition.Value);
					}
					else
					{
						Apocalypse.EnemiesStatsBaseValueModifiers[enemiesStatBaseValueModifierApocalypseEffectDefinition.AffectedEnemies[num2]][enemiesStatBaseValueModifierApocalypseEffectDefinition.Stat] += enemiesStatBaseValueModifierApocalypseEffectDefinition.Value;
					}
				}
			}
		}
	}

	private void ComputeEnemiesStatModifierAfterTurn()
	{
		foreach (AddEnemiesStatModifierFromTurnApocalypseEffect addEnemiesStatModifierFromTurnEffect in Apocalypse.AddEnemiesStatModifierFromTurnEffects)
		{
			if (addEnemiesStatModifierFromTurnEffect.IsActive)
			{
				addEnemiesStatModifierFromTurnEffect.StatModifierEffectController.DeActivate(onLoad: false);
			}
			if (addEnemiesStatModifierFromTurnEffect.IsHookedForActivation)
			{
				addEnemiesStatModifierFromTurnEffect.StatModifierEffectController.UnHookActivationConditions();
			}
			if (addEnemiesStatModifierFromTurnEffect.IsHookedForDeactivation)
			{
				addEnemiesStatModifierFromTurnEffect.StatModifierEffectController.UnHookDeactivationConditions();
			}
		}
		Apocalypse.AddEnemiesStatModifierFromTurnEffects.Clear();
		Apocalypse.ActiveStatModifierFromTurn.Clear();
		for (int num = Apocalypse.AllEffects.Count - 1; num >= 0; num--)
		{
			if (Apocalypse.AllEffects[num] is AddEnemiesStatModifierFromTurnApocalypseEffectDefinition effectDefinition)
			{
				AddEnemiesStatModifierFromTurnApocalypseEffect statModifierEffect = new AddEnemiesStatModifierFromTurnApocalypseEffectController(effectDefinition).StatModifierEffect;
				Apocalypse.AddEnemiesStatModifierFromTurnEffects.Add(statModifierEffect);
				statModifierEffect.StatModifierEffectController.HookActivationConditions();
				statModifierEffect.StatModifierEffectController.HookDeactivationConditions();
				if (ApplicationManager.Application.IsGameState)
				{
					statModifierEffect.StatModifierEffectController.CheckIfCanActivate(onLoad: false);
				}
			}
		}
	}

	private void ComputeFogSpawnersMultiplier()
	{
		Apocalypse.FogSpawnersApocalypseMultiplier = 0f;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is GenerateFogSpawnersApocalypseEffectDefinition generateFogSpawnersApocalypseEffectDefinition)
			{
				Apocalypse.FogSpawnersApocalypseMultiplier = generateFogSpawnersApocalypseEffectDefinition.Multiplier;
			}
		}
	}

	private void ComputeForbiddenBuildingCategoryAroundMagicCircle()
	{
		Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (!(allEffect is ForbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition))
			{
				continue;
			}
			int i = 1;
			int radiusRange = forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition.RadiusRange;
			if (i > radiusRange)
			{
				continue;
			}
			for (; i <= radiusRange; i++)
			{
				if (Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange.ContainsKey(i))
				{
					if (!Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange[i].Contains(forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition.BuildingCategory))
					{
						Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange[i].Add(forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition.BuildingCategory);
					}
				}
				else
				{
					Apocalypse.ForbiddenBuildingCategoriesAroundMagicCircleByRange.Add(i, new List<BuildingDefinition.E_BuildingCategory> { forbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition.BuildingCategory });
				}
			}
		}
	}

	private void ComputeInnSlotsRecruitmentLevel()
	{
		Apocalypse.InnSlotsOverridenRecruitmentLevelId.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is ModifyInnSlotRecruitmentLevelApocalypseEffectDefinition modifyInnSlotRecruitmentLevelApocalypseEffectDefinition)
			{
				if (Apocalypse.InnSlotsOverridenRecruitmentLevelId.ContainsKey(modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.SlotIndex))
				{
					Apocalypse.InnSlotsOverridenRecruitmentLevelId[modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.SlotIndex] = modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.LevelId;
				}
				else
				{
					Apocalypse.InnSlotsOverridenRecruitmentLevelId.Add(modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.SlotIndex, modifyInnSlotRecruitmentLevelApocalypseEffectDefinition.LevelId);
				}
			}
		}
	}

	private void ComputeMagicCircleStartingHealthMultiplier()
	{
		Apocalypse.MagicCircleStartingHealthMultiplier = 1f;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is ModifyMagicCircleStartingHealthApocalypseEffectDefinition modifyMagicCircleStartingHealthApocalypseEffectDefinition)
			{
				Apocalypse.MagicCircleStartingHealthMultiplier = modifyMagicCircleStartingHealthApocalypseEffectDefinition.HealthMultiplier;
			}
		}
	}

	private void ComputePanicGainMultiplier()
	{
		bool flag = false;
		Apocalypse.PanicGainMultiplier = 0f;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is MultiplyPanicGainApocalypseEffectDefinition multiplyPanicGainApocalypseEffectDefinition)
			{
				flag = true;
				Apocalypse.PanicGainMultiplier += multiplyPanicGainApocalypseEffectDefinition.Multiplier;
			}
		}
		if (!flag)
		{
			Apocalypse.PanicGainMultiplier = 1f;
		}
	}

	private void ComputePlayableUnitBlockLineOfSight()
	{
		Apocalypse.PlayableUnitBlockLineOfSight = false;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is PlayableUnitBlockLineOfSightApocalypseEffectDefinition)
			{
				Apocalypse.PlayableUnitBlockLineOfSight = true;
			}
		}
	}

	private void ComputeRemoveStartingPlayableUnit()
	{
		Apocalypse.RemoveStartingPlayableUnitAmount = 0;
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is RemoveStartingPlayableUnitApocalypseEffectDefinition removeStartingPlayableUnitApocalypseEffectDefinition)
			{
				Apocalypse.RemoveStartingPlayableUnitAmount += Mathf.Abs(removeStartingPlayableUnitApocalypseEffectDefinition.Amount);
			}
		}
	}

	private void ComputeSkillProgressionFlags()
	{
		Apocalypse.SkillProgressionFlags.Clear();
		foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
		{
			if (allEffect is AddSkillProgressionFlagApocalypseEffectDefinition addSkillProgressionFlagApocalypseEffectDefinition && !Apocalypse.SkillProgressionFlags.Contains(addSkillProgressionFlagApocalypseEffectDefinition.Flag))
			{
				Apocalypse.SkillProgressionFlags.Add(addSkillProgressionFlagApocalypseEffectDefinition.Flag);
			}
		}
	}
}

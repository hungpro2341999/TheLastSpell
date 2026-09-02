using System.Collections.Generic;
using TPLib;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Recruitment;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Controller.Unit;

public static class RecruitmentController
{
	public enum E_ImpossibleRecruitmentReason
	{
		None,
		NotEnoughGold,
		MaxAmountReach
	}

	public static void BuyRerollRoster()
	{
		if (TPSingleton<ResourceManager>.Instance.Gold >= ComputeRerollCost())
		{
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - ComputeRerollCost());
			GenerateNewRoster();
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_REROLL_INN);
		}
	}

	public static bool CanOpenRecruitmentPanel()
	{
		if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production && (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction))
		{
			return BuildingManager.HasInn();
		}
		return false;
	}

	public static bool CanRecruitMage(out E_ImpossibleRecruitmentReason impossibleRecruitmentReason)
	{
		impossibleRecruitmentReason = E_ImpossibleRecruitmentReason.None;
		if (ComputeMageCost() > TPSingleton<ResourceManager>.Instance.Gold)
		{
			impossibleRecruitmentReason = E_ImpossibleRecruitmentReason.NotEnoughGold;
		}
		if (BuildingManager.MagicCircle.MageCount == BuildingManager.MagicCircle.MageSlots)
		{
			impossibleRecruitmentReason = E_ImpossibleRecruitmentReason.MaxAmountReach;
		}
		if (TPSingleton<PlayableUnitManager>.Instance.Recruitment.HasMage && BuildingManager.MagicCircle.MageCount < BuildingManager.MagicCircle.MageSlots)
		{
			return TPSingleton<ResourceManager>.Instance.Gold >= ComputeMageCost();
		}
		return false;
	}

	public static bool CanRecruitUnit(PlayableUnit unitToRecruit, out E_ImpossibleRecruitmentReason impossibleRecruitmentReason)
	{
		impossibleRecruitmentReason = E_ImpossibleRecruitmentReason.None;
		if (ComputeUnitCost(unitToRecruit) > TPSingleton<ResourceManager>.Instance.Gold)
		{
			impossibleRecruitmentReason |= E_ImpossibleRecruitmentReason.NotEnoughGold;
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count >= TPSingleton<PlayableUnitManager>.Instance.Recruitment.UnitsLimit)
		{
			impossibleRecruitmentReason |= E_ImpossibleRecruitmentReason.MaxAmountReach;
		}
		return impossibleRecruitmentReason == E_ImpossibleRecruitmentReason.None;
	}

	public static void CloseRecruitmentPanel()
	{
		if (!TPSingleton<RecruitmentView>.Instance.IsGeneratingNewRoster)
		{
			TPSingleton<RecruitmentView>.Instance.Close();
			GameController.SetState(Game.E_State.Management);
		}
	}

	public static int ComputeMageCost()
	{
		int num = ResourceManager.ComputeExtraPercentageForCost(ResourceManager.E_PriceModifierType.Recruitment, ResourceManager.E_ResourceType.Gold);
		int num2 = PlayableUnitDatabase.RecruitmentDefinition.MageCost.EvalToInt();
		return Mathf.RoundToInt((float)num2 + (float)num2 * ((float)num * 0.01f));
	}

	public static int ComputeRerollCost()
	{
		int num = ResourceManager.ComputeExtraPercentageForCost(ResourceManager.E_PriceModifierType.Recruitment, ResourceManager.E_ResourceType.Gold);
		return Mathf.RoundToInt((float)PlayableUnitDatabase.RecruitmentDefinition.RosterRerollCost + (float)PlayableUnitDatabase.RecruitmentDefinition.RosterRerollCost * ((float)num * 0.01f));
	}

	public static int ComputeUnitCost(PlayableUnit unitToRecruit)
	{
		int num = ResourceManager.ComputeExtraPercentageForCost(ResourceManager.E_PriceModifierType.Recruitment, ResourceManager.E_ResourceType.Gold);
		int num2 = PlayableUnitDatabase.RecruitmentDefinition.UnitCost.EvalToInt(unitToRecruit);
		return Mathf.RoundToInt((float)num2 + (float)num2 * ((float)num * 0.01f));
	}

	public static void ClearCurrentRoster()
	{
		if (TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits == null)
		{
			TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits = new List<PlayableUnit>();
		}
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits.Count; i++)
		{
			if (TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits[i] != null)
			{
				PlayableUnitView.RemoveUsedPortraitColor(TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits[i].PortraitColor);
				PlayableUnitView.RemoveUsedPortrait(TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits[i].PortraitSprite);
			}
		}
		TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits.Clear();
	}

	public static void GenerateNewRoster()
	{
		TPSingleton<RecruitmentView>.Instance.IsGeneratingNewRoster = true;
		ClearCurrentRoster();
		for (int i = 0; i < PlayableUnitDatabase.RecruitmentDefinition.UnitsToGenerate.Count; i++)
		{
			if (PlayableUnitDatabase.RecruitmentDefinition.UnitsToGenerate[i] != null)
			{
				SealedUnitGenerationLevelDefinition sealedUnitGenerationLevelDefinition = PlayableUnitDatabase.RecruitmentDefinition.UnitsToGenerate[i].UnitGenerationLevelDefinition.SealDefinitions[0];
				if (ApocalypseManager.CurrentApocalypse.InnSlotsOverridenRecruitmentLevelId.ContainsKey(i) && PlayableUnitDatabase.UnitGenerationLevelDefinitions.TryGetValue(ApocalypseManager.CurrentApocalypse.InnSlotsOverridenRecruitmentLevelId[i], out var value))
				{
					sealedUnitGenerationLevelDefinition = value.SealDefinitions[0];
				}
				int unitLevel = sealedUnitGenerationLevelDefinition.Level.EvalToInt(TPSingleton<PlayableUnitManager>.Instance.Recruitment);
				string archetypeId = PlayableUnitDatabase.RecruitmentDefinition.UnitsToGenerate[i].PlayableUnitGenerationDefinitionArchetypeIds[RandomManager.GetRandomRange(typeof(RecruitmentController).Name, 0, PlayableUnitDatabase.RecruitmentDefinition.UnitsToGenerate[i].PlayableUnitGenerationDefinitionArchetypeIds.Count)];
				int randomRange = RandomManager.GetRandomRange(typeof(RecruitmentController).Name, PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers.x, PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers.y + 1);
				PlayableUnit item = PlayableUnitManager.GenerateUnit(unitLevel, archetypeId, randomRange, i);
				TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits.Add(item);
			}
		}
		float randomRange2 = RandomManager.GetRandomRange(typeof(RecruitmentController).Name, 0f, 100f);
		TPSingleton<PlayableUnitManager>.Instance.Recruitment.HasMage = randomRange2 <= TPSingleton<PlayableUnitManager>.Instance.Recruitment.MageGenerationCurrentProbability;
		if (TPSingleton<PlayableUnitManager>.Instance.Recruitment.HasMage)
		{
			ResetMageGenerationProbability();
		}
		else
		{
			TPSingleton<PlayableUnitManager>.Instance.Recruitment.MageGenerationCurrentProbability += PlayableUnitDatabase.RecruitmentDefinition.MageGenerationProbabilityIncreasedPerReRoll;
		}
		RecruitmentView.Refresh();
		TPSingleton<RecruitmentView>.Instance.IsGeneratingNewRoster = false;
	}

	public static void InitMageGenerationProbability()
	{
		TPSingleton<PlayableUnitManager>.Instance.Recruitment.MageGenerationCurrentProbability = 100f;
	}

	public static void OpenRecruitmentPanel()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != Construction.E_State.None)
		{
			ConstructionManager.ExitConstructionMode();
		}
		if (TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits == null || TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits.Count == 0)
		{
			GenerateNewRoster();
		}
		TPSingleton<RecruitmentView>.Instance.Open();
		GameController.SetState(Game.E_State.Recruitment);
	}

	public static void PlaceNewUnit(PlayableUnit unitToRecruit)
	{
		if (CanRecruitUnit(unitToRecruit, out var _))
		{
			TPSingleton<PlayableUnitManager>.Instance.Recruitment.SelectedUnit = unitToRecruit;
			TPSingleton<RecruitmentView>.Instance.Close();
			GameController.SetState(Game.E_State.PlaceUnit);
			TPSingleton<PlayableUnitManager>.Instance.SelectedPlayableUnitGhost = unitToRecruit.PlayableUnitView as PlayableUnitGhostView;
		}
	}

	public static void RecruitMage()
	{
		if (CanRecruitMage(out var _))
		{
			TPSingleton<RecruitmentView>.Instance.UnitPlacingDoneThisFrame = true;
			TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - ComputeMageCost());
			TPSingleton<PlayableUnitManager>.Instance.Recruitment.HasMage = false;
			TPSingleton<BuildingManager>.Instance.BuyNewMage();
			RecruitmentView.Unselect();
			RecruitmentView.Refresh();
		}
	}

	public static void RecruitUnit(Tile unitTile)
	{
		TPSingleton<RecruitmentView>.Instance.UnitPlacingDoneThisFrame = true;
		PlayableUnit selectedUnit = TPSingleton<PlayableUnitManager>.Instance.Recruitment.SelectedUnit;
		int num = ComputeUnitCost(selectedUnit);
		TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold - num);
		TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits[TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits.IndexOf(selectedUnit)] = null;
		RecruitmentView.Unselect();
		RecruitmentView.Refresh();
		PlayableUnitManager.InstantiateUnit(selectedUnit, unitTile);
		selectedUnit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.ActionPoints, 0f);
		unitTile.TileController.SetUnit(selectedUnit);
		TPSingleton<PlayableUnitManager>.Instance.Recruitment.SelectedUnit = null;
		TPSingleton<MetaConditionManager>.Instance.IncreaseRecruitedHeroes((int)selectedUnit.Level, num);
		if (selectedUnit.RaceDefinition.Id == "Dwarf")
		{
			TPSingleton<AchievementManager>.Instance.IncreaseAchievementProgression(StatContainer.STAT_DWARVES_RECRUIT_AMOUNT, 1);
		}
		else if (selectedUnit.RaceDefinition.Id == "Elf")
		{
			TPSingleton<AchievementManager>.Instance.IncreaseAchievementProgression(StatContainer.STAT_ELVES_RECRUIT_AMOUNT, 1);
		}
	}

	public static void ResetMageGenerationProbability()
	{
		TPSingleton<PlayableUnitManager>.Instance.Recruitment.MageGenerationCurrentProbability = PlayableUnitDatabase.RecruitmentDefinition.MageGenerationStartProbability;
	}
}

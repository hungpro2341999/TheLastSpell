using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.GoalCondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPostcondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;
using TheLastStand.Definition.Unit.Enemy.TargetingMethod;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy;

public class GoalController
{
	private List<TileObjectSelectionManager.E_Orientation> orientationsToCheck;

	public bool IsOrientationImportant
	{
		get
		{
			if (Goal.Owner is EnemyUnit { GoalComputingStep: IBehaviorModel.E_GoalComputingStep.BeforeMoving } && !Goal.Skill.SkillDefinition.AreaOfEffectDefinition.IsSingleTarget)
			{
				return !Goal.Skill.SkillDefinition.LockAutoOrientation;
			}
			return false;
		}
	}

	public Goal Goal { get; private set; }

	public GoalController(GoalDefinition goalDefinition, IBehaviorModel owner)
	{
		Goal = new Goal(goalDefinition, this, owner);
		if (!SkillManager.TryGetSkillDefinitionOrDatabase(owner.SkillProgressions, goalDefinition.SkillId, owner.ModifiedDayNumber, out var skillDefinition))
		{
			TPSingleton<NightTurnsManager>.Instance.LogError("Skill " + goalDefinition.SkillId + " not found!");
		}
		Goal.Skill = new SkillController(skillDefinition, Goal).Skill;
	}

	public SkillTargetedTileInfo ComputeTarget(Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedDamageables = null)
	{
		if (Goal.Cooldown >= 0)
		{
			return null;
		}
		if (Goal.Owner is TheLastStand.Model.Unit.Unit { IsStunned: not false } && Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.OnDeath && Goal.GoalDefinition.SkillId != "SkipTurn")
		{
			return null;
		}
		if (Goal.Owner.PreventedSkillsIds.Contains(Goal.Skill.SkillDefinition.Id))
		{
			return null;
		}
		if (!CheckPreconditionGroups())
		{
			return null;
		}
		if (IsOrientationImportant)
		{
			orientationsToCheck = new List<TileObjectSelectionManager.E_Orientation>(4)
			{
				TileObjectSelectionManager.E_Orientation.NORTH,
				TileObjectSelectionManager.E_Orientation.EAST,
				TileObjectSelectionManager.E_Orientation.SOUTH,
				TileObjectSelectionManager.E_Orientation.WEST
			};
		}
		else
		{
			orientationsToCheck = null;
		}
		if (Goal.Skill.SkillAction.SkillActionController is SpawnSkillActionController spawnSkillActionController)
		{
			spawnSkillActionController.ComputeUnitsToSpawn();
		}
		Dictionary<SkillTargetedTileInfo, bool> candidateTargetTiles = GetCandidateTargetTiles(alreadyTargetedDamageables);
		if (candidateTargetTiles.Count == 0)
		{
			return null;
		}
		if (!CheckPostconditionGroups(candidateTargetTiles.Keys.ToList()))
		{
			return null;
		}
		return ComputeBestTarget(candidateTargetTiles, alreadyTargetedDamageables);
	}

	private SkillTargetedTileInfo ComputeBestTarget(Dictionary<SkillTargetedTileInfo, bool> candidateTargetTiles, Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedDamageables = null)
	{
		if (candidateTargetTiles.Count < 1)
		{
			return null;
		}
		if (alreadyTargetedDamageables != null)
		{
			TargetingMethodsContainerDefinition targetingMethodsContainer = Goal.GoalDefinition.TargetingMethodsContainer;
			if (targetingMethodsContainer != null && targetingMethodsContainer.AvoidOverkill)
			{
				TPSingleton<NightTurnsManager>.Instance.Log("currentlyTargetedTiles : " + string.Join(" | ", alreadyTargetedDamageables.Select((KeyValuePair<IDamageable, GroupTargetingInfo> x) => string.Format("{0} {1} - [{2}~{3}] : ({4})", x.Key.DamageableType, (x.Key as IEntity)?.UniqueIdentifier, x.Value.MinDamage, x.Value.MaxDamage, string.Join(", ", x.Value.EntitiesIdTargeting)))), CLogLevel.DETAILED);
			}
		}
		SkillTargetedTileInfo skillTargetedTileInfo = FilterCandidatesThroughTargetingMethods(candidateTargetTiles);
		if (alreadyTargetedDamageables != null && skillTargetedTileInfo?.Tile.GetDamageable() != null && Goal.Skill.SkillAction.SkillActionController is AttackSkillActionController attackSkillActionController)
		{
			Vector2Int finalDamageRange = attackSkillActionController.ComputeFinalDamageRange(skillTargetedTileInfo.Tile, Goal.Owner).FinalDamageRange;
			if (alreadyTargetedDamageables.ContainsKey(skillTargetedTileInfo.Tile.GetDamageable()))
			{
				alreadyTargetedDamageables[skillTargetedTileInfo.Tile.GetDamageable()].MinDamage += finalDamageRange.x;
				alreadyTargetedDamageables[skillTargetedTileInfo.Tile.GetDamageable()].MaxDamage += finalDamageRange.y;
				alreadyTargetedDamageables[skillTargetedTileInfo.Tile.GetDamageable()].EntitiesIdTargeting.Add(Goal.Owner.RandomId);
			}
			else
			{
				alreadyTargetedDamageables.Add(skillTargetedTileInfo.Tile.GetDamageable(), new GroupTargetingInfo(finalDamageRange.x, finalDamageRange.y, Goal.Owner.RandomId));
			}
		}
		return skillTargetedTileInfo;
	}

	public void StartTurn()
	{
		Goal.Cooldown = Mathf.Max(Goal.Cooldown - 1, -1);
	}

	private bool CheckPostconditionGroup(List<SkillTargetedTileInfo> candidateTargetTiles, GoalConditionDefinition[] conditionGroup)
	{
		int i = 0;
		for (int num = conditionGroup.Length; i < num; i++)
		{
			GoalConditionDefinition goalConditionDefinition = conditionGroup[i];
			if (CheckPostcondition(candidateTargetTiles, goalConditionDefinition))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckPostconditionGroups(List<SkillTargetedTileInfo> candidateTargetTiles)
	{
		if (Goal.GoalDefinition.PostconditionGroups == null)
		{
			return true;
		}
		int i = 0;
		for (int num = Goal.GoalDefinition.PostconditionGroups.Length; i < num; i++)
		{
			if (!CheckPostconditionGroup(candidateTargetTiles, Goal.GoalDefinition.PostconditionGroups[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckPostcondition(List<SkillTargetedTileInfo> candidateTargetTiles, GoalConditionDefinition goalConditionDefinition)
	{
		if (goalConditionDefinition is TargetsCountCondition targetsCountCondition)
		{
			if (candidateTargetTiles.Count >= targetsCountCondition.Min)
			{
				return candidateTargetTiles.Count <= targetsCountCondition.Max;
			}
			return false;
		}
		TPSingleton<NightTurnsManager>.Instance.LogError("The postcondition has could not be evaluated");
		return false;
	}

	private bool CheckPrecondition(GoalConditionDefinition goalConditionDefinition)
	{
		if (Goal.Owner is TheLastStand.Model.Unit.Unit unit && goalConditionDefinition is CasterHasStatusConditionDefinition casterHasStatusConditionDefinition)
		{
			return unit.StatusOwned.HasFlag(casterHasStatusConditionDefinition.StatusType);
		}
		if (Goal.Owner is IDamageable damageable && goalConditionDefinition is CasterHealthConditionDefinition casterHealthConditionDefinition)
		{
			float num = damageable.Health / damageable.HealthTotal;
			if (num >= casterHealthConditionDefinition.Min)
			{
				return num <= casterHealthConditionDefinition.Max;
			}
			return false;
		}
		IBehaviorModel owner = Goal.Owner;
		if (owner != null && goalConditionDefinition is InterpretedTurnCondition interpretedTurnCondition)
		{
			return interpretedTurnCondition.Expression.EvalToInt(owner.InterpretedTurnConditionContext) == TPSingleton<GameManager>.Instance.Game.CurrentNightHour;
		}
		if (goalConditionDefinition is PlayableUnitCloseToCasterConditionDefinition)
		{
			return Goal.Owner.TileObjectController.GetAdjacentTilesWithDiagonals().Any((Tile x) => x.Unit is PlayableUnit);
		}
		DamageableAroundConditionDefinition damageableAroundConditionDefinition = goalConditionDefinition as DamageableAroundConditionDefinition;
		if (damageableAroundConditionDefinition != null)
		{
			return Goal.Owner.TileObjectController.GetTilesInRange(damageableAroundConditionDefinition.MaxRange, damageableAroundConditionDefinition.MinRange).Count((Tile x) => x.GetDamageable()?.DamageableType == damageableAroundConditionDefinition.DamageableType) >= damageableAroundConditionDefinition.MinAmount;
		}
		if (goalConditionDefinition is NotInFogCondition notInFogCondition)
		{
			int num2 = notInFogCondition.NbTurns?.EvalToInt(Goal) ?? 0;
			if (!(Goal.Owner is EnemyUnit enemyUnit))
			{
				return !Goal.Owner.OriginTile.HasFog;
			}
			if (!Goal.Owner.OriginTile.HasFog)
			{
				return TPSingleton<GameManager>.Instance.Game.CurrentNightHour - enemyUnit.EnemyUnitController.EnemyUnit.LastHourInFog >= num2;
			}
			return false;
		}
		if (goalConditionDefinition is NotInAnyFogCondition notInAnyFogCondition)
		{
			int num3 = notInAnyFogCondition.NbTurns?.EvalToInt(Goal) ?? 0;
			if (!(Goal.Owner is EnemyUnit enemyUnit2))
			{
				return !Goal.Owner.OriginTile.HasAnyFog;
			}
			if (!Goal.Owner.OriginTile.HasAnyFog)
			{
				return TPSingleton<GameManager>.Instance.Game.CurrentNightHour - enemyUnit2.EnemyUnitController.EnemyUnit.LastHourInAnyFog >= num3;
			}
			return false;
		}
		if (goalConditionDefinition is SkillProgressionFlagIsToggledConditionDefinition skillProgressionFlagIsToggledConditionDefinition)
		{
			return TPSingleton<GlyphManager>.Instance.SkillProgressionFlag.HasFlag(skillProgressionFlagIsToggledConditionDefinition.SkillProgressionFlag);
		}
		if (goalConditionDefinition is ApocalypseSkillProgressionFlagIsToggledConditionDefinition apocalypseSkillProgressionFlagIsToggledConditionDefinition)
		{
			return ApocalypseManager.CurrentApocalypse.SkillProgressionFlags.Contains(apocalypseSkillProgressionFlagIsToggledConditionDefinition.SkillProgressionFlag);
		}
		TPSingleton<NightTurnsManager>.Instance.LogError("The precondition has could not be evaluated");
		return false;
	}

	private bool CheckPreconditionGroup(GoalConditionDefinition[] conditionGroup)
	{
		int i = 0;
		for (int num = conditionGroup.Length; i < num; i++)
		{
			GoalConditionDefinition goalConditionDefinition = conditionGroup[i];
			if (CheckPrecondition(goalConditionDefinition))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckPreconditionGroups()
	{
		if (Goal.GoalDefinition.PreconditionGroups == null)
		{
			return true;
		}
		int i = 0;
		for (int num = Goal.GoalDefinition.PreconditionGroups.Length; i < num; i++)
		{
			if (!CheckPreconditionGroup(Goal.GoalDefinition.PreconditionGroups[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckTargetFlagTileCondition(TileFlagDefinition.E_TileFlagTag tileFlagTag, Tile tile, GoalConditionDefinition goalConditionDefinition)
	{
		if (goalConditionDefinition is FlagTagConditionDefinition flagTagConditionDefinition)
		{
			return tileFlagTag == flagTagConditionDefinition.FlagTag;
		}
		if (goalConditionDefinition is TargetInRangeConditionDefinition targetInRangeConditionDefinition)
		{
			int num = TileMapController.DistanceBetweenTiles(Goal.Owner.OriginTile, tile);
			int minEvalToInt = targetInRangeConditionDefinition.GetMinEvalToInt(Goal);
			int maxEvalToInt = targetInRangeConditionDefinition.GetMaxEvalToInt(Goal);
			if (num >= minEvalToInt)
			{
				return num <= maxEvalToInt;
			}
			return false;
		}
		return true;
	}

	private bool CheckTargetFlagTileConditionGroup(TileFlagDefinition.E_TileFlagTag tileFlagTag, Tile tile, GoalTargetTypeDefinition goalTargetTypeDefinition, GoalConditionDefinition[] conditionGroup)
	{
		int i = 0;
		for (int num = conditionGroup.Length; i < num; i++)
		{
			GoalConditionDefinition goalConditionDefinition = conditionGroup[i];
			if (CheckTargetFlagTileCondition(tileFlagTag, tile, goalConditionDefinition))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckTargetFlagTileConditionGroups(TileFlagDefinition.E_TileFlagTag tileFlagTag, Tile tile, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		if (goalTargetTypeDefinition.ConditionGroups == null)
		{
			return true;
		}
		int i = 0;
		for (int num = goalTargetTypeDefinition.ConditionGroups.Length; i < num; i++)
		{
			if (!CheckTargetFlagTileConditionGroup(tileFlagTag, tile, goalTargetTypeDefinition, goalTargetTypeDefinition.ConditionGroups[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckTargetTileObjectCondition(ITileObject targetTileObject, GoalConditionDefinition goalConditionDefinition, TileObjectSelectionManager.E_Orientation skillOrientation)
	{
		if (goalConditionDefinition is TargetHealthConditionDefinition targetHealthConditionDefinition)
		{
			if (!(targetTileObject is IDamageable damageable) || (targetTileObject is TheLastStand.Model.Building.Building building && building.BlueprintModule.IsIndestructible))
			{
				CLoggerManager.Log("Target isn't a damageable, that is unexpected.", LogType.Warning);
				return false;
			}
			float num = damageable.Health / damageable.HealthTotal;
			if (num >= targetHealthConditionDefinition.Min)
			{
				return num <= targetHealthConditionDefinition.Max;
			}
			return false;
		}
		if (goalConditionDefinition is TargetIdConditionDefinition targetIdConditionDefinition)
		{
			if (targetTileObject is PlayableUnit)
			{
				TPSingleton<EnemyUnitManager>.Instance.LogWarning("Don't add TargetIdCondition to TargetType PlayableUnit! --'", CLogLevel.DETAILED);
				return false;
			}
			for (int num2 = targetIdConditionDefinition.TargetIds.Length - 1; num2 >= 0; num2--)
			{
				string text = targetIdConditionDefinition.TargetIds[num2];
				if (targetTileObject.Id == text)
				{
					return !targetIdConditionDefinition.Exclude;
				}
			}
			return targetIdConditionDefinition.Exclude;
		}
		if (goalConditionDefinition is TargetHasBuildingIdConditionDefinition targetHasBuildingIdConditionDefinition)
		{
			if (targetTileObject.OriginTile.Building == null && !targetHasBuildingIdConditionDefinition.Exclude)
			{
				return false;
			}
			for (int num3 = targetHasBuildingIdConditionDefinition.BuildingIds.Length - 1; num3 >= 0; num3--)
			{
				string text2 = targetHasBuildingIdConditionDefinition.BuildingIds[num3];
				if (targetTileObject.OriginTile.Building != null && targetTileObject.OriginTile.Building.Id == text2)
				{
					return !targetHasBuildingIdConditionDefinition.Exclude;
				}
			}
			return targetHasBuildingIdConditionDefinition.Exclude;
		}
		if (goalConditionDefinition is TargetIsNotEliteConditionDefinition)
		{
			return !(targetTileObject is EliteEnemyUnit);
		}
		DamageableCountInAoeConditionDefinition damageableCountInAoe = goalConditionDefinition as DamageableCountInAoeConditionDefinition;
		if (damageableCountInAoe != null)
		{
			List<Tile> affectedTiles = Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetAffectedTiles(targetTileObject.OriginTile, alwaysReturnFullPattern: false, skillOrientation);
			bool shouldCheckCanBeDamaged = Goal.Skill.IsAttackOrExecuteOrSurroundingDamage;
			int num4 = affectedTiles.Count((Tile tile) => tile.GetDamageable() != null && damageableCountInAoe.DamageableTypesToCount.Contains(tile.GetDamageable().DamageableType) && (!shouldCheckCanBeDamaged || tile.GetDamageable() == Goal.Owner || tile.GetDamageable().CanBeDamaged()));
			if (num4 >= damageableCountInAoe.Min)
			{
				return num4 <= damageableCountInAoe.Max;
			}
			return false;
		}
		ExcludeDamageableTypeInAoeConditionDefinition excludeUnitTypeInAoe = goalConditionDefinition as ExcludeDamageableTypeInAoeConditionDefinition;
		if (excludeUnitTypeInAoe != null)
		{
			return !Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetAffectedTiles(targetTileObject.OriginTile, alwaysReturnFullPattern: false, skillOrientation).Any((Tile tile) => tile.Unit != null && excludeUnitTypeInAoe.ExcludeDamageableTypes.Contains(tile.Unit.UnitTemplateDefinition.UnitType));
		}
		GroundCategoryConditionDefinition groundCategoryConditionDefinition = goalConditionDefinition as GroundCategoryConditionDefinition;
		if (groundCategoryConditionDefinition != null)
		{
			return targetTileObject.OccupiedTiles.Any((Tile x) => x.GroundDefinition.GroundCategory == groundCategoryConditionDefinition.GroundCategory);
		}
		TileHasHazardConditionDefinition tileHasHazardConditionDefinition = goalConditionDefinition as TileHasHazardConditionDefinition;
		if (tileHasHazardConditionDefinition != null)
		{
			return targetTileObject.OccupiedTiles.Any((Tile x) => x.HazardOwned.HasFlag(tileHasHazardConditionDefinition.HazardType));
		}
		if (Goal.Owner.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving && goalConditionDefinition is TargetInRangeConditionDefinition targetInRangeConditionDefinition)
		{
			_ = Goal.Owner;
			int minEvalToInt = targetInRangeConditionDefinition.GetMinEvalToInt(Goal);
			int maxEvalToInt = targetInRangeConditionDefinition.GetMaxEvalToInt(Goal);
			foreach (Tile occupiedTile in Goal.Owner.OccupiedTiles)
			{
				foreach (Tile occupiedTile2 in targetTileObject.OccupiedTiles)
				{
					int num5 = TileMapController.DistanceBetweenTiles(occupiedTile, occupiedTile2);
					if (num5 >= minEvalToInt && num5 <= maxEvalToInt)
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	private bool CheckTargetTileObjectConditionGroup(ITileObject targetTileObject, GoalTargetTypeDefinition goalTargetTypeDefinition, GoalConditionDefinition[] conditionGroup, TileObjectSelectionManager.E_Orientation skillOrientation)
	{
		int i = 0;
		for (int num = conditionGroup.Length; i < num; i++)
		{
			GoalConditionDefinition goalConditionDefinition = conditionGroup[i];
			if (CheckTargetTileObjectCondition(targetTileObject, goalConditionDefinition, skillOrientation))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckTargetTileObjectConditionGroups(ITileObject targetTileObject, GoalTargetTypeDefinition goalTargetTypeDefinition, TileObjectSelectionManager.E_Orientation skillOrientation)
	{
		if (goalTargetTypeDefinition.ConditionGroups == null)
		{
			return true;
		}
		int i = 0;
		for (int num = goalTargetTypeDefinition.ConditionGroups.Length; i < num; i++)
		{
			if (!CheckTargetTileObjectConditionGroup(targetTileObject, goalTargetTypeDefinition, goalTargetTypeDefinition.ConditionGroups[i], skillOrientation))
			{
				return false;
			}
		}
		return true;
	}

	private void AddCandidateTargetBuildings(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		EnemyUnit enemyUnit = Goal.Owner as EnemyUnit;
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			TheLastStand.Model.Building.Building building = TPSingleton<BuildingManager>.Instance.Buildings[num];
			if (building.DamageableModule != null && (Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.AfterMoving || Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(building)))
			{
				foreach (Tile occupiedTile in building.OccupiedTiles)
				{
					foreach (TileObjectSelectionManager.E_Orientation item in GetOrientationsToCheck(occupiedTile))
					{
						SkillTargetedTileInfo skillTargetedTileInfo = new SkillTargetedTileInfo(occupiedTile, item);
						if (enemyUnit != null && item != TileObjectSelectionManager.E_Orientation.NONE && enemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
						{
							Tile tileFromTileToOrientation = TileObjectSelectionManager.GetTileFromTileToOrientation(skillTargetedTileInfo.Tile, skillTargetedTileInfo.Orientation, -1);
							if (tileFromTileToOrientation == null || (!(building is MagicCircle) && !enemyUnit.CanStopOn(tileFromTileToOrientation)))
							{
								continue;
							}
						}
						if (building.IsTargetableByAI() && CheckTargetTileObjectConditionGroups(building, goalTargetTypeDefinition, skillTargetedTileInfo.Orientation))
						{
							candidateTargetTiles.Add(skillTargetedTileInfo);
						}
					}
				}
			}
		}
	}

	private void AddCandidateTargetTiles(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		HashSet<Tile> hashSet = new HashSet<Tile>();
		EnemyUnit enemyUnit = Goal.Owner as EnemyUnit;
		if (goalTargetTypeDefinition.ConditionGroups != null)
		{
			_ = Goal.Owner;
			int num = int.MaxValue;
			int num2 = 0;
			GoalConditionDefinition[][] conditionGroups = goalTargetTypeDefinition.ConditionGroups;
			foreach (GoalConditionDefinition[] array in conditionGroups)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] is TargetInRangeConditionDefinition targetInRangeConditionDefinition)
					{
						int minEvalToInt = targetInRangeConditionDefinition.GetMinEvalToInt(Goal);
						int maxEvalToInt = targetInRangeConditionDefinition.GetMaxEvalToInt(Goal);
						if (minEvalToInt < num)
						{
							num = minEvalToInt;
						}
						if (maxEvalToInt > num2)
						{
							num2 = maxEvalToInt;
						}
					}
				}
			}
			hashSet.AddRange(Goal.Owner.TileObjectController.GetTilesInRange(num2, num));
		}
		else
		{
			Goal.Owner.LogWarning("Trying to target a Tile, but no ConditionGroup has been found. Are you SURE you want to do this? It's a high cost goal.");
			hashSet.AddRange(TPSingleton<TileMapManager>.Instance.TileMap.Tiles);
		}
		foreach (Tile item in hashSet)
		{
			if ((Goal.Owner.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.AfterMoving && !Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(item)) || (goalTargetTypeDefinition.IsTileContentAccepted.HasValue && (item.Unit != null || item.Building != null) != goalTargetTypeDefinition.IsTileContentAccepted))
			{
				continue;
			}
			foreach (TileObjectSelectionManager.E_Orientation item2 in GetOrientationsToCheck(item))
			{
				SkillTargetedTileInfo skillTargetedTileInfo = new SkillTargetedTileInfo(item, item2);
				if (enemyUnit != null && item2 != TileObjectSelectionManager.E_Orientation.NONE && enemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
				{
					Tile tileFromTileToOrientation = TileObjectSelectionManager.GetTileFromTileToOrientation(skillTargetedTileInfo.Tile, skillTargetedTileInfo.Orientation, -1);
					if (tileFromTileToOrientation == null || !enemyUnit.CanStopOn(tileFromTileToOrientation))
					{
						continue;
					}
				}
				if (CheckTargetTileObjectConditionGroups(item, goalTargetTypeDefinition, skillTargetedTileInfo.Orientation))
				{
					candidateTargetTiles.Add(skillTargetedTileInfo);
				}
			}
		}
	}

	private void AddCandidateTargetEnemyUnits(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		List<EnemyUnit> list = new List<EnemyUnit>(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count + TPSingleton<BossManager>.Instance.BossUnits.Count);
		list.AddRange(TPSingleton<EnemyUnitManager>.Instance.EnemyUnits);
		list.AddRange(TPSingleton<BossManager>.Instance.BossUnits);
		EnemyUnit enemyUnit = Goal.Owner as EnemyUnit;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			EnemyUnit enemyUnit2 = list[num];
			if (Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.AfterMoving || Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(enemyUnit2))
			{
				Tile tile = ((enemyUnit2.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.BeforeMoving || enemyUnit2 == Goal.Owner || enemyUnit2.TargetTile == null) ? enemyUnit2.OriginTile : enemyUnit2.TargetTile);
				foreach (TileObjectSelectionManager.E_Orientation item in GetOrientationsToCheck(tile))
				{
					SkillTargetedTileInfo skillTargetedTileInfo = new SkillTargetedTileInfo(tile, item);
					if (enemyUnit != null && item != TileObjectSelectionManager.E_Orientation.NONE && enemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
					{
						Tile tileFromTileToOrientation = TileObjectSelectionManager.GetTileFromTileToOrientation(skillTargetedTileInfo.Tile, skillTargetedTileInfo.Orientation, -1);
						if (tileFromTileToOrientation == null || !enemyUnit.CanStopOn(tileFromTileToOrientation))
						{
							continue;
						}
					}
					if (enemyUnit2.IsTargetableByAI() && CheckTargetTileObjectConditionGroups(enemyUnit2, goalTargetTypeDefinition, skillTargetedTileInfo.Orientation))
					{
						candidateTargetTiles.Add(skillTargetedTileInfo);
					}
				}
			}
		}
	}

	private void AddCandidateTargetPlayableUnits(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		EnemyUnit enemyUnit = Goal.Owner as EnemyUnit;
		for (int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num >= 0; num--)
		{
			PlayableUnit playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num];
			if (Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.AfterMoving || Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(playableUnit.OriginTile))
			{
				foreach (TileObjectSelectionManager.E_Orientation item in GetOrientationsToCheck(playableUnit.OriginTile))
				{
					SkillTargetedTileInfo skillTargetedTileInfo = new SkillTargetedTileInfo(playableUnit.OriginTile, item);
					if (enemyUnit != null && item != TileObjectSelectionManager.E_Orientation.NONE && enemyUnit.GoalComputingStep == IBehaviorModel.E_GoalComputingStep.BeforeMoving)
					{
						Tile tileFromTileToOrientation = TileObjectSelectionManager.GetTileFromTileToOrientation(skillTargetedTileInfo.Tile, skillTargetedTileInfo.Orientation, -1);
						if (tileFromTileToOrientation == null || !enemyUnit.CanStopOn(tileFromTileToOrientation))
						{
							continue;
						}
					}
					if (playableUnit.IsTargetableByAI() && CheckTargetTileObjectConditionGroups(playableUnit, goalTargetTypeDefinition, skillTargetedTileInfo.Orientation))
					{
						candidateTargetTiles.Add(skillTargetedTileInfo);
					}
				}
			}
		}
	}

	private void AddCandidateTargetTileFlags(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		foreach (KeyValuePair<TileFlagDefinition.E_TileFlagTag, List<Tile>> item2 in TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag)
		{
			for (int num = item2.Value.Count - 1; num >= 0; num--)
			{
				Tile tile = item2.Value[num];
				if ((Goal.Owner.GoalComputingStep != IBehaviorModel.E_GoalComputingStep.AfterMoving || Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(tile)) && CheckTargetFlagTileConditionGroups(item2.Key, tile, goalTargetTypeDefinition))
				{
					SkillTargetedTileInfo item = new SkillTargetedTileInfo(tile, TileObjectSelectionManager.E_Orientation.NONE);
					candidateTargetTiles.Add(item);
				}
			}
		}
	}

	private void AddCandidateTargets(List<SkillTargetedTileInfo> candidateTargetTiles, GoalTargetTypeDefinition goalTargetTypeDefinition)
	{
		switch (goalTargetTypeDefinition.TargetType)
		{
		case GoalTargetTypeDefinition.E_TargetType.PlayableUnit:
			AddCandidateTargetPlayableUnits(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.EnemyUnit:
			AddCandidateTargetEnemyUnits(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.Building:
			AddCandidateTargetBuildings(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.Tile:
			AddCandidateTargetTiles(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.TileFlag:
			AddCandidateTargetTileFlags(candidateTargetTiles, goalTargetTypeDefinition);
			break;
		case GoalTargetTypeDefinition.E_TargetType.Itself:
		{
			SkillTargetedTileInfo skillTargetedTileInfo = new SkillTargetedTileInfo(Goal.Owner.OriginTile, TileObjectSelectionManager.E_Orientation.NONE);
			if (CheckTargetTileObjectConditionGroups(Goal.Owner.OriginTile, goalTargetTypeDefinition, skillTargetedTileInfo.Orientation))
			{
				candidateTargetTiles.Add(skillTargetedTileInfo);
			}
			break;
		}
		}
	}

	private SkillTargetedTileInfo FilterCandidatesThroughTargetingMethods(Dictionary<SkillTargetedTileInfo, bool> candidateTargetTiles)
	{
		List<SkillTargetedTileInfo> list = ((!Goal.GoalDefinition.TargetingMethodsContainer.AvoidOverkill || !candidateTargetTiles.Any((KeyValuePair<SkillTargetedTileInfo, bool> kvp) => !kvp.Value)) ? candidateTargetTiles.Keys.ToList() : (from kvp in candidateTargetTiles
			where !kvp.Value
			select kvp.Key).ToList());
		for (int num = 0; num < Goal.GoalDefinition.TargetingMethodsContainer.TargetingMethods.Count; num++)
		{
			if (list.Count == 1)
			{
				return list[0];
			}
			TargetingMethodDefinition targetingMethodDefinition = Goal.GoalDefinition.TargetingMethodsContainer.TargetingMethods[num];
			if (targetingMethodDefinition is ClosestTargetingMethodDefinition)
			{
				List<Tuple<SkillTargetedTileInfo, int>> list2 = new List<Tuple<SkillTargetedTileInfo, int>>();
				int minDistance = int.MaxValue;
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					int num3 = TileMapController.DistanceBetweenTiles(list[num2].Tile, Goal.Owner.OriginTile);
					list2.Add(new Tuple<SkillTargetedTileInfo, int>(list[num2], num3));
					if (num3 < minDistance)
					{
						minDistance = num3;
					}
				}
				list = list.Except(from tuple2 in list2
					where tuple2.Item2 > minDistance
					select tuple2.Item1).ToList();
				continue;
			}
			if (targetingMethodDefinition is FarthestTargetingMethodDefinition)
			{
				List<Tuple<SkillTargetedTileInfo, int>> list3 = new List<Tuple<SkillTargetedTileInfo, int>>();
				int maxDistance = int.MinValue;
				for (int num4 = list.Count - 1; num4 >= 0; num4--)
				{
					int num5 = TileMapController.DistanceBetweenTiles(list[num4].Tile, Goal.Owner.OriginTile);
					list3.Add(new Tuple<SkillTargetedTileInfo, int>(list[num4], num5));
					if (num5 > maxDistance)
					{
						maxDistance = num5;
					}
				}
				list = list.Except(from tuple2 in list3
					where tuple2.Item2 < maxDistance
					select tuple2.Item1).ToList();
				continue;
			}
			if (targetingMethodDefinition is FirstTargetTargetingMethodDefinition)
			{
				return list[0];
			}
			OptimalTargetingMethodDefinition optimalTargetingMethodDefinition = targetingMethodDefinition as OptimalTargetingMethodDefinition;
			if (optimalTargetingMethodDefinition != null)
			{
				List<Tuple<SkillTargetedTileInfo, int>> list4 = new List<Tuple<SkillTargetedTileInfo, int>>();
				int maxScore = int.MinValue;
				for (int num6 = list.Count - 1; num6 >= 0; num6--)
				{
					List<Tile> affectedTiles = Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetAffectedTiles(list[num6].Tile, alwaysReturnFullPattern: false, list[num6].Orientation);
					HashSet<IDamageable> hashSet = new HashSet<IDamageable>();
					foreach (Tile item in affectedTiles)
					{
						if (item.GetDamageable() != null)
						{
							hashSet.Add(item.GetDamageable());
						}
					}
					int num7 = 0;
					if (optimalTargetingMethodDefinition.DamageableTypesWeight != null)
					{
						num7 = hashSet.Sum((IDamageable d) => optimalTargetingMethodDefinition.DamageableTypesWeight.TryGetValue(d.DamageableType, out var value) ? value : 0);
					}
					if (optimalTargetingMethodDefinition.DamageableIdsWeight != null)
					{
						foreach (IDamageable item2 in hashSet)
						{
							string text = ((item2 is DamageableModule damageableModule) ? damageableModule.BuildingParent.Id : ((!(item2 is TheLastStand.Model.Unit.Unit unit)) ? null : unit.Id));
							string text2 = text;
							if (text2 == null)
							{
								Goal.Owner.LogError($"Tried to do an optimal goal on {item2} but Id comparison is not setup.");
								continue;
							}
							foreach (var (source, num8) in optimalTargetingMethodDefinition.DamageableIdsWeight)
							{
								if (source.Contains(text2))
								{
									num7 += num8;
								}
							}
						}
					}
					list4.Add(new Tuple<SkillTargetedTileInfo, int>(list[num6], num7));
					if (num7 > maxScore)
					{
						maxScore = num7;
					}
				}
				list = list.Except(from tuple2 in list4
					where tuple2.Item2 < maxScore
					select tuple2.Item1).ToList();
				continue;
			}
			if (targetingMethodDefinition is RandomTargetingMethodDefinition)
			{
				return list[RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, list.Count - 1)];
			}
			if (!(targetingMethodDefinition is ScoreTargetingMethodDefinition scoreTargetingMethodDefinition))
			{
				continue;
			}
			List<Tuple<SkillTargetedTileInfo, float>> list5 = new List<Tuple<SkillTargetedTileInfo, float>>();
			float maxScore2 = float.MinValue;
			for (int num9 = list.Count - 1; num9 >= 0; num9--)
			{
				Goal.GoalInterpreterContext.TargetCandidateTile = list[num9].Tile;
				float num10 = scoreTargetingMethodDefinition.Score.EvalToFloat(Goal.GoalInterpreterContext);
				list5.Add(new Tuple<SkillTargetedTileInfo, float>(list[num9], num10));
				if (num10 > maxScore2)
				{
					maxScore2 = num10;
				}
			}
			list = list.Except(from tuple2 in list5
				where tuple2.Item2 < maxScore2
				select tuple2.Item1).ToList();
		}
		return list[RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, list.Count - 1)];
	}

	private Dictionary<SkillTargetedTileInfo, bool> GetCandidateTargetTiles(Dictionary<IDamageable, GroupTargetingInfo> alreadyTargetedDamageables = null)
	{
		List<SkillTargetedTileInfo> list = new List<SkillTargetedTileInfo>();
		for (int num = Goal.GoalDefinition.GoalTargetTypeDefinitions.Length - 1; num >= 0; num--)
		{
			AddCandidateTargets(list, Goal.GoalDefinition.GoalTargetTypeDefinitions[num]);
		}
		Dictionary<SkillTargetedTileInfo, bool> dictionary = list.ToDictionary((SkillTargetedTileInfo key) => key, (SkillTargetedTileInfo skillTargetedTileInfo) => false);
		for (int num2 = dictionary.Count - 1; num2 >= 0; num2--)
		{
			SkillTargetedTileInfo skillInfo = dictionary.ElementAt(num2).Key;
			if (alreadyTargetedDamageables != null && skillInfo.Tile.GetDamageable() != null && alreadyTargetedDamageables.TryFind((KeyValuePair<IDamageable, GroupTargetingInfo> x) => x.Key == skillInfo.Tile.GetDamageable(), out var value))
			{
				if (value.Value.EntitiesIdTargeting.Contains(Goal.Owner.RandomId) && Goal.Owner is BattleModule battleModule && battleModule.BuildingParent.IsTurret)
				{
					dictionary.Remove(skillInfo);
					continue;
				}
				TargetingMethodsContainerDefinition targetingMethodsContainer = Goal.GoalDefinition.TargetingMethodsContainer;
				if (targetingMethodsContainer != null && targetingMethodsContainer.AvoidOverkill && value.Value.MinDamage >= value.Key.Health + value.Key.Armor)
				{
					dictionary[skillInfo] = true;
				}
			}
			if (Goal.Skill.HasManeuver && !Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.IsManeuverValid(skillInfo.Tile, skillInfo.Orientation))
			{
				Goal.Owner.Log($"Removed {skillInfo.Tile}:{skillInfo.Orientation} from candidate target tiles. Reason: Invalid Maneuver", CLogLevel.NORMAL, forcePrintInUnity: true);
				dictionary.Remove(skillInfo);
			}
			else if (!Goal.GoalDefinition.GoalTargetTypeDefinitions.Any((GoalTargetTypeDefinition x) => x.TargetType == GoalTargetTypeDefinition.E_TargetType.Itself) && Goal.Skill.IsAttackOrExecuteOrSurroundingDamage && !Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.GetAffectedTiles(skillInfo.Tile, alwaysReturnFullPattern: false, skillInfo.Orientation).Any((Tile tile) => tile.GetDamageable() != null && (tile.GetDamageable() == Goal.Owner || tile.GetDamageable().CanBeDamaged())))
			{
				Goal.Owner.Log($"Removed {skillInfo.Tile}:{skillInfo.Orientation} from candidate target tiles. Reason: Not Damaging any target in aoe", CLogLevel.NORMAL, forcePrintInUnity: true);
				dictionary.Remove(skillInfo);
			}
			else if (Goal.Skill.SkillAction.SkillActionController is SpawnSkillActionController spawnSkillActionController && !spawnSkillActionController.ValidateCandidateTargetTile(skillInfo.Tile))
			{
				dictionary.Remove(skillInfo);
			}
		}
		return dictionary;
	}

	private List<TileObjectSelectionManager.E_Orientation> GetOrientationsToCheck(Tile tile)
	{
		return orientationsToCheck ?? new List<TileObjectSelectionManager.E_Orientation>(1) { Goal.Skill.TileDependantOrientation(tile) };
	}
}

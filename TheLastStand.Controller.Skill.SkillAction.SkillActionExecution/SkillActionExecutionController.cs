using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.CastFx;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Maths;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.CastFx;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public abstract class SkillActionExecutionController
{
	private List<SkillTargetedTileInfo> savedTargets = new List<SkillTargetedTileInfo>();

	public bool HasTargets => SkillActionExecution.TargetTiles.Count > 0;

	public TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecution SkillActionExecution { get; protected set; }

	public SkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
	{
	}

	public static bool CheckAndUpdateLineOfSight(Tile tile, List<Tile> sourceTiles, Vector2Int position, int maxRange, HashSet<Tile> obstacleTiles, HashSet<Tile> excludeTiles, bool hasVisionOnCityTiles)
	{
		if (!sourceTiles.Any((Tile sourceTile) => IsInLineOfSight(tile, sourceTile, obstacleTiles)))
		{
			bool flag = tile.IsCityTile && hasVisionOnCityTiles;
			if (sourceTiles.Count == 1)
			{
				ExcludeTiles(tile, position, maxRange, obstacleTiles, excludeTiles, hasVisionOnCityTiles);
			}
			else
			{
				if (IsBlockingLineOfSight(tile))
				{
					obstacleTiles.Add(tile);
				}
				if (!flag)
				{
					excludeTiles.Add(tile);
				}
			}
			if (flag)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool CheckAndUpdateLineOfSight(Tile tile, Tile sourceTile, Vector2Int position, int maxRange, HashSet<Tile> obstacleTiles, HashSet<Tile> excludeTiles)
	{
		if (!IsInLineOfSight(tile, sourceTile, obstacleTiles))
		{
			ExcludeTiles(tile, position, maxRange, obstacleTiles, excludeTiles);
			return false;
		}
		return true;
	}

	public static void ExcludeTiles(Tile tile, Vector2Int position, int maxRange, HashSet<Tile> obstacleTiles, HashSet<Tile> excludeTiles, bool hasVisionOnCityTiles = false)
	{
		int num = Maths.GCD(position.x, position.y);
		Vector2Int vector2Int = new Vector2Int(position.x / num, position.y / num);
		Vector2Int vector2Int2 = vector2Int;
		while (Mathf.Abs(position.x + vector2Int2.x) + Mathf.Abs(position.y + vector2Int2.y) <= maxRange)
		{
			Tile tile2 = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(tile.Position.x + vector2Int2.x, tile.Position.y + vector2Int2.y);
			if (tile2 == null)
			{
				break;
			}
			if (IsBlockingLineOfSight(tile2))
			{
				obstacleTiles.Add(tile2);
			}
			if (!(tile2.IsCityTile && hasVisionOnCityTiles))
			{
				excludeTiles.Add(tile2);
			}
			vector2Int2 = new Vector2Int(vector2Int2.x + vector2Int.x, vector2Int2.y + vector2Int.y);
		}
	}

	public static bool HasAtLeastOneTileInLineOfSight(Tile[] destinationTiles, Tile sourceTile, HashSet<Tile> obstacleTiles)
	{
		for (int i = 0; i < destinationTiles.Length; i++)
		{
			if (IsInLineOfSight(destinationTiles[i], sourceTile, obstacleTiles))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBlockingLineOfSight(Tile tile)
	{
		if (tile.Building != null && tile.Building.BlueprintModule.IsStoppingLineOfSight && TPSingleton<SkillManager>.Instance.LineOfSight.BuildingsBlocking.Contains(tile.Building.BuildingDefinition.Id))
		{
			return true;
		}
		if (tile.Unit is EnemyUnit enemyUnit && SkillDatabase.LineOfSightDefinition.EnemyUnitsBlocking != null && SkillDatabase.LineOfSightDefinition.EnemyUnitsBlocking.Contains(enemyUnit.EnemyUnitTemplateDefinition.Id))
		{
			return true;
		}
		if (tile.Unit is PlayableUnit && ApocalypseManager.CurrentApocalypse.PlayableUnitBlockLineOfSight)
		{
			return true;
		}
		return false;
	}

	public static bool IsInLineOfSight(Tile destinationTile, Tile sourceTile, HashSet<Tile> obstacleTiles)
	{
		TileController.ComputeSegmentEndsPositions(sourceTile, destinationTile, SkillManager.LineOfSightTargeting, out var sourcePos, out var destPos);
		foreach (Tile obstacleTile in obstacleTiles)
		{
			if (destinationTile != obstacleTile && obstacleTile.TileController.SegmentTileIntersection(sourcePos, destPos, SkillManager.LineOfSightTolerance))
			{
				return false;
			}
		}
		return true;
	}

	public void AddPropagationAffectedUnit(int index, TheLastStand.Model.Unit.Unit unit)
	{
		GetPropagationAffectedUnits(index).Add(unit);
	}

	public void AddTarget(Tile tile, TileObjectSelectionManager.E_Orientation orientation, bool isMirroredOnYAxis = false)
	{
		AddTarget(new SkillTargetedTileInfo(tile, orientation, isMirroredOnYAxis), refreshSkillEffectFeedback: true);
	}

	public void AddTarget(SkillTargetedTileInfo targetedTileInfo, bool refreshSkillEffectFeedback)
	{
		SkillActionExecution.TargetTiles.Add(targetedTileInfo);
		if (!SkillActionExecution.Skill.SkillAction.HasEffect("MultiHit"))
		{
			return;
		}
		Tile tile = targetedTileInfo.Tile;
		if (refreshSkillEffectFeedback)
		{
			SkillManager.SkillEffectFeedback.Refresh();
		}
		List<Tile> affectedTiles = GetAffectedTiles(tile, alwaysReturnFullPattern: false, targetedTileInfo.Orientation, targetedTileInfo.IsMirroredOnYAxis);
		List<Tile> surroundingTiles = GetSurroundingTiles(tile, targetedTileInfo.Orientation, targetedTileInfo.IsMirroredOnYAxis);
		affectedTiles.AddRange(surroundingTiles);
		foreach (Tile item in affectedTiles)
		{
			SkillManager.AddMultiHitTargetHUD(item);
		}
		SkillActionExecution.MultiHitTargetedTiles.Add(affectedTiles);
	}

	public void ComputeSkillRangeTiles(bool updateView = true)
	{
		SkillActionExecution.InRangeTiles.Clear();
		SkillActionExecution.TileInaccurates.Clear();
		bool flag = SkillManager.SelectedSkill == SkillActionExecution.Skill;
		if (SkillActionExecution.Skill.SkillDefinition.InfiniteRange)
		{
			SkillActionExecution.InRangeTiles.InfiniteRange = true;
			if (SkillActionExecution.Skill.Targets == null)
			{
				return;
			}
			for (int num = SkillActionExecution.Skill.Targets.Count - 1; num >= 0; num--)
			{
				ITileObject tileObject = SkillActionExecution.Skill.Targets[num];
				foreach (Tile occupiedTile in tileObject.OccupiedTiles)
				{
					SkillActionExecution.InRangeTiles.Range.Add(occupiedTile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: true, TileObjectSelectionManager.GetOrientationFromSelectionToTile(tileObject.OriginTile), flag));
				}
			}
			SkillActionExecution.Caster.SkillCasterController.FilterTilesInRange(SkillActionExecution.InRangeTiles, SkillActionExecution.SkillSourceTiles);
			if (!updateView)
			{
				return;
			}
			foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> item in SkillActionExecution.InRangeTiles.Range)
			{
				Tile key = item.Key;
				TileMapView.SetTile(TileMapView.SkillRangeTilemap, key, "View/Tiles/Feedbacks/Skill/SkillRange");
				TileMapView.SetTileColor(TileMapView.SkillRangeTilemap, key, item.Value.TileColor);
			}
			bool flag2 = (SkillActionExecution.Skill.SkillDefinition.CanFlip || SkillManager.DebugSkillsForceCanFlip) && flag;
			if ((SkillActionExecution.Skill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate) && flag)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayDialsTilesFrom(SkillActionExecution.Caster.OriginTile, SkillActionExecution.InRangeTiles.Range);
				if (TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT))
				{
					flag2 = false;
					TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, TPSingleton<GameManager>.Instance.Game.Cursor.Tile, "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_RotationSkill_On");
				}
			}
			if (flag2 && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null)
			{
				TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, TPSingleton<GameManager>.Instance.Game.Cursor.Tile, TileMapView.GetSkillFlipIconPathFromOrientation(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection));
			}
			return;
		}
		int maxRange = ComputeSkillMaxRange();
		bool requiresLineOfSight = RequiresLineOfSight();
		bool flag3 = RequiresLineOfSightInCityTiles();
		bool hasInaccurateEffect = HasInaccurateEffect();
		foreach (KeyValuePair<Tile, Tile> item2 in SkillActionExecution.SkillSourceTileObject.TileObjectController.GetTilesInRangeWithClosestOccupiedTile(maxRange, 0, SkillActionExecution.Skill.SkillDefinition.CardinalDirectionOnly))
		{
			bool hasVisionOnCityTiles = !flag3;
			AddSkillRangeTile(distance: new Vector2Int(item2.Key.X - item2.Value.X, item2.Key.Y - item2.Value.Y), tile: item2.Key, maxRange: maxRange, requiresLineOfSight: requiresLineOfSight, hasVisionOnCityTiles: hasVisionOnCityTiles, hasInaccurateEffect: hasInaccurateEffect, updateView: updateView);
		}
		SkillActionExecution.InRangeTiles.ClearLonelyTilesInLineOfSight(SkillActionExecution.SkillSourceTile, SkillActionExecution.Skill.SkillDefinition.Range.x, maxRange);
		SkillActionExecution.Caster.SkillCasterController.FilterTilesInRange(SkillActionExecution.InRangeTiles, SkillActionExecution.SkillSourceTiles);
		if (!updateView)
		{
			return;
		}
		if (flag)
		{
			(SkillActionExecution.Caster as TheLastStand.Model.Unit.Unit)?.UnitController.LookAtDirection(TileObjectSelectionManager.GetDirectionFromOrientation(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection));
			bool flag4 = SkillActionExecution.Skill.SkillDefinition.CanFlip || SkillManager.DebugSkillsForceCanFlip;
			if (SkillActionExecution.Skill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayDialsTilesFrom(SkillActionExecution.Caster.OriginTile, SkillActionExecution.InRangeTiles.Range);
				if (TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT))
				{
					flag4 = false;
					TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, TPSingleton<GameManager>.Instance.Game.Cursor.Tile, "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_RotationSkill_On");
				}
			}
			if (flag4 && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null)
			{
				TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, TPSingleton<GameManager>.Instance.Game.Cursor.Tile, TileMapView.GetSkillFlipIconPathFromOrientation(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection));
			}
		}
		TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayRangeTiles(SkillActionExecution.InRangeTiles.Range);
		TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayInaccurateRangeTiles(SkillActionExecution.TileInaccurates);
	}

	public void DisplayInRangeTiles()
	{
		SkillActionExecution.InRangeTiles.Clear();
		SkillActionExecution.TileInaccurates.Clear();
		if (SkillActionExecution.Skill.SkillDefinition.InfiniteRange)
		{
			return;
		}
		int maxRange = ComputeSkillMaxRange();
		bool requiresLineOfSight = RequiresLineOfSight();
		bool flag = RequiresLineOfSightInCityTiles();
		bool hasInaccurateEffect = HasInaccurateEffect();
		foreach (KeyValuePair<Tile, Tile> item in SkillActionExecution.SkillSourceTileObject.TileObjectController.GetTilesInRangeWithClosestOccupiedTile(maxRange, 0, SkillActionExecution.Skill.SkillDefinition.CardinalDirectionOnly))
		{
			bool hasVisionOnCityTiles = !flag;
			AddSkillRangeTile(distance: new Vector2Int(item.Key.X - item.Value.X, item.Key.Y - item.Value.Y), tile: item.Key, maxRange: maxRange, requiresLineOfSight: requiresLineOfSight, hasVisionOnCityTiles: hasVisionOnCityTiles, hasInaccurateEffect: hasInaccurateEffect);
		}
		SkillActionExecution.InRangeTiles.ClearLonelyTilesInLineOfSight(SkillActionExecution.SkillSourceTile, SkillActionExecution.Skill.SkillDefinition.Range.x, maxRange);
		TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayRangeTiles(SkillActionExecution.InRangeTiles.Range);
	}

	public void ExecuteSkill()
	{
		if (SkillActionExecution.Skill.OverallUsesRemaining > 0)
		{
			SkillActionExecution.Skill.OverallUsesRemaining = Mathf.Max(SkillActionExecution.Skill.OverallUsesRemaining - 1, 0);
		}
		if (SkillActionExecution.Skill.UsesPerTurnCountFromDefinition != -1)
		{
			SkillActionExecution.Skill.SetUsesPerTurnRemaining(SkillActionExecution.Skill.UsesPerTurnRemaining - 1, applyChangeToLinkedSkills: true);
		}
		SkillActionExecution.Caster.SkillCasterController.PaySkillCost(SkillActionExecution.Skill);
		if (SkillActionExecution.Caster is PlayableUnit playableUnit)
		{
			if (playableUnit.WillDieByPoison)
			{
				playableUnit.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: false);
			}
			TPSingleton<BarkManager>.Instance.CheckPlayableUnitOutOfResources(playableUnit, SkillActionExecution.Skill);
			if (SkillActionExecution.Skill.IsPunch)
			{
				playableUnit.LifetimeStats.LifetimeStatsController.IncreasePunchesUsed();
			}
			if (SkillActionExecution.Skill.IsJump)
			{
				playableUnit.LifetimeStats.LifetimeStatsController.IncreaseJumpsOverWallUsed();
			}
			if (SkillActionExecution.Skill.SkillContainer is TheLastStand.Model.Item.Item item)
			{
				playableUnit.LifetimeStats.LifetimeStatsController.IncreaseSkillUses(item.ItemDefinition.Id, SkillActionExecution.Skill.ActionPointsCost);
				if (item.ItemDefinition.Category == ItemDefinition.E_Category.Scroll)
				{
					TPSingleton<AchievementManager>.Instance.IncreaseAchievementProgression(StatContainer.STAT_SCROLL_USES_AMOUNT, 1);
				}
			}
		}
		SkillActionExecution.SkillExecutionView.Clear();
		TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearRangedSkillsModifiers();
		SkillManager.SkillEffectFeedback.Hide();
		SkillActionExecution.Caster.SkillExecutionCoroutine = TPSingleton<SkillManager>.Instance.StartCoroutine(SkillExecutionCoroutine());
		TPSingleton<TileObjectSelectionManager>.Instance.HasToWaitForNextFrame = true;
	}

	public void ForgetSavedTargets()
	{
		savedTargets.Clear();
	}

	public bool HasAtLeastOneTileInLineOfSight(Tile[] destinationTiles, Tile sourceTile)
	{
		return HasAtLeastOneTileInLineOfSight(destinationTiles, sourceTile, SkillActionExecution.InRangeTiles.Obstacle);
	}

	public void HideSkillTargetingForTargets()
	{
		if (SkillActionExecution.Skill.Targets == null || SkillActionExecution.Skill.Targets.Count == 0)
		{
			return;
		}
		foreach (ITileObject target in SkillActionExecution.Skill.Targets)
		{
			target.TileObjectView.ToggleSkillTargeting(display: false);
		}
	}

	public bool IsInLineOfSight(Tile destinationTile, List<Tile> sourceTiles)
	{
		return sourceTiles.Any((Tile sourceTile) => IsInLineOfSight(destinationTile, sourceTile, SkillActionExecution.InRangeTiles.Obstacle));
	}

	public bool IsInLineOfSight(Tile destinationTile, Tile sourceTile = null)
	{
		return IsInLineOfSight(destinationTile, sourceTile ?? SkillActionExecution.SkillSourceTile, SkillActionExecution.InRangeTiles.Obstacle);
	}

	public bool IsManeuverValid(Tile targetTile, TileObjectSelectionManager.E_Orientation specificOrientation)
	{
		if (!SkillActionExecution.Skill.HasManeuver)
		{
			return true;
		}
		Tile maneuverTile = GetManeuverTile(targetTile, specificOrientation);
		if (maneuverTile != null && !maneuverTile.HasFog)
		{
			return (SkillActionExecution.Caster as TheLastStand.Model.Unit.Unit)?.CanStopOn(maneuverTile) ?? false;
		}
		return false;
	}

	public virtual bool IsTileAffected(Tile tile)
	{
		if (tile == null)
		{
			return false;
		}
		IDamageable damageable = tile.GetDamageable(SkillActionExecution.Skill.SkillAction.SkillActionController.CanTargetUnitOverBuilding(tile));
		if (tile.CanAffectThroughFog(SkillActionExecution.Caster))
		{
			if (SkillActionExecution.Skill.HasExtinguish)
			{
				TheLastStand.Model.Building.Building building = tile.Building;
				if (building != null && building.IsBrazier)
				{
					goto IL_00a9;
				}
			}
			if (!(SkillActionExecution.Skill.SkillAction is QuitWatchtowerSkillAction))
			{
				return SkillActionExecution.Skill.SkillDefinition.AffectedUnits.ShouldDamageableBeAffected(SkillActionExecution.Caster, damageable, SkillActionExecution.Skill);
			}
			goto IL_00a9;
		}
		return false;
		IL_00a9:
		return true;
	}

	public virtual List<Tile> GetAffectedTiles(Tile targetTile, bool alwaysReturnFullPattern, TileObjectSelectionManager.E_Orientation specificOrientation, bool mirroredOnYAxis = false)
	{
		AreaOfEffectDefinition areaOfEffectDefinition = SkillActionExecution.Skill.SkillDefinition.AreaOfEffectDefinition;
		List<Tile> list = new List<Tile>(areaOfEffectDefinition.Pattern.Count * areaOfEffectDefinition.Pattern[0].Count);
		int angleFromOrientation = TileObjectSelectionManager.GetAngleFromOrientation(specificOrientation);
		int i = 0;
		for (int count = areaOfEffectDefinition.Pattern.Count; i < count; i++)
		{
			int j = 0;
			for (int count2 = areaOfEffectDefinition.Pattern[i].Count; j < count2; j++)
			{
				if (areaOfEffectDefinition.Pattern[i][j] == 'X')
				{
					Vector2Int vector2Int = new Vector2Int(j - areaOfEffectDefinition.Origin.x, i - areaOfEffectDefinition.Origin.y);
					if (mirroredOnYAxis)
					{
						vector2Int = vector2Int.FlipX();
					}
					Vector2Int rotatedTilemapPosition = TileMapController.GetRotatedTilemapPosition(vector2Int, targetTile.Position, angleFromOrientation);
					Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(rotatedTilemapPosition.x, rotatedTilemapPosition.y);
					if (tile != null && (alwaysReturnFullPattern || IsTileAffected(tile)))
					{
						list.Add(tile);
					}
				}
			}
		}
		return list;
	}

	public Tile GetManeuverTile(Tile targetTile, TileObjectSelectionManager.E_Orientation specificOrientation, bool mirroredOnYAxis = false)
	{
		AreaOfEffectDefinition areaOfEffectDefinition = SkillActionExecution.Skill.SkillDefinition.AreaOfEffectDefinition;
		int angleFromOrientation = TileObjectSelectionManager.GetAngleFromOrientation(specificOrientation);
		int i = 0;
		for (int count = areaOfEffectDefinition.Pattern.Count; i < count; i++)
		{
			int j = 0;
			for (int count2 = areaOfEffectDefinition.Pattern[i].Count; j < count2; j++)
			{
				if (areaOfEffectDefinition.Pattern[i][j] == 'M')
				{
					Vector2Int vector2Int = new Vector2Int(j - areaOfEffectDefinition.Origin.x, i - areaOfEffectDefinition.Origin.y);
					if (mirroredOnYAxis)
					{
						vector2Int = vector2Int.FlipX();
					}
					Vector2Int rotatedTilemapPosition = TileMapController.GetRotatedTilemapPosition(vector2Int, targetTile.Position, angleFromOrientation);
					return TPSingleton<TileMapManager>.Instance.TileMap.GetTile(rotatedTilemapPosition.x, rotatedTilemapPosition.y);
				}
			}
		}
		return null;
	}

	public List<TheLastStand.Model.Unit.Unit> GetPropagationAffectedUnits(int index)
	{
		if (SkillActionExecution.PropagationAffectedUnits == null)
		{
			SkillActionExecution.PropagationAffectedUnits = new Dictionary<int, List<TheLastStand.Model.Unit.Unit>>();
		}
		if (!SkillActionExecution.PropagationAffectedUnits.TryGetValue(index, out var value))
		{
			value = new List<TheLastStand.Model.Unit.Unit>();
			SkillActionExecution.PropagationAffectedUnits.Add(index, value);
		}
		return value;
	}

	public List<Tile> GetSurroundingTiles(Tile targetTile, TileObjectSelectionManager.E_Orientation specificOrientation, bool mirroredOnYAxis = false)
	{
		List<Tile> list = new List<Tile>();
		AreaOfEffectDefinition areaOfEffectDefinition = SkillActionExecution.Skill.SkillDefinition.AreaOfEffectDefinition;
		int angleFromOrientation = TileObjectSelectionManager.GetAngleFromOrientation(specificOrientation);
		int i = 0;
		for (int count = areaOfEffectDefinition.Pattern.Count; i < count; i++)
		{
			int j = 0;
			for (int count2 = areaOfEffectDefinition.Pattern[i].Count; j < count2; j++)
			{
				if (areaOfEffectDefinition.Pattern[i][j] == 'e')
				{
					Vector2Int vector2Int = new Vector2Int(j - areaOfEffectDefinition.Origin.x, i - areaOfEffectDefinition.Origin.y);
					if (mirroredOnYAxis)
					{
						vector2Int = vector2Int.FlipX();
					}
					Vector2Int rotatedTilemapPosition = TileMapController.GetRotatedTilemapPosition(vector2Int, targetTile.Position, angleFromOrientation);
					list.Add(TPSingleton<TileMapManager>.Instance.TileMap.GetTile(rotatedTilemapPosition.x, rotatedTilemapPosition.y));
				}
			}
		}
		return list;
	}

	public float PlayPreCastFxs()
	{
		if (SkillActionExecution.PreCastFx?.CastFxDefinition == null)
		{
			return 0f;
		}
		SkillActionExecution.PreCastFx.TargetTile = SkillActionExecution.SkillSourceTile;
		SetCastFxAffectedTiles(SkillActionExecution.PreCastFx, null, null, TileObjectSelectionManager.E_Orientation.NONE, mirroredOnYAxis: false, includeSkillEffects: false);
		SkillActionExecution.PreCastFx.CastFxController.PlayCastFxs(TileObjectSelectionManager.E_Orientation.NONE, (SkillActionExecution.Caster as EnemyUnit)?.EnemyUnitTemplateDefinition.VisualOffset ?? Vector2.zero, SkillActionExecution.Caster);
		return SkillActionExecution.PreCastFx.CastFxDefinition.CastTotalDuration.EvalToFloat(SkillActionExecution.PreCastFx.CastFXInterpreterContext);
	}

	public void PrepareSkill(ISkillCaster caster, Tile originTile = null)
	{
		SkillActionExecution.SkillExecutionController.Reset();
		SkillActionExecution.Caster = caster;
		SkillActionExecution.SkillSourceTileObject = originTile;
		if (SkillActionExecution.CastFx != null)
		{
			SkillActionExecution.CastFx.SourceTile = SkillActionExecution.SkillSourceTile;
		}
		if (SkillActionExecution.PreCastFx != null)
		{
			SkillActionExecution.PreCastFx.SourceTile = SkillActionExecution.SkillSourceTile;
		}
		bool flag = SkillActionExecution.Caster is PlayableUnit || (SkillActionExecution.Caster is BattleModule battleModule && battleModule.BuildingParent.IsHandledDefense);
		ComputeSkillRangeTiles(flag);
		SkillActionExecution.Skill.SkillController.ComputeTargetsAndValidity(SkillActionExecution.Caster, flag);
		if (flag)
		{
			TheLastStand.Model.Skill.SkillAction.SkillAction skillAction = SkillActionExecution.Skill.SkillAction;
			if (skillAction is AttackSkillAction attackSkillAction)
			{
				if (attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Ranged && !attackSkillAction.HasEffect("NoDodge"))
				{
					TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayRangedSkillsModifiers(SkillActionExecution.SkillSourceTileObject, SkillActionExecution.Skill);
				}
			}
			else if (skillAction is ResupplySkillAction resupplySkillAction)
			{
				resupplySkillAction.ResupplySkillActionExecution.ResupplySkillActionExecutionView.DisplayTargetingBuildingsHUD();
			}
			TileObjectSelectionManager.UpdateCursorOrientationFromSelection(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
			SkillManager.SkillEffectFeedback.Display();
		}
		SkillActionExecution.HasBeenPrepared = true;
	}

	public void RemoveLastTarget()
	{
		int num = SkillActionExecution.TargetTiles.Count - 1;
		SkillActionExecution.TargetTiles.RemoveAt(num);
		if (SkillActionExecution.Skill.SkillAction.HasEffect("MultiHit") && SkillActionExecution.MultiHitTargetedTiles.Count > 0 && num < SkillActionExecution.MultiHitTargetedTiles.Count)
		{
			foreach (Tile item in SkillActionExecution.MultiHitTargetedTiles[num])
			{
				SkillManager.RemoveMultiHitTargetHUD(item);
			}
			SkillActionExecution.MultiHitTargetedTiles.RemoveAt(num);
		}
		SkillManager.SkillEffectFeedback.Refresh();
	}

	public virtual void Reset()
	{
		if (!SkillActionExecution.HasBeenPrepared)
		{
			return;
		}
		SkillActionExecution.Skill.SkillAction.SkillActionController.ResetPerkData();
		TPSingleton<PlayableUnitManagementView>.Instance.RefreshModifiersLayoutView();
		SkillActionExecution.SkillExecutionView.Clear();
		if (SkillActionExecution.MultiHitTargetedTiles.Count > 0)
		{
			foreach (List<Tile> multiHitTargetedTile in SkillActionExecution.MultiHitTargetedTiles)
			{
				foreach (Tile item in multiHitTargetedTile)
				{
					SkillManager.RemoveMultiHitTargetHUD(item);
				}
			}
		}
		SkillActionExecution.Caster = null;
		SkillActionExecution.SkillSourceTileObject = null;
		if (SkillActionExecution.CastFx != null)
		{
			SkillActionExecution.CastFx.SourceTile = null;
		}
		if (SkillActionExecution.Skill.SkillAction is ResupplySkillAction resupplySkillAction && (resupplySkillAction.HasEffect("ResupplyCharges") || resupplySkillAction.HasEffect("ResupplyOverallUses")))
		{
			resupplySkillAction.ResupplySkillActionExecution.ResupplySkillActionExecutionView.HideDisplayedHUD();
		}
		SkillManager.SkillEffectFeedback.Hide();
		SkillActionExecution.TargetTiles.Clear();
		SkillActionExecution.MultiHitTargetedTiles.Clear();
		TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearRangedSkillsModifiers();
		SkillActionExecution.AllResultData.Clear();
		if (SkillActionExecution.PropagationAffectedUnits != null)
		{
			foreach (KeyValuePair<int, List<TheLastStand.Model.Unit.Unit>> propagationAffectedUnit in SkillActionExecution.PropagationAffectedUnits)
			{
				TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.PropagationBounces, propagationAffectedUnit.Value.Count);
			}
			SkillActionExecution.PropagationAffectedUnits.Clear();
		}
		SkillActionExecution.HasBeenPrepared = false;
	}

	public void RestoreTargets()
	{
		if (savedTargets.Count == 0)
		{
			return;
		}
		foreach (SkillTargetedTileInfo savedTarget in savedTargets)
		{
			AddTarget(savedTarget, refreshSkillEffectFeedback: false);
		}
		SkillManager.SkillEffectFeedback.Refresh();
	}

	public void SaveTargets()
	{
		if (SkillActionExecution.TargetTiles.Count != 0)
		{
			savedTargets = new List<SkillTargetedTileInfo>(SkillActionExecution.TargetTiles);
		}
	}

	protected virtual List<SkillActionResultDatas> ApplyEffects(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, List<Tile> affectedTiles, List<Tile> surroundingTiles)
	{
		return skill.SkillAction.SkillActionController.ApplyEffect(caster, affectedTiles, surroundingTiles);
	}

	protected virtual void ApplyEffectsAfterFXs(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, List<SkillActionResultDatas> resultData, List<Tile> affectedTiles, List<Tile> surroundingTiles)
	{
	}

	protected virtual void ApplySkillEffects(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, Tile currentTargetTile, List<SkillActionResultDatas> resultData, TileObjectSelectionManager.E_Orientation specificOrientation)
	{
		ExileSkillExecution(caster, skill);
		if (caster is TheLastStand.Model.Unit.Unit caster2)
		{
			ManeuverSkillExecution(caster2, skill, currentTargetTile, specificOrientation);
		}
	}

	protected void ExileSkillExecution(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill)
	{
		ExileCasterEffectDefinition firstEffect = skill.SkillAction.GetFirstEffect<ExileCasterEffectDefinition>("ExileCaster");
		if (caster is TheLastStand.Model.Unit.Unit unit && firstEffect != null)
		{
			if (unit is EnemyUnit)
			{
				TrophyManager.AppendValueToTrophiesConditions<EnemiesKilledWithoutAttackTrophyConditionController>(new object[1] { 1 });
			}
			unit.UnitController.PrepareForExile(firstEffect.ForcePlayDieAnim);
		}
	}

	protected virtual float PlayCastFxs(TheLastStand.Model.CastFx.CastFx castFx, ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, Tile currentTargetTile, List<Tile> affectedTiles, TileObjectSelectionManager.E_Orientation currentTargetTileOrientation, bool mirroredOnYAxis = false, bool includeSkillEffects = true)
	{
		if (castFx?.CastFxDefinition != null)
		{
			TheLastStand.Model.Unit.Unit unit = caster as TheLastStand.Model.Unit.Unit;
			PlaySkillCastAnim(unit);
			Vector2 offset = (unit as EnemyUnit)?.EnemyUnitTemplateDefinition.VisualOffset ?? Vector2.zero;
			SetCastFxAffectedTiles(castFx, currentTargetTile, affectedTiles, currentTargetTileOrientation, mirroredOnYAxis);
			TileObjectSelectionManager.E_Orientation specificOrientation = currentTargetTileOrientation;
			if (caster is EnemyUnit enemyUnit && enemyUnit.EnemyUnitTemplateDefinition.LockedOrientation != GameDefinition.E_Direction.None)
			{
				specificOrientation = TileObjectSelectionManager.GetOrientationFromDirection(enemyUnit.EnemyUnitTemplateDefinition.LockedOrientation);
			}
			castFx.CastFxController.PlayCastFxs(specificOrientation, offset, skill.SkillAction.SkillActionExecution.Caster, mirroredOnYAxis);
			float num = castFx.CastFxDefinition.CastTotalDuration.EvalToFloat(castFx.CastFXInterpreterContext);
			if (includeSkillEffects && skill.HasPropagation && skill.SkillAction.SkillActionExecution.PropagationAffectedUnits != null && skill.SkillAction.SkillActionExecution.PropagationAffectedUnits.TryGetValue(skill.SkillAction.SkillActionExecution.HitIndex, out var value))
			{
				SkillCastFxDefinition skillCastFxDefinition = castFx.CastFxDefinition as SkillCastFxDefinition;
				num += skillCastFxDefinition.PropagationDelay * (float)value.Count;
			}
			return num;
		}
		return 0f;
	}

	protected void PlaySkillCastAnim(TheLastStand.Model.Unit.Unit caster)
	{
		if (!(caster is EnemyUnit { GoalComputingStep: IBehaviorModel.E_GoalComputingStep.OnSpawn }))
		{
			caster?.UnitController.PlaySkillCastAnim(SkillActionExecution);
		}
	}

	protected virtual void PlaySkillSFXs(Tile currentTargetTile)
	{
	}

	protected virtual void ResetAfterExecution()
	{
	}

	protected virtual void ResetBetweenMultiHits(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, List<SkillActionResultDatas> resultData, List<Tile> allTilesInAoe)
	{
		skill.SkillAction.SkillActionController.Reset();
	}

	public void SetCastFxAffectedTiles(TheLastStand.Model.CastFx.CastFx castFx, Tile targetTile, List<Tile> hitTiles, TileObjectSelectionManager.E_Orientation currentTargetTileOrientation, bool mirroredOnYAxis = false, bool includeSkillEffects = true)
	{
		castFx.AffectedTiles.Clear();
		List<Tile> list = null;
		if (includeSkillEffects)
		{
			if (SkillActionExecution.PropagationAffectedUnits != null && SkillActionExecution.PropagationAffectedUnits.TryGetValue(SkillActionExecution.HitIndex, out var value))
			{
				int count = value.Count;
				list = new List<Tile>(count);
				for (int i = 0; i < count; i++)
				{
					list.Add(value[i].OriginTile);
				}
			}
			if (SkillActionExecution.Skill.SkillAction.HasEffect("SurroundingEffect"))
			{
				List<Tile> surroundingTiles = GetSurroundingTiles(targetTile, currentTargetTileOrientation, mirroredOnYAxis);
				if (surroundingTiles != null)
				{
					hitTiles.AddRange(surroundingTiles);
				}
			}
		}
		List<Tile> list2 = null;
		int j = 0;
		for (int count2 = castFx.CastFxDefinition.VisualEffectDefinitions.Count; j < count2; j++)
		{
			if (castFx.CastFxDefinition.VisualEffectDefinitions[j] is StandardVisualEffectDefinition standardVisualEffectDefinition)
			{
				switch (standardVisualEffectDefinition.Target.TargetType)
				{
				case StandardVisualEffectDefinition.TargetData.E_TargetType.HitTiles:
					list2 = hitTiles;
					break;
				case StandardVisualEffectDefinition.TargetData.E_TargetType.HitUnits:
					list2 = hitTiles.FindAll((Tile hitTile) => hitTile.Unit != null);
					break;
				case StandardVisualEffectDefinition.TargetData.E_TargetType.HitPlayableUnits:
					list2 = hitTiles.FindAll((Tile hitTile) => hitTile.Unit is PlayableUnit);
					break;
				case StandardVisualEffectDefinition.TargetData.E_TargetType.HitEnemyUnits:
					list2 = hitTiles.FindAll((Tile hitTile) => hitTile.Unit is EnemyUnit);
					break;
				case StandardVisualEffectDefinition.TargetData.E_TargetType.PropagationTiles:
					list2 = list;
					break;
				case StandardVisualEffectDefinition.TargetData.E_TargetType.Caster:
					list2 = new List<Tile>(1) { SkillActionExecution.SkillSourceTile };
					break;
				case StandardVisualEffectDefinition.TargetData.E_TargetType.FollowCaster:
					list2 = new List<Tile>(1) { SkillActionExecution.Caster.OriginTile };
					break;
				case StandardVisualEffectDefinition.TargetData.E_TargetType.AoeOrigin:
					list2 = new List<Tile>(1) { targetTile };
					break;
				case StandardVisualEffectDefinition.TargetData.E_TargetType.CastFxSourceTile:
					list2 = new List<Tile>(1) { castFx.SourceTile };
					break;
				}
				castFx.AffectedTiles.Add(new List<Tile>());
				if (list2 != null)
				{
					castFx.AffectedTiles[^1].AddRange(list2);
				}
			}
			else
			{
				TPSingleton<SkillManager>.Instance.LogError("Failed to prepare visual effect when casting " + SkillActionExecution.Skill.Name + ": visualEffectDefinition has an invalid type!", CLogLevel.DETAILED);
			}
		}
	}

	private void AddSkillRangeTile(Tile tile, Vector2Int distance, int maxRange, bool requiresLineOfSight, bool hasVisionOnCityTiles, bool hasInaccurateEffect, bool updateView = true)
	{
		if (tile == null)
		{
			return;
		}
		int num = Mathf.Abs(distance.x) + Mathf.Abs(distance.y);
		bool isSkillSelected = SkillManager.SelectedSkill == SkillActionExecution.Skill;
		TileObjectSelectionManager.E_Orientation orientationFromSelectionToTile = TileObjectSelectionManager.GetOrientationFromSelectionToTile(tile);
		if (hasInaccurateEffect)
		{
			SkillActionExecution.TileInaccurates.Add(tile);
		}
		if (!SkillActionExecution.InRangeTiles.Exclude.Contains(tile))
		{
			if (num >= SkillActionExecution.Skill.SkillDefinition.Range.x)
			{
				if (!requiresLineOfSight || CheckAndUpdateLineOfSight(tile, SkillActionExecution.SkillSourceTiles, distance, maxRange, SkillActionExecution.InRangeTiles.Obstacle, SkillActionExecution.InRangeTiles.Exclude, hasVisionOnCityTiles))
				{
					SkillActionExecution.InRangeTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: true, orientationFromSelectionToTile, isSkillSelected));
				}
				else
				{
					SkillActionExecution.InRangeTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: false, orientationFromSelectionToTile, isSkillSelected));
					SkillActionExecution.InRangeTiles.Exclude.Add(tile);
				}
			}
			if (distance != Vector2Int.zero && IsBlockingLineOfSight(tile) && requiresLineOfSight)
			{
				SkillActionExecution.InRangeTiles.Obstacle.Add(tile);
				if (SkillActionExecution.SkillSourceTiles.Count == 1)
				{
					ExcludeTiles(tile, distance, maxRange, SkillActionExecution.InRangeTiles.Obstacle, SkillActionExecution.InRangeTiles.Exclude, hasVisionOnCityTiles);
				}
			}
		}
		else if (num >= SkillActionExecution.Skill.SkillDefinition.Range.x)
		{
			SkillActionExecution.InRangeTiles.Range.Add(tile, new TilesInRangeInfos.TileDisplayInfos(hasLineOfSight: false, orientationFromSelectionToTile, isSkillSelected));
		}
	}

	private int ComputeSkillMaxRange()
	{
		if (SkillActionExecution.Caster is TheLastStand.Model.Unit.Unit unit)
		{
			return unit.UnitController.GetModifiedMaxRange(SkillActionExecution.Skill);
		}
		return SkillActionExecution.Skill.SkillDefinition.Range.y;
	}

	private bool HasInaccurateEffect()
	{
		return SkillActionExecution.Skill.SkillAction.HasEffect("Inaccurate");
	}

	private bool IsBossPhaseActorDying(List<SkillActionResultDatas> resultData, out EnemyUnit eUnit)
	{
		eUnit = null;
		if (resultData == null)
		{
			return false;
		}
		foreach (SkillActionResultDatas resultDatum in resultData)
		{
			if (resultDatum?.AffectedUnits == null)
			{
				continue;
			}
			foreach (TheLastStand.Model.Unit.Unit affectedUnit in resultDatum.AffectedUnits)
			{
				if (affectedUnit is EnemyUnit { IsBossPhaseActor: not false, IsDead: not false } enemyUnit)
				{
					eUnit = enemyUnit;
					return true;
				}
			}
		}
		return false;
	}

	private void ManeuverSkillExecution(TheLastStand.Model.Unit.Unit caster, TheLastStand.Model.Skill.Skill skill, Tile currentTargetTile, TileObjectSelectionManager.E_Orientation specificOrientation)
	{
		if (!skill.HasManeuver)
		{
			return;
		}
		Tile maneuverTile = GetManeuverTile(currentTargetTile, specificOrientation);
		if (caster.Path == null)
		{
			caster.Path = new List<Tile>();
		}
		else
		{
			caster.Path.Clear();
		}
		caster.Path.Add(caster.OriginTile);
		caster.Path.Add(maneuverTile);
		if (caster is PlayableUnit playableUnit)
		{
			int num = TileMapController.DistanceBetweenTiles(caster.OriginTile, maneuverTile);
			playableUnit.PlayableUnitController.AddCrossedTiles(num, skill.HasNoMomentum);
			if (skill.SkillDefinition.Id != "JumpOverWall")
			{
				TrophyManager.AppendValueToTrophiesConditions<TilesMovedUsingSkillsTrophyConditionController>(new object[2] { playableUnit.RandomId, num });
			}
			if (num >= 14 && playableUnit.RaceDefinition.Id == "Dwarf" && skill.SkillDefinition.Id == "EmergencyTunnelSkill")
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_DWARF_14_TILES_EMERGENCY_TUNNEL);
			}
		}
		caster.UnitController.PrepareForMovement(playWalkAnim: false, followPathOrientation: false, skill.SkillDefinition.SkillCastFxDefinition.ManeuverFxDefinition.Speed.EvalToFloat(SkillActionExecution.CastFx.CastFXInterpreterContext), skill.SkillDefinition.SkillCastFxDefinition.ManeuverFxDefinition.Delay.EvalToFloat(SkillActionExecution.CastFx.CastFXInterpreterContext)).StartTask();
	}

	private IEnumerator MultiHitSkillExecutionCoroutine(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill)
	{
		TheLastStand.Model.Unit.Unit casterUnit = caster as TheLastStand.Model.Unit.Unit;
		int multiHitIndex = 0;
		int multiHitCount = SkillActionExecution.TargetTiles.Count;
		while (multiHitIndex < multiHitCount && (casterUnit == null || !casterUnit.IsDead || casterUnit is EnemyUnit { IsDeathRattling: not false }) && SkillActionExecution.Caster != null)
		{
			SkillActionExecution.HitIndex = multiHitIndex;
			SkillTargetedTileInfo targetTileInfo = SkillActionExecution.TargetTiles[multiHitIndex];
			Tile currentTargetTile = targetTileInfo.Tile;
			SkillActionExecution.Skill.SkillAction.SkillActionController.EnsurePerkData(currentTargetTile, null, null, skill.SkillContainer is Perk);
			if (SkillActionExecution.Skill.SkillDefinition.SkillCastFxDefinition != null)
			{
				SkillActionExecution.CastFx.TargetTile = currentTargetTile;
			}
			List<Tile> affectedTiles = GetAffectedTiles(currentTargetTile, alwaysReturnFullPattern: false, targetTileInfo.Orientation, targetTileInfo.IsMirroredOnYAxis);
			List<Tile> surroundingTiles = GetSurroundingTiles(currentTargetTile, targetTileInfo.Orientation, targetTileInfo.IsMirroredOnYAxis);
			List<SkillActionResultDatas> resultData = ApplyEffects(caster, skill, affectedTiles, surroundingTiles);
			SkillActionExecution.AllResultData.Add(resultData);
			if (casterUnit != null)
			{
				if (targetTileInfo.Orientation != TileObjectSelectionManager.E_Orientation.NONE)
				{
					casterUnit.UnitController.LookAtDirection(TileObjectSelectionManager.GetDirectionFromOrientation(targetTileInfo.Orientation));
				}
				else
				{
					casterUnit.UnitController.LookAt(currentTargetTile);
				}
			}
			ApplySkillEffects(caster, skill, currentTargetTile, resultData, targetTileInfo.Orientation);
			SetBuildingCanPrepareForDeath(resultData, canPrepareForDeath: true);
			if (IsBossPhaseActorDying(resultData, out var eUnit) && TPSingleton<BossManager>.Instance.ShouldTriggerBossWaveVictory)
			{
				TPSingleton<GameManager>.Instance.Game.NightTurn = Game.E_NightTurn.FinalBossDeath;
				ACameraView.Zoom(zoomIn: true);
				ACameraView.MoveTo(eUnit.UnitView.transform.position + Vector3.up * 2f, TPSingleton<BossManager>.Instance.BossKilledCameraMovementDuration, TPSingleton<BossManager>.Instance.BossKilledCameraMovementEasing);
				yield return SharedYields.WaitForSeconds(TPSingleton<BossManager>.Instance.BossKilledCameraMovementDuration);
			}
			float num = PlayCastFxs(SkillActionExecution.CastFx, caster, skill, currentTargetTile, GetAffectedTiles(currentTargetTile, alwaysReturnFullPattern: true, targetTileInfo.Orientation, targetTileInfo.IsMirroredOnYAxis), targetTileInfo.Orientation, targetTileInfo.IsMirroredOnYAxis);
			if (num > 0f)
			{
				yield return SharedYields.WaitForSeconds(num);
			}
			if ((this is SpawnSkillActionExecutionController || this is BuildSkillActionExecutionController) && ShouldWaitForDeathRattleOrHeroesDeathSequence(resultData))
			{
				yield return WaitForDeathRattleAndHeroesDeathSequence(resultData);
			}
			ApplyEffectsAfterFXs(caster, skill, resultData, affectedTiles, surroundingTiles);
			SetBuildingCanPrepareForDeath(resultData, canPrepareForDeath: true);
			PlaySkillSFXs(currentTargetTile);
			TPSingleton<EnemyUnitManager>.Instance.ApplyContagionToDyingEnemies();
			if (!(skill.SkillContainer is Perk) && caster is PlayableUnit playableUnit)
			{
				playableUnit.Events.GetValueOrDefault(E_EffectTime.OnSkillNextHit)?.Invoke(SkillActionExecution.Skill.SkillAction.PerkDataContainer);
			}
			TPSingleton<EnemyUnitManager>.Instance.ApplyContagionToDyingEnemies();
			EffectManager.DisplayEffects();
			if (skill.SkillDefinition.SkillCastFxDefinition != null && multiHitCount > 1 && multiHitIndex < multiHitCount - 1 && skill.SkillDefinition.SkillCastFxDefinition.MultiHitDelay > 0f)
			{
				yield return SharedYields.WaitForSeconds(skill.SkillDefinition.SkillCastFxDefinition.MultiHitDelay);
			}
			if (multiHitIndex < SkillActionExecution.MultiHitTargetedTiles.Count)
			{
				foreach (Tile item in SkillActionExecution.MultiHitTargetedTiles[multiHitIndex])
				{
					SkillManager.RemoveMultiHitTargetHUD(item);
				}
			}
			if (casterUnit?.UnitController.MoveTask != null)
			{
				yield return new WaitUntil(() => casterUnit.UnitController.MoveTask == null);
			}
			if (ShouldWaitForDeathRattleOrHeroesDeathSequence(resultData))
			{
				yield return WaitForDeathRattleAndHeroesDeathSequence(resultData);
			}
			if (TPSingleton<BossManager>.Instance.IsPlayingDeathCutscene)
			{
				yield return TPSingleton<BossManager>.Instance.DeathCutscene.WaitUntilIsOver;
			}
			ResetBetweenMultiHits(caster, skill, resultData, affectedTiles.Concat(surroundingTiles).ToList());
			int num2 = multiHitIndex + 1;
			multiHitIndex = num2;
		}
		ResetAfterExecution();
	}

	private void RefreshTargetsHud()
	{
		foreach (TheLastStand.Model.Unit.Unit allAttackedUnit in SkillActionExecution.AllAttackedUnits)
		{
			if (!allAttackedUnit.IsDead)
			{
				if (!allAttackedUnit.UnitView.UnitHUD.IsAnimating)
				{
					allAttackedUnit.UnitView.RefreshHealth();
					allAttackedUnit.UnitView.RefreshArmor();
					allAttackedUnit.UnitView.RefreshStatus();
					allAttackedUnit.UnitView.RefreshInjuryStage();
					allAttackedUnit.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: true);
				}
				else
				{
					allAttackedUnit.UnitView.UnitHUD.AnimatedDisplayFinishEvent += allAttackedUnit.UnitController.OnUnitHUDAnimatedDisplayFinished;
				}
			}
		}
	}

	private bool RequiresLineOfSight()
	{
		if (!SkillActionExecution.Skill.SkillAction.HasEffect("IgnoreLineOfSight"))
		{
			TheLastStand.Model.Building.Building building = SkillActionExecution.Caster.OriginTile.Building;
			if (building == null)
			{
				return true;
			}
			return !building.IsWatchtower;
		}
		return false;
	}

	private bool RequiresLineOfSightInCityTiles()
	{
		return !SkillActionExecution.Skill.SkillAction.HasEffect("IgnoreLineOfSightInCityTiles");
	}

	private void SetBuildingCanPrepareForDeath(List<SkillActionResultDatas> resultData, bool canPrepareForDeath)
	{
		foreach (SkillActionResultDatas resultDatum in resultData)
		{
			if (resultDatum == null || resultDatum.AffectedBuildings.Count == 0)
			{
				continue;
			}
			foreach (TheLastStand.Model.Building.Building affectedBuilding in resultDatum.AffectedBuildings)
			{
				affectedBuilding.DamageableModule?.DamageableModuleController.ChangeCanPrepareForDeath(canPrepareForDeath: true);
			}
		}
	}

	private bool ShouldWaitForDeathRattleOrHeroesDeathSequence(List<SkillActionResultDatas> resultData)
	{
		if (TPSingleton<PlayableUnitManager>.Instance.ShouldWaitUntilDeathSequences)
		{
			return true;
		}
		foreach (SkillActionResultDatas resultDatum in resultData)
		{
			if (resultDatum == null || (resultDatum.AffectedUnits.Count == 0 && resultDatum.AffectedBuildings.Count == 0))
			{
				continue;
			}
			foreach (TheLastStand.Model.Unit.Unit affectedUnit in resultDatum.AffectedUnits)
			{
				if (affectedUnit is EnemyUnit { IsDeathRattling: not false })
				{
					return true;
				}
			}
			foreach (TheLastStand.Model.Building.Building affectedBuilding in resultDatum.AffectedBuildings)
			{
				if (affectedBuilding.ShouldWaitDeathLikeEffect)
				{
					return true;
				}
			}
		}
		return false;
	}

	private IEnumerator SkillExecutionCoroutine()
	{
		ISkillCaster caster = SkillActionExecution.Caster;
		IEntity casterEntity = caster as IEntity;
		TheLastStand.Model.Skill.Skill skill = SkillActionExecution.Skill;
		TheLastStand.Model.Unit.Unit casterUnit = caster as TheLastStand.Model.Unit.Unit;
		PlayableUnit playableUnitCaster = caster as PlayableUnit;
		casterUnit?.UnitView.UnitHUD.ToggleFollowElement(toggle: true);
		SkillActionExecution.Skill.SkillAction.SkillActionController.EnsurePerkData(null, null, null, skill.SkillContainer is Perk);
		if (playableUnitCaster != null && !(skill.SkillContainer is Perk))
		{
			playableUnitCaster.Events.GetValueOrDefault(E_EffectTime.OnSkillCastBegin)?.Invoke(SkillActionExecution.Skill.SkillAction.PerkDataContainer);
		}
		yield return MultiHitSkillExecutionCoroutine(caster, skill);
		if (playableUnitCaster != null)
		{
			if (!(skill.SkillContainer is Perk))
			{
				playableUnitCaster.Events.GetValueOrDefault(E_EffectTime.OnSkillCastEnd)?.Invoke(SkillActionExecution.Skill.SkillAction.PerkDataContainer);
				EffectManager.DisplayEffects();
				TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnSkillExecutionOver);
			}
			if (playableUnitCaster.WillDieByPoison)
			{
				playableUnitCaster.UnitView.UnitHUD.DisplayIconFeedback();
				TileMapView.SetTiles(TileMapView.UnitFeedbackTilemap, playableUnitCaster.OccupiedTiles, "View/Tiles/Feedbacks/PoisonDeath");
			}
		}
		if (caster is BattleModule { IsDeathRattling: false } battleModule)
		{
			battleModule.BattleModuleController.OnSkillCastEnded(skill);
		}
		if (casterUnit != null && casterUnit.HasBeenExiled)
		{
			casterUnit.UnitController.ExecuteExile();
		}
		if (caster is EnemyUnit enemyUnit)
		{
			if (!enemyUnit.IsExecutingSkillOnSpawn)
			{
				if (enemyUnit.IsDeathRattling)
				{
					EnemyUnit enemyCaster = enemyUnit;
					yield return TPSingleton<PlayableUnitManager>.Instance.WaitUntilTakeDamageSequences;
					enemyCaster.IsDeathRattling = false;
					TPSingleton<EnemyUnitManager>.Instance.EnemiesDeathRattling.Remove(enemyCaster);
				}
			}
			else
			{
				enemyUnit.IsExecutingSkillOnSpawn = false;
				TPSingleton<EnemyUnitManager>.Instance.EnemiesExecutingSkillsOnSpawn.Remove(enemyUnit);
			}
		}
		else if (caster is BattleModule { IsDeathRattling: not false } battleModule2)
		{
			yield return TPSingleton<PlayableUnitManager>.Instance.WaitUntilTakeDamageSequences;
			BattleModuleController.FinalizeDeathRattling(battleModule2);
		}
		if (TPSingleton<EnemyUnitManager>.Instance.EnemiesDeathRattling.Count > 0)
		{
			yield return TPSingleton<EnemyUnitManager>.Instance.WaitUntilDeathRattlingEnemiesAreDone;
		}
		if (TPSingleton<BuildingManager>.Instance.BuildingsDeathRattling.Count > 0)
		{
			yield return TPSingleton<BuildingManager>.Instance.WaitUntilDeathRattlingBuildingsAreDone;
		}
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		if (((currentSpawnWave != null && !currentSpawnWave.SpawnWaveDefinition.IsBossWave) || TPSingleton<BossManager>.Instance.VictoryConditionIsToFinishWave) && skill.Owner is PlayableUnit && TPSingleton<EnemyUnitManager>.Instance.ComputedEnemyUnitsCount > 0 && TPSingleton<EnemyUnitManager>.Instance.EnemiesDying.Count >= TPSingleton<EnemyUnitManager>.Instance.ComputedEnemyUnitsCount)
		{
			yield return TPSingleton<EnemyUnitManager>.Instance.WaitUntilDyingEnemiesAreDone;
		}
		if (casterUnit != null && !casterUnit.IsDead)
		{
			casterUnit.State = TheLastStand.Model.Unit.Unit.E_State.Ready;
		}
		if (casterUnit != null && SkillActionExecution.Skill.SkillAction is AttackSkillAction attackSkillAction)
		{
			casterUnit.LastSkillType = attackSkillAction.AttackType;
		}
		TPSingleton<BarkManager>.Instance.Display();
		if (SkillActionExecution.CastFx != null)
		{
			SkillActionExecution.CastFx.TargetTile = null;
		}
		caster.SkillExecutionCoroutine = null;
		if (casterUnit != null)
		{
			UnitView unitView = casterUnit.UnitView;
			if ((object)unitView != null && unitView.IsCastingSkill)
			{
				casterUnit.UnitView.StartCoroutine(casterUnit.UnitView.ToggleFollowElementOffWhenIdle());
			}
		}
		TPSingleton<GameManager>.Instance.GameAnalytics.OnSkillExecutionEnd(this);
		TPSingleton<AchievementManager>.Instance.HandleSkillEnd(this);
		casterEntity?.Log("Casting " + SkillActionExecution.Skill.SkillDefinition.Id);
		UpdateUndoCondition();
		RefreshTargetsHud();
		Reset();
	}

	private void UpdateUndoCondition()
	{
		foreach (List<SkillActionResultDatas> allResultDatum in SkillActionExecution.AllResultData)
		{
			foreach (SkillActionResultDatas item in allResultDatum)
			{
				if (item.AffectedBuildings.Count > 0 || item.AffectedUnits.Count > 0 || item.UnitsToSpawnTarget.Count > 0 || item.PotentiallyAffectedDamageables.Count > 0)
				{
					TPSingleton<PlayableUnitManager>.Instance.ShouldClearUndoStack = true;
					return;
				}
			}
		}
	}

	private IEnumerator WaitForDeathRattleAndHeroesDeathSequence(List<SkillActionResultDatas> resultData)
	{
		foreach (SkillActionResultDatas result in resultData)
		{
			if (result == null || (result.AffectedUnits.Count == 0 && result.AffectedBuildings.Count == 0))
			{
				continue;
			}
			foreach (TheLastStand.Model.Building.Building attackedBuilding in result.AffectedBuildings)
			{
				yield return new WaitUntil(() => !attackedBuilding.ShouldWaitDeathLikeEffect);
			}
			foreach (TheLastStand.Model.Unit.Unit affectedUnit in result.AffectedUnits)
			{
				EnemyUnit dyingEnemy = affectedUnit as EnemyUnit;
				if (dyingEnemy != null && dyingEnemy.IsDeathRattling)
				{
					yield return new WaitUntil(() => !dyingEnemy.IsDeathRattling);
				}
				if (TPSingleton<PlayableUnitManager>.Instance.ShouldWaitUntilDeathSequences)
				{
					if (TPSingleton<PlayableUnitManager>.Instance.ShouldTriggerPlayableUnitsDeathSequence)
					{
						EffectManager.DisplayEffects();
					}
					yield return TPSingleton<PlayableUnitManager>.Instance.WaitUntilDeathSequences;
				}
			}
			EffectManager.DisplayEffects();
		}
	}
}

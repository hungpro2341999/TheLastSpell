using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit.UI;

namespace TheLastStand.View.Skill.SkillAction.SkillActionExecution;

public abstract class SkillActionExecutionView
{
	public TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecution SkillExecution { get; set; }

	public void Clear(bool keepRangeTiles = false)
	{
		TileMapView.ClearTiles(TileMapView.AreaOfEffectTilemap);
		ResetAffectedDamageablesHud();
		if (!keepRangeTiles)
		{
			TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearDialsTiles(SkillExecution.Caster.OriginTile);
			TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearRangeTiles(SkillExecution.InRangeTiles.Range);
			TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearInaccurateRangeTiles(SkillExecution.TileInaccurates);
			SkillExecution.SkillExecutionController.HideSkillTargetingForTargets();
		}
	}

	public void DisplayAreaOfEffect(Tile originTile, bool hit = false, bool clearAreaOfEffectTiles = true)
	{
		if (originTile == null)
		{
			return;
		}
		if (clearAreaOfEffectTiles)
		{
			ResetAffectedDamageablesHud();
			TileMapView.ClearTiles(TileMapView.AreaOfEffectTilemap);
		}
		if (SkillExecution.Caster == null)
		{
			return;
		}
		TileObjectSelectionManager.E_Orientation cursorDependantOrientation = SkillExecution.Skill.CursorDependantOrientation;
		bool flag = SkillExecution.Skill.SkillController.IsValidatingTargetingConstraints(originTile) && SkillExecution.InRangeTiles.IsInLineOfSight(originTile) && SkillExecution.SkillExecutionController.IsManeuverValid(originTile, cursorDependantOrientation);
		bool isSelectedSkillFlipped = SkillManager.IsSelectedSkillFlipped;
		List<Tile> affectedTiles = SkillExecution.SkillExecutionController.GetAffectedTiles(originTile, alwaysReturnFullPattern: true, cursorDependantOrientation, isSelectedSkillFlipped);
		Tile maneuverTile = SkillExecution.SkillExecutionController.GetManeuverTile(originTile, cursorDependantOrientation, isSelectedSkillFlipped);
		if (maneuverTile != null)
		{
			affectedTiles.Add(maneuverTile);
		}
		List<Tile> surroundingTiles = SkillExecution.SkillExecutionController.GetSurroundingTiles(originTile, cursorDependantOrientation, isSelectedSkillFlipped);
		affectedTiles.AddRange(surroundingTiles);
		Tile tile = null;
		int i = 0;
		for (int count = affectedTiles.Count; i < count; i++)
		{
			tile = affectedTiles[i];
			bool flag2 = surroundingTiles.Contains(tile);
			bool flag3 = tile == maneuverTile;
			if (hit)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayAreaOfEffectHitFeedback(tile);
			}
			else
			{
				TileMapView.E_AreaOfEffectTileDisplayType areaOfEffectTileDisplayType = TileMapView.E_AreaOfEffectTileDisplayType.AreaOfEffect;
				if (flag3)
				{
					areaOfEffectTileDisplayType = TileMapView.E_AreaOfEffectTileDisplayType.Maneuver;
				}
				else if (flag2)
				{
					areaOfEffectTileDisplayType = TileMapView.E_AreaOfEffectTileDisplayType.Surrounding;
				}
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayAreaOfEffectTile(tile, areaOfEffectTileDisplayType, !flag || (tile.HasFog && TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night), TileMapView.AreaOfEffectTilemap);
			}
			if (!flag3)
			{
				DisplayAreaOfEffectTileFeedback(tile, flag2);
			}
		}
		if (SkillExecution.Caster is PlayableUnit playableUnit && SkillExecution.Skill.SkillAction.HasEffect("CasterEffect"))
		{
			playableUnit.UnitView.UnitHUD.AttackEstimationDisplay.Display(SkillExecution, null, playableUnit, isSurroundingEffect: false, casterEffect: true);
		}
	}

	protected virtual void DisplayAreaOfEffectTileFeedback(Tile affectedTile, bool isSurroundingEffect)
	{
		IDamageable damageable = null;
		if (SkillExecution.Skill.SkillAction.SkillActionController.IsUnitAffected(affectedTile))
		{
			damageable = affectedTile.Unit;
		}
		if (damageable != null && !SkillExecution.PreviewAffectedDamageables.Contains(damageable))
		{
			SkillExecution.PreviewAffectedDamageables.Add(damageable);
			if (affectedTile.Unit.UnitView.UnitHUD is EnemyUnitHUD enemyUnitHUD)
			{
				enemyUnitHUD.DisplayIconFeedback(show: false);
			}
			affectedTile.Unit.UnitView.UnitHUD.AttackEstimationDisplay.Display(SkillExecution, affectedTile, affectedTile.Unit, isSurroundingEffect, casterEffect: false);
		}
	}

	public void ResetAffectedDamageablesHud()
	{
		if (SkillExecution.Caster is PlayableUnit playableUnit)
		{
			playableUnit.UnitView.UnitHUD.AttackEstimationDisplay.Hide();
		}
		for (int num = SkillExecution.PreviewAffectedDamageables.Count - 1; num >= 0; num--)
		{
			IDamageableHUD damageableHUD;
			if (SkillExecution.PreviewAffectedDamageables[num] != null && (damageableHUD = SkillExecution.PreviewAffectedDamageables[num].DamageableView.DamageableHUD) != null)
			{
				if (damageableHUD is UnitHUD unitHUD)
				{
					unitHUD.AttackEstimationDisplay.Hide();
				}
				damageableHUD.Highlight = false;
				damageableHUD.DisplayHealthIfNeeded();
				if (damageableHUD is EnemyUnitHUD enemyUnitHUD)
				{
					enemyUnitHUD.DisplayIconFeedback();
				}
			}
		}
		SkillExecution.PreviewAffectedDamageables.Clear();
	}
}

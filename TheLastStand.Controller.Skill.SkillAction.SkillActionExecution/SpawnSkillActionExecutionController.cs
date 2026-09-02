using System.Collections.Generic;
using TheLastStand.Database.Unit;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.CastFx;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;
using UnityEngine;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class SpawnSkillActionExecutionController : SkillActionExecutionController
{
	public SpawnSkillActionExecution SpawnSkillActionExecution => base.SkillActionExecution as SpawnSkillActionExecution;

	public SpawnSkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
		: base(skill)
	{
		SpawnSkillActionExecutionView spawnSkillActionExecutionView = new SpawnSkillActionExecutionView();
		base.SkillActionExecution = new SpawnSkillActionExecution(this, spawnSkillActionExecutionView, skill);
		spawnSkillActionExecutionView.SkillExecution = base.SkillActionExecution;
	}

	public override bool IsTileAffected(Tile tile)
	{
		if (tile.Unit == SpawnSkillActionExecution.Caster)
		{
			return true;
		}
		if (tile.Unit != null || tile.Building != null)
		{
			return ((SpawnSkillAction)base.SkillActionExecution.Skill.SkillAction).SpawnSkillActionController.CanDestroyBuilding(tile.Building);
		}
		return true;
	}

	protected override void ApplyEffectsAfterFXs(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, List<SkillActionResultDatas> resultData, List<Tile> affectedTiles, List<Tile> surroundingTiles)
	{
		base.ApplyEffectsAfterFXs(caster, skill, resultData, affectedTiles, surroundingTiles);
		foreach (SkillActionResultDatas resultDatum in resultData)
		{
			foreach (KeyValuePair<Tile, (string, UnitCreationSettings)> item in resultDatum.UnitsToSpawnTarget)
			{
				resultDatum.AddAffectedUnit(EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.TryGetValue(item.Value.Item1, out var value) ? EnemyUnitManager.CreateEliteEnemyUnit(value, item.Key, item.Value.Item2) : EnemyUnitManager.CreateEnemyUnit(EnemyUnitDatabase.EnemyUnitTemplateDefinitions[item.Value.Item1], item.Key, item.Value.Item2));
			}
		}
		if (caster is EnemyUnit enemyUnit && enemyUnit.EnemyUnitTemplateDefinition.VisualEvolutions != null && enemyUnit.Id == "SpawnerCocoon")
		{
			enemyUnit.CurrentVariantIndex++;
			string variantId = enemyUnit.VariantId;
			enemyUnit.VariantId = ((enemyUnit.EnemyUnitTemplateDefinition.VisualEvolutions.Count > enemyUnit.CurrentVariantIndex) ? enemyUnit.EnemyUnitTemplateDefinition.VisualEvolutions[enemyUnit.CurrentVariantIndex] : string.Empty);
			if (enemyUnit.VariantId != string.Empty)
			{
				enemyUnit.EnemyUnitView.InitVisuals(playSpawnAnim: true);
				return;
			}
			enemyUnit.VariantId = variantId;
			enemyUnit.EnemyUnitController.PrepareForExile();
		}
	}

	protected override float PlayCastFxs(TheLastStand.Model.CastFx.CastFx castFx, ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, Tile currentTargetTile, List<Tile> affectedTiles, TileObjectSelectionManager.E_Orientation currentTargetTileOrientation, bool mirroredOnYAxis = false, bool includeSkillEffects = true)
	{
		float num = ((SpawnSkillAction)skill.SkillAction).SpawnSkillActionDefinition.Delay;
		if (base.SkillActionExecution.Skill.SkillDefinition.SkillCastFxDefinition != null && !(caster is BossUnit))
		{
			PlaySkillCastAnim(caster as TheLastStand.Model.Unit.Unit);
			SetCastFxAffectedTiles(base.SkillActionExecution.CastFx, currentTargetTile, affectedTiles, currentTargetTileOrientation, mirroredOnYAxis);
			TileObjectSelectionManager.E_Orientation orientationFromTileToTile = TileObjectSelectionManager.GetOrientationFromTileToTile(base.SkillActionExecution.SkillSourceTile, currentTargetTile);
			base.SkillActionExecution.CastFx.CastFxController.PlayCastFxs(orientationFromTileToTile, default(Vector2), caster, mirroredOnYAxis);
			if (num <= 0f)
			{
				num = skill.SkillDefinition.SkillCastFxDefinition.CastTotalDuration.EvalToFloat(base.SkillActionExecution.CastFx.CastFXInterpreterContext);
			}
		}
		return num;
	}
}

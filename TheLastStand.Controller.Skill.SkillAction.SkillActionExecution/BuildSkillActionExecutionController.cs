using System.Collections.Generic;
using System.Linq;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.CastFx;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;
using UnityEngine;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class BuildSkillActionExecutionController : SkillActionExecutionController
{
	public BuildSkillActionExecution BuildSkillActionExecution => base.SkillActionExecution as BuildSkillActionExecution;

	public BuildSkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
		: base(skill)
	{
		BuildSkillActionExecutionView buildSkillActionExecutionView = new BuildSkillActionExecutionView();
		base.SkillActionExecution = new BuildSkillActionExecution(this, buildSkillActionExecutionView, skill);
		buildSkillActionExecutionView.SkillExecution = base.SkillActionExecution;
	}

	public override List<Tile> GetAffectedTiles(Tile targetTile, bool alwaysReturnFullPattern, TileObjectSelectionManager.E_Orientation specificOrientation, bool mirroredOnYAxis = false)
	{
		switch ((base.SkillActionExecution.Skill.SkillAction as BuildSkillAction).BuildSkillActionDefinition.BuildLocationDefinition.Name)
		{
		case "AroundTheCaster":
		{
			ITileObject caster = base.SkillActionExecution.Caster;
			if (caster != null)
			{
				return (from tile in caster.TileObjectController.GetAdjacentTilesWithDiagonals()
					where tile.IsCrossable
					select tile).ToList();
			}
			return new List<Tile> { targetTile };
		}
		case "OnSelf":
			return base.SkillActionExecution.Caster.OccupiedTiles.ToList();
		default:
			return new List<Tile> { targetTile };
		}
	}

	protected override float PlayCastFxs(TheLastStand.Model.CastFx.CastFx castFx, ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, Tile currentTargetTile, List<Tile> affectedTiles, TileObjectSelectionManager.E_Orientation currentTargetTileOrientation, bool mirroredOnYAxis = false, bool includeSkillEffects = true)
	{
		if (base.SkillActionExecution.Skill.SkillDefinition.SkillCastFxDefinition != null)
		{
			PlaySkillCastAnim(caster as TheLastStand.Model.Unit.Unit);
			BuildSkillActionExecution.CastFx.TargetTile = affectedTiles.ElementAt(0);
			BuildSkillActionExecution.CastFx.SourceTile = caster.OriginTile;
			BuildSkillActionExecution.CastFx.AffectedTiles.Clear();
			BuildSkillActionExecution.CastFx.AffectedTiles.Add(affectedTiles);
			BuildSkillActionExecution.CastFx.CastFxController.PlayCastFxs(TileObjectSelectionManager.E_Orientation.NONE, default(Vector2), caster);
			return ((BuildSkillAction)skill.SkillAction).BuildSkillActionDefinition.Delay;
		}
		return 0f;
	}

	protected override List<SkillActionResultDatas> ApplyEffects(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, List<Tile> affectedTiles, List<Tile> surroundingTiles)
	{
		return new List<SkillActionResultDatas>();
	}

	protected override void ApplyEffectsAfterFXs(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, List<SkillActionResultDatas> resultData, List<Tile> affectedTiles, List<Tile> surroundingTiles)
	{
		skill.SkillAction.SkillActionController.ApplyEffect(caster, affectedTiles, surroundingTiles);
		base.ApplyEffectsAfterFXs(caster, skill, resultData, affectedTiles, surroundingTiles);
	}
}

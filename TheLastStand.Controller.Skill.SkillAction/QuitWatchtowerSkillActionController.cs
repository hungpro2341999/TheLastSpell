using TPLib;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Skill.SkillAction;

public class QuitWatchtowerSkillActionController : SkillActionController
{
	public QuitWatchtowerSkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new QuitWatchtowerSkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new QuitWatchtowerSkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
	}

	public override bool IsBuildingAffected(Tile targetTile)
	{
		return false;
	}

	public override bool IsUnitAffected(Tile targetTile)
	{
		if (targetTile.Unit != null && targetTile.CanAffectThroughFog(base.SkillAction.SkillActionExecution.Caster))
		{
			return !targetTile.Unit.IsDead;
		}
		return false;
	}

	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		PlayableUnit playableUnit = caster as PlayableUnit;
		playableUnit.PlayableUnitController.LookAt(targetTile, playableUnit.OriginTile);
		playableUnit.OriginTile.Unit = null;
		playableUnit.OriginTile = targetTile;
		targetTile.Unit = playableUnit;
		if (playableUnit.OriginTile.Building != null)
		{
			TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuilding(playableUnit.OriginTile.Building, playableUnit.OriginTile.Building.OriginTile);
		}
		playableUnit.PlayableUnitView.UpdatePosition();
		return new SkillActionResultDatas();
	}

	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		return new SkillActionResultDatas();
	}
}

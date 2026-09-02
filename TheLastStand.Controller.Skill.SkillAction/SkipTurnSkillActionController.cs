using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Skill.SkillAction;

public class SkipTurnSkillActionController : SkillActionController
{
	public SkipTurnSkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new SkipTurnSkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new SkipTurnSkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
	}

	public override bool IsBuildingAffected(Tile targetTile)
	{
		return false;
	}

	public override bool IsUnitAffected(Tile targetTile)
	{
		return false;
	}

	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		return new SkillActionResultDatas();
	}

	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		return new SkillActionResultDatas();
	}
}

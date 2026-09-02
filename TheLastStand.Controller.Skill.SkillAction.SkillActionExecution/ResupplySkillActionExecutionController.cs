using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class ResupplySkillActionExecutionController : SkillActionExecutionController
{
	public ResupplySkillActionExecution ResupplySkillActionExecution => base.SkillActionExecution as ResupplySkillActionExecution;

	public ResupplySkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
		: base(skill)
	{
		ResupplySkillActionExecutionView resupplySkillActionExecutionView = new ResupplySkillActionExecutionView();
		base.SkillActionExecution = new ResupplySkillActionExecution(this, resupplySkillActionExecutionView, skill);
		resupplySkillActionExecutionView.SkillExecution = base.SkillActionExecution;
	}

	public override void Reset()
	{
		base.Reset();
		if (base.SkillActionExecution.Skill.SkillAction is ResupplySkillAction resupplySkillAction && (resupplySkillAction.HasEffect("ResupplyCharges") || resupplySkillAction.HasEffect("ResupplyOverallUses")))
		{
			resupplySkillAction.ResupplySkillActionExecution.ResupplySkillActionExecutionView.HideDisplayedHUD();
		}
	}

	public override bool IsTileAffected(Tile tile)
	{
		if (tile.Building == null || !base.SkillActionExecution.Skill.SkillDefinition.AffectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Building))
		{
			if (tile.Unit is PlayableUnit)
			{
				return base.SkillActionExecution.Skill.SkillDefinition.AffectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit);
			}
			return false;
		}
		return true;
	}
}

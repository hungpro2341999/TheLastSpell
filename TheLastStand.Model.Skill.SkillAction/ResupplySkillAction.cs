using System.Collections.Generic;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Skill.SkillAction;

public class ResupplySkillAction : SkillAction
{
	public ResupplySkillActionController ResupplySkillActionController => base.SkillActionController as ResupplySkillActionController;

	public ResupplySkillActionDefinition ResupplySkillActionDefinition => base.SkillActionDefinition as ResupplySkillActionDefinition;

	public ResupplySkillActionExecution ResupplySkillActionExecution => base.SkillActionExecution as ResupplySkillActionExecution;

	public override string EstimationIconId => "Resupply";

	public ResupplySkillAction(SkillActionDefinition definition, SkillActionController controller, Skill skill)
		: base(definition, controller, skill)
	{
	}

	public bool CheckBuildingNeedRepair(TheLastStand.Model.Building.Building building)
	{
		if (ResupplySkillActionDefinition.TryGetAllEffects("ResupplyCharges", out List<SkillEffectDefinition> effects) && effects.Count != 0 && (effects[0] as ResupplyChargesSkillEffectDefinition).TargetIds.Contains(building.Id) && building.BattleModule != null && building.BattleModule.RemainingTrapCharges < building.BuildingDefinition.BattleModuleDefinition.MaximumTrapCharges)
		{
			return true;
		}
		if (ResupplySkillActionDefinition.TryGetAllEffects("ResupplyOverallUses", out List<SkillEffectDefinition> effects2) && effects2.Count != 0 && (effects2[0] as ResupplyOverallUsesSkillEffectDefinition).TargetIds.Contains(building.Id) && building.BattleModule != null)
		{
			foreach (Skill skill in building.BattleModule.Skills)
			{
				if (skill != null && skill.OverallUses != 0 && skill.OverallUsesRemaining < skill.OverallUses)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckUnitNeedResupply(TheLastStand.Model.Unit.Unit unit)
	{
		if (unit is PlayableUnit playableUnit && TryGetFirstEffect<ResupplySkillsSkillEffectDefinition>("ResupplySkills", out var _))
		{
			foreach (Skill weaponSkill in playableUnit.PlayableUnitController.GetWeaponSkills(dontCheckPhase: false, getBaseAndReplacementSkills: true))
			{
				if (weaponSkill.UsesPerTurnRemaining < weaponSkill.UsesPerTurn)
				{
					return true;
				}
			}
		}
		return false;
	}
}

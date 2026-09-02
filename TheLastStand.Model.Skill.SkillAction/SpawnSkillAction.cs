using System;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Skill.SkillAction;

public class SpawnSkillAction : SkillAction
{
	public bool ComputedUnitsToSpawn;

	public Tuple<string, UnitCreationSettings> UnitToSpawnByWeight;

	public SpawnSkillActionController SpawnSkillActionController => base.SkillActionController as SpawnSkillActionController;

	public SpawnSkillActionDefinition SpawnSkillActionDefinition => base.SkillActionDefinition as SpawnSkillActionDefinition;

	public SpawnSkillActionExecution SpawnSkillActionExecution => base.SkillActionExecution as SpawnSkillActionExecution;

	public override string EstimationIconId => "Spawn";

	public SpawnSkillAction(SkillActionDefinition definition, SkillActionController controller, Skill skill)
		: base(definition, controller, skill)
	{
	}
}

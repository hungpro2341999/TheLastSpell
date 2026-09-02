using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Model.Building.BuildingPassive;

public class GenerateGuardian : BuildingPassiveEffect
{
	public EnemyUnit Guardian;

	public GenerateGuardianDefinition GenerateGuardianDefinition => base.BuildingPassiveEffectDefinition as GenerateGuardianDefinition;

	public GenerateGuardian(PassivesModule buildingPassivesModule, GenerateGuardianDefinition buildingPassiveDefinition, GenerateGuardianController buildingPassiveController)
		: base(buildingPassivesModule, buildingPassiveDefinition, buildingPassiveController)
	{
	}
}

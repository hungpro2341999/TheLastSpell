using TPLib;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class IncreaseWorkersController : BuildingPassiveEffectController
{
	public IncreaseWorkers IncreaseWorkers => base.BuildingPassiveEffect as IncreaseWorkers;

	public IncreaseWorkersController(PassivesModule buildingPassivesModule, IncreaseWorkersDefinition increaseWorkersDefinition)
	{
		base.BuildingPassiveEffect = new IncreaseWorkers(buildingPassivesModule, increaseWorkersDefinition, this);
	}

	public override void Apply()
	{
		TPSingleton<ResourceManager>.Instance.IncreaseMaxWorkers(IncreaseWorkers.IncreaseWorkersDefinition.Value.EvalToInt() + IncreaseWorkers.UpgradedBonusValue);
	}

	public override void ImproveEffect(int bonus)
	{
		IncreaseWorkers.UpgradedBonusValue += bonus;
		TPSingleton<ResourceManager>.Instance.IncreaseMaxWorkers(bonus);
	}

	public override void Unapply()
	{
		TPSingleton<ResourceManager>.Instance.DecreaseMaxWorkers(IncreaseWorkers.IncreaseWorkersDefinition.Value.EvalToInt() + IncreaseWorkers.UpgradedBonusValue);
	}
}

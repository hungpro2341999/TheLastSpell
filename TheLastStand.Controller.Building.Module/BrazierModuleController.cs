using TheLastStand.Definition.Building.Module;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using UnityEngine;

namespace TheLastStand.Controller.Building.Module;

public class BrazierModuleController : BuildingModuleController
{
	public BrazierModule BrazierModule { get; }

	public BrazierModuleController(BuildingController buildingControllerParent, BrazierModuleDefinition brazierModuleDefinition)
		: base(buildingControllerParent, brazierModuleDefinition)
	{
		BrazierModule = base.BuildingModule as BrazierModule;
	}

	public int LoseBrazierPoints(int damage, bool triggerEvent = false)
	{
		if (BrazierModule.BrazierPoints <= 0)
		{
			return 0;
		}
		int num = Mathf.Min(damage, BrazierModule.BrazierPoints);
		BrazierModule.BrazierPoints -= num;
		if (BrazierModule.BuildingParent.IsBossPhaseActor && BrazierModule.BrazierPoints == 0)
		{
			BrazierModule.BuildingParent.PrepareBossActorDeath();
		}
		if (triggerEvent && BrazierModule.BrazierPoints == 0)
		{
			BrazierModule.IsExtinguishing = true;
			base.BuildingControllerParent.PassivesModuleController?.ApplyPassiveEffect(E_EffectTime.OnExtinguish);
		}
		return num;
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new BrazierModule(building, buildingModuleDefinition as BrazierModuleDefinition, this);
	}
}

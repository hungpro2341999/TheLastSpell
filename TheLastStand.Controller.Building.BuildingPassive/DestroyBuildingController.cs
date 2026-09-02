using System.Collections;
using TPLib;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class DestroyBuildingController : BuildingPassiveEffectController
{
	public DestroyBuildingController(PassivesModule buildingPassivesModule, DestroyBuildingDefinition destroyBuildingDefinition)
	{
		base.BuildingPassiveEffect = new DestroyBuilding(buildingPassivesModule, destroyBuildingDefinition, this);
	}

	public override void Apply()
	{
		TPSingleton<BuildingManager>.Instance.StartCoroutine(WaitDisplayEffectsAndDestroyBuilding());
	}

	private IEnumerator WaitDisplayEffectsAndDestroyBuilding()
	{
		yield return base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingView.DisplaySkillEffects(0f);
		yield return base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.BuildingView.PlayDieAnimCoroutine();
		BuildingManager.DestroyBuilding(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OriginTile);
		TPSingleton<BuildingManager>.Instance.RestoreBuildingIfNeeded(base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.OriginTile);
	}
}

using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class TransformBuildingController : BuildingPassiveEffectController
{
	public TransformBuilding TransformBuilding => base.BuildingPassiveEffect as TransformBuilding;

	public TransformBuildingController(PassivesModule buildingPassivesModule, TransformBuildingDefinition transformBuildingDefinition)
	{
		base.BuildingPassiveEffect = new TransformBuilding(buildingPassivesModule, transformBuildingDefinition, this);
	}

	public override void Apply()
	{
		TheLastStand.Model.Building.Building buildingParent = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent;
		TheLastStand.Model.Building.Building building = BuildingManager.ReplaceBuilding(buildingParent.OriginTile, buildingParent, BuildingDatabase.BuildingDefinitions[TransformBuilding.TransformBuildingDefinition.GetRandomBuildingId()], ignoreBuilding: true, TransformBuilding.TransformBuildingDefinition.Instantaneous);
		if (TransformBuilding.TransformBuildingDefinition.PlayDestructionSmoke)
		{
			building.BuildingView.StartCoroutine(building.BuildingView.PlayDestructionSmokeCoroutine());
		}
	}
}

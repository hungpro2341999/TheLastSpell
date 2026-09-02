using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class BuildingGlobalUpgradeController : BuildingUpgradeController
{
	public BuildingGlobalUpgrade BuildingGlobalUpgrade => base.BuildingUpgrade as BuildingGlobalUpgrade;

	public BuildingGlobalUpgradeController(SerializedGlobalUpgrade container, TheLastStand.Model.Building.Building building)
		: base(container, building)
	{
	}

	public BuildingGlobalUpgradeController(BuildingUpgradeDefinition definition, TheLastStand.Model.Building.Building building)
		: base(definition, building)
	{
	}

	protected override TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade CreateModel(SerializedUpgrade container, BuildingUpgradeDefinition definition, BuildingUpgradeController controller, TheLastStand.Model.Building.Building building)
	{
		return new BuildingGlobalUpgrade(container, definition, controller, building);
	}

	protected override TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade CreateModel(BuildingUpgradeDefinition definition, BuildingUpgradeController controller, TheLastStand.Model.Building.Building building)
	{
		return new BuildingGlobalUpgrade(definition, controller, building);
	}

	protected override void PlayFx()
	{
		foreach (BuildingGlobalUpgrade buildingGlobalUpgrade in BuildingGlobalUpgrade.BuildingUpgradeLevel.BuildingGlobalUpgrades)
		{
			(buildingGlobalUpgrade.BuildingUpgradeLevels[buildingGlobalUpgrade.UpgradeLevel].OverrideCastFx ?? buildingGlobalUpgrade.CastFx)?.CastFxController.PlayCastFxs(TileObjectSelectionManager.E_Orientation.NONE, default(Vector2), base.BuildingUpgrade.Building);
		}
	}
}

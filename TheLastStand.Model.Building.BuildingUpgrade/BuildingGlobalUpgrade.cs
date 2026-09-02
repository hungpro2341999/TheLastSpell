using TPLib;
using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Serialization;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class BuildingGlobalUpgrade : BuildingUpgrade
{
	private BuildingGlobalUpgradeController BuildingGlobalUpgradeController => base.BuildingUpgradeController as BuildingGlobalUpgradeController;

	public BuildingUpgradeLevel BuildingUpgradeLevel { get; private set; }

	public override int UpgradeLevel
	{
		get
		{
			return BuildingUpgradeLevel.UpgradeLevel;
		}
		set
		{
			BuildingUpgradeLevel.UpgradeLevel = value;
		}
	}

	public BuildingGlobalUpgrade(SerializedUpgrade container, BuildingUpgradeDefinition definition, BuildingUpgradeController controller, Building building)
		: base(container, definition, controller, building)
	{
		BuildingUpgradeLevel = TPSingleton<BuildingManager>.Instance.GlobalItemProductionUpgradeLevel;
		BuildingUpgradeLevel.BuildingGlobalUpgrades.Add(this);
	}

	public BuildingGlobalUpgrade(BuildingUpgradeDefinition definition, BuildingUpgradeController controller, Building building)
		: base(definition, controller, building)
	{
		BuildingUpgradeLevel = TPSingleton<BuildingManager>.Instance.GlobalItemProductionUpgradeLevel;
		BuildingUpgradeLevel.BuildingGlobalUpgrades.Add(this);
	}

	public override void Deserialize(SerializedUpgrade buildingActionElement)
	{
		BuildingUpgradeLevel = TPSingleton<BuildingManager>.Instance.GlobalItemProductionUpgradeLevel;
		base.Deserialize(buildingActionElement);
	}

	public override ISerializedData Serialize()
	{
		return new SerializedGlobalUpgrade
		{
			Id = base.BuildingUpgradeDefinition.Id,
			UpgradeLevel = UpgradeLevel
		};
	}
}

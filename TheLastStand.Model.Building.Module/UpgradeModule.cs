using System.Collections.Generic;
using System.Linq;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Model.Building.Module;

public class UpgradeModule : BuildingModule
{
	public UpgradeModuleController UpgradeModuleController => base.BuildingModuleController as UpgradeModuleController;

	public UpgradeModuleDefinition UpgradeModuleDefinition => base.BuildingModuleDefinition as UpgradeModuleDefinition;

	public List<TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade> BuildingUpgrades { get; set; }

	public List<BuildingGlobalUpgrade> BuildingGlobalUpgrades { get; set; }

	public UpgradeModule(Building buildingParent, UpgradeModuleDefinition upgradeModuleDefinition, UpgradeModuleController upgradeModuleController)
		: base(buildingParent, upgradeModuleDefinition, upgradeModuleController)
	{
	}

	public void Serialize(SerializedBuilding buildingElement)
	{
		buildingElement.Upgrades = BuildingUpgrades?.Select((TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade o) => o.Serialize() as SerializedUpgrade)?.Where((SerializedUpgrade o) => o != null)?.ToList() ?? new List<SerializedUpgrade>();
		buildingElement.GlobalUpgrades = BuildingGlobalUpgrades?.Select((BuildingGlobalUpgrade o) => o.Serialize() as SerializedGlobalUpgrade)?.Where((SerializedGlobalUpgrade o) => o != null)?.ToList() ?? new List<SerializedGlobalUpgrade>();
	}

	public List<BuildingUpgradeEffect> GetActivatedBuildingUpgradeEffects()
	{
		List<BuildingUpgradeEffect> list = new List<BuildingUpgradeEffect>();
		for (int i = 0; i < BuildingUpgrades.Count; i++)
		{
			for (int j = 0; j < BuildingUpgrades[i].UpgradeLevel + 1; j++)
			{
				list.AddRange(BuildingUpgrades[i].BuildingUpgradeLevels[j].Effects);
			}
		}
		for (int k = 0; k < BuildingGlobalUpgrades.Count; k++)
		{
			for (int l = 0; l < BuildingGlobalUpgrades[k].UpgradeLevel + 1; l++)
			{
				list.AddRange(BuildingGlobalUpgrades[k].BuildingUpgradeLevels[l].Effects);
			}
		}
		return list;
	}
}

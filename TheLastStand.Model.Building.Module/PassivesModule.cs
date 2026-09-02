using System.Collections.Generic;
using System.Linq;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Model.Building.Module;

public class PassivesModule : BuildingModule
{
	public PassivesModuleController PassivesModuleController => base.BuildingModuleController as PassivesModuleController;

	public PassivesModuleDefinition PassivesModuleDefinition => base.BuildingModuleDefinition as PassivesModuleDefinition;

	public List<TheLastStand.Model.Building.BuildingPassive.BuildingPassive> BuildingPassives { get; set; }

	public PassivesModule(Building buildingParent, PassivesModuleDefinition passivesModuleDefinition, PassivesModuleController passivesModuleController)
		: base(buildingParent, passivesModuleDefinition, passivesModuleController)
	{
	}

	public List<EnemyUnit> GetLinkedEnemies()
	{
		List<EnemyUnit> list = new List<EnemyUnit>();
		foreach (TheLastStand.Model.Building.BuildingPassive.BuildingPassive buildingPassife in BuildingPassives)
		{
			foreach (BuildingPassiveEffect passiveEffect in buildingPassife.PassiveEffects)
			{
				if (passiveEffect is GenerateGuardian { Guardian: not null } generateGuardian)
				{
					list.Add(generateGuardian.Guardian);
				}
			}
		}
		return list;
	}

	public void Serialize(SerializedBuilding buildingElement)
	{
		buildingElement.Passives = BuildingPassives?.Select((TheLastStand.Model.Building.BuildingPassive.BuildingPassive o) => o.Serialize() as SerializedBuildingPassive)?.ToList() ?? new List<SerializedBuildingPassive>();
	}
}

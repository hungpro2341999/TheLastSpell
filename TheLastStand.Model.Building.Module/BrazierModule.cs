using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Serialization.Building;
using UnityEngine;

namespace TheLastStand.Model.Building.Module;

public class BrazierModule : BuildingModule
{
	public BrazierModuleController BrazierModuleController => base.BuildingModuleController as BrazierModuleController;

	public BrazierModuleDefinition BrazierModuleDefinition => base.BuildingModuleDefinition as BrazierModuleDefinition;

	public int BrazierPoints { get; set; }

	public int BrazierPointsTotal { get; set; }

	public bool IsExtinguishing { get; set; }

	public BrazierModule(Building buildingParent, BrazierModuleDefinition brazierDefinition, BrazierModuleController brazierModuleController)
		: base(buildingParent, brazierDefinition, brazierModuleController)
	{
		BrazierPointsTotal = brazierDefinition.BrazierPointsTotal;
		BrazierPoints = BrazierPointsTotal;
	}

	public void Deserialize(SerializedBuilding buildingElement)
	{
		BrazierPoints = Mathf.Min(buildingElement.BrazierPoints, BrazierPointsTotal);
	}

	public void Serialize(SerializedBuilding buildingElement)
	{
		buildingElement.BrazierPoints = BrazierPoints;
	}
}

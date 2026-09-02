using System.Collections.Generic;
using System.Linq;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Model.Building.Module;

public class ProductionModule : BuildingModule, ILevelOwner
{
	public ProductionModuleController ProductionModuleController => base.BuildingModuleController as ProductionModuleController;

	public ProductionModuleDefinition ProductionModuleDefinition => base.BuildingModuleDefinition as ProductionModuleDefinition;

	public List<TheLastStand.Model.Building.BuildingAction.BuildingAction> BuildingActions { get; set; }

	public TheLastStand.Model.Building.BuildingGaugeEffect.BuildingGaugeEffect BuildingGaugeEffect { get; set; }

	public int Level { get; set; }

	public string Name => base.BuildingParent.Name;

	public ProductionModule(Building buildingParent, ProductionModuleDefinition productionModuleDefinition, ProductionModuleController productionModuleController)
		: base(buildingParent, productionModuleDefinition, productionModuleController)
	{
		Level = productionModuleDefinition?.Level ?? 1;
	}

	public void Deserialize(SerializedBuilding buildingElement)
	{
		Level = buildingElement.Level;
	}

	public void Serialize(SerializedBuilding buildingElement)
	{
		buildingElement.Level = Level;
		buildingElement.GaugeEffect = BuildingGaugeEffect?.Serialize() as SerializedGaugeEffect;
		buildingElement.Actions = ProductionModuleController.SerializeUsedActions()?.ToList() ?? new List<SerializedBuildingAction>();
	}
}

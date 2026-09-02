using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Building.BuildingAction;

public abstract class BuildingActionEffect
{
	public ProductionModule ProductionBuilding { get; protected set; }

	public BuildingActionEffectController BuildingActionEffectController { get; protected set; }

	public BuildingActionEffectDefinition BuildingActionEffectDefinition { get; protected set; }

	public Tile Target { get; set; }

	public BuildingActionEffect(BuildingActionEffectDefinition definition, BuildingActionEffectController controller, ProductionModule productionBuilding)
	{
		BuildingActionEffectDefinition = definition;
		BuildingActionEffectController = controller;
		ProductionBuilding = productionBuilding;
	}
}

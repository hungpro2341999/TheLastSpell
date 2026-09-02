using System.Collections.Generic;
using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.CastFx;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Building.BuildingAction;

public class BuildingAction
{
	public ProductionModule ProductionBuilding { get; private set; }

	public BuildingActionController BuildingActionController { get; private set; }

	public BuildingActionDefinition BuildingActionDefinition { get; private set; }

	public List<BuildingActionEffect> BuildingActionEffects { get; set; }

	public TheLastStand.Model.CastFx.CastFx CastFx { get; set; }

	public bool IsExecutionInstant { get; set; }

	public Tile Target { get; set; }

	public int UsesPerTurnRemaining { get; set; }

	public BuildingAction(BuildingActionDefinition definition, BuildingActionController controller, ProductionModule productionBuilding)
	{
		BuildingActionDefinition = definition;
		BuildingActionController = controller;
		ProductionBuilding = productionBuilding;
		UsesPerTurnRemaining = definition.UsesPerTurnCount;
	}
}

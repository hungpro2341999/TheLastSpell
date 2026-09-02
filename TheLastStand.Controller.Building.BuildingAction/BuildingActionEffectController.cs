using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Building.BuildingAction;

public abstract class BuildingActionEffectController
{
	public BuildingActionEffect BuildingActionEffect { get; protected set; }

	protected BuildingActionEffectController(BuildingActionEffectDefinition definition, ProductionModule productionBuilding)
	{
	}

	public abstract bool CanExecuteActionEffectOnTile(Tile tile);

	public abstract void ExecuteActionEffect();
}

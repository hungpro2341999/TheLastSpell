using System.Collections.Generic;
using System.Linq;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.Command;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.LevelEditor.Conversation;

public class PlaceBuildingCommand : ICompensableCommand, ICommand
{
	public delegate void BuildingPlacedEventHandler(TheLastStand.Model.Building.Building building);

	private struct OverridenBuilding
	{
		public BuildingDefinition BuildingDefinition;

		public Tile Tile;
	}

	private BuildingDefinition buildingDefinition;

	private Tile tile;

	private List<OverridenBuilding> previousBuildings = new List<OverridenBuilding>();

	private BuildingDefinition previousTileBuildingDefinition;

	public static BuildingPlacedEventHandler BuildingPlaced;

	public PlaceBuildingCommand(BuildingDefinition buildingDefinition, Tile tile)
	{
		this.buildingDefinition = buildingDefinition;
		this.tile = tile;
		if (this.tile.Building != null)
		{
			previousTileBuildingDefinition = this.tile.Building.BuildingDefinition;
		}
		foreach (Tile occupiedTile in this.tile.GetOccupiedTiles(this.buildingDefinition.BlueprintModuleDefinition))
		{
			if (occupiedTile.Building != null && !previousBuildings.Any((OverridenBuilding o) => o.BuildingDefinition == occupiedTile.Building.BuildingDefinition))
			{
				previousBuildings.Add(new OverridenBuilding
				{
					BuildingDefinition = occupiedTile.Building.BuildingDefinition,
					Tile = occupiedTile.Building.OriginTile
				});
			}
		}
	}

	public void Compensate()
	{
		BuildingManager.DestroyBuilding(tile);
		foreach (OverridenBuilding previousBuilding in previousBuildings)
		{
			BuildingManager.DestroyBuilding(previousBuilding.Tile);
			BuildingManager.CreateBuilding(previousBuilding.BuildingDefinition, previousBuilding.Tile);
		}
	}

	public bool Execute()
	{
		if (buildingDefinition == previousTileBuildingDefinition)
		{
			return false;
		}
		foreach (OverridenBuilding previousBuilding in previousBuildings)
		{
			BuildingManager.DestroyBuilding(previousBuilding.Tile);
		}
		TheLastStand.Model.Building.Building building = BuildingManager.CreateBuilding(buildingDefinition, tile);
		BuildingPlaced?.Invoke(building);
		return true;
	}

	public override string ToString()
	{
		return "Place building " + buildingDefinition.Id;
	}
}

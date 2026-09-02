using TheLastStand.Definition.Building;
using TheLastStand.Framework.Command;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Manager.Building;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.LevelEditor.Conversation;

public class DestroyBuildingCommand : ICompensableCommand, ICommand
{
	private Tile tile;

	private BuildingDefinition destroyedBuildingDefinition;

	public DestroyBuildingCommand(Tile tile)
	{
		this.tile = tile;
	}

	public void Compensate()
	{
		BuildingManager.CreateBuilding(destroyedBuildingDefinition, tile);
	}

	public bool Execute()
	{
		if (tile.Building == null)
		{
			return false;
		}
		destroyedBuildingDefinition = tile.Building.BuildingDefinition;
		BuildingManager.DestroyBuilding(tile);
		return true;
	}

	public override string ToString()
	{
		return "Destroy building " + destroyedBuildingDefinition.Id;
	}
}

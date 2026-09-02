using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Model.Building;

public class BuildingToRestore : ISerializable, IDeserializable
{
	public Tile Tile { get; private set; }

	public string BuildingId { get; private set; }

	public int TrapUses { get; private set; }

	public BuildingToRestore(Tile tile, string buildingId, int trapUses)
	{
		Tile = tile;
		BuildingId = buildingId;
		TrapUses = trapUses;
	}

	public BuildingToRestore(ISerializedData container = null, int saveVersion = -1)
	{
		Deserialize(container, saveVersion);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedBuildingToRestore serializedBuildingToRestore = container as SerializedBuildingToRestore;
		Tile = TileMapManager.GetTile(serializedBuildingToRestore.TilePosition.X, serializedBuildingToRestore.TilePosition.Y);
		BuildingId = serializedBuildingToRestore.BuildingId;
		TrapUses = serializedBuildingToRestore.TrapUses;
	}

	public ISerializedData Serialize()
	{
		return new SerializedBuildingToRestore
		{
			TilePosition = new SerializableVector2Int(Tile.Position),
			BuildingId = BuildingId,
			TrapUses = TrapUses
		};
	}
}

using System.Xml.Serialization;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Serialization.Building;

public class SerializedBuildingToRestore : ISerializedData
{
	public SerializableVector2Int TilePosition;

	[XmlAttribute]
	public string BuildingId;

	[XmlAttribute]
	public int TrapUses;
}

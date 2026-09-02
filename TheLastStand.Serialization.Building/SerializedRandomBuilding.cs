using System.Xml.Serialization;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Serialization.Building;

public class SerializedRandomBuilding : ISerializedData
{
	public SerializableVector2Int TilePosition;

	[XmlAttribute]
	public string BuildingId;
}

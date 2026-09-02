using System.Xml.Serialization;

namespace TheLastStand.Serialization.Unit;

public class SerializedGoal : ISerializedData
{
	[XmlAttribute]
	public string Id;

	[XmlAttribute]
	public int Cooldown;
}

using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedBossPhaseActorsKills : ISerializedData
{
	[XmlAttribute]
	public string ActorId;

	[XmlAttribute]
	public int Amount;
}

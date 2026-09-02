using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedBossPhaseHandler : ISerializedData
{
	[XmlAttribute]
	public string Id;

	[XmlAttribute]
	public bool IsLocked;
}

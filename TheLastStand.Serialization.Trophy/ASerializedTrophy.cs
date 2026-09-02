using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Trophy;

[Serializable]
public abstract class ASerializedTrophy : ISerializedData
{
	[XmlAttribute]
	public string Name;
}

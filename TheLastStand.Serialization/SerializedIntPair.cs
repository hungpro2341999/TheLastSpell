using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization;

[Serializable]
public struct SerializedIntPair : ISerializedData
{
	[XmlAttribute]
	public int UnitId;

	[XmlAttribute]
	public int Value;
}

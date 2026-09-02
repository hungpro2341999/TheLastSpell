using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Meta;

[Serializable]
public class SerializedMetaUpgrade : ISerializedData
{
	[XmlAttribute]
	public string Id;

	[XmlAttribute]
	public uint InvestedSouls;
}

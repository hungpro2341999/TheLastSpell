using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Perk;

[Serializable]
[XmlInclude(typeof(SerializedGaugeModule))]
public class SerializedBufferModule : SerializedModule, ISerializedData
{
	public int Buffer;

	public int Buffer2;

	public int Buffer3;

	public int Buffer4;

	public int Buffer5;

	public int Buffer6;

	public int Buffer7;

	public int Buffer8;

	public int Buffer9;
}

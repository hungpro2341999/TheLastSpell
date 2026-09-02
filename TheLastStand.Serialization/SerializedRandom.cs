using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedRandom : ISerializedData
{
	[XmlAttribute]
	public string CallerID;

	public byte[] CurrentState;

	public byte[] SavedState;
}

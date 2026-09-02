using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Perk;

[Serializable]
[XmlInclude(typeof(SerializedBufferModule))]
public class SerializedModule : ISerializedData
{
}

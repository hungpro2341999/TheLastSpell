using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedDelayedBossPhaseAction : ISerializedData
{
	[XmlAttribute]
	public int Delay;

	[XmlAttribute]
	public int ActionIndex;

	[XmlAttribute]
	public string HandlerId;

	[XmlAttribute]
	public string PhaseId;
}

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedBossPhase : ISerializedData
{
	[XmlAttribute]
	public int PhaseStartedAtTurn;

	public List<SerializedBossPhaseHandler> BossPhaseHandlers;
}

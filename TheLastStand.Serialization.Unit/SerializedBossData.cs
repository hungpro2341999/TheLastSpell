using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedBossData : ISerializedData
{
	public List<SerializedEnemyUnit> BossUnits = new List<SerializedEnemyUnit>();

	[XmlAttribute]
	public string CurrentBossPhaseId = string.Empty;

	[XmlAttribute]
	public float NightProgressionValue;

	public SerializedBossPhase BossPhase;

	public List<SerializedBossPhaseActorsKills> BossPhaseActorsKills = new List<SerializedBossPhaseActorsKills>();

	public List<SerializedDelayedBossPhaseAction> PendingBossPhaseActions = new List<SerializedDelayedBossPhaseAction>();
}

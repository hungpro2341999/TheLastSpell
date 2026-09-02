using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public abstract class ASerializedEnemyUnit : ISerializedData
{
	[XmlAttribute]
	public string Id;

	[XmlAttribute]
	public string BossPhaseActorId;

	[XmlAttribute]
	public int OverrideVariantId;

	public int? LinkedBuilding;

	public bool IsGuardian;

	public bool IgnoreFromEnemyUnitsCount;

	public int LastHourInFog;

	public int LastHourInAnyFog;

	public SerializedBehavior SerializedBehavior;

	public SerializedUnit Unit;
}

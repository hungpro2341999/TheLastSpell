using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Serialization.Building;

[Serializable]
public class SerializedBuilding : ISerializedData
{
	[XmlAttribute]
	public string BossPhaseActorId;

	public string Id;

	public int RandomId = -1;

	public SerializableVector2Int Position;

	public float Health;

	public int Level;

	public int RemainingTrapUsage;

	public int BrazierPoints;

	public List<SerializedSkill> Skills = new List<SerializedSkill>();

	public SerializedGaugeEffect GaugeEffect;

	public List<SerializedBuildingAction> Actions = new List<SerializedBuildingAction>();

	public List<SerializedBuildingPassive> Passives = new List<SerializedBuildingPassive>();

	public List<SerializedUpgrade> Upgrades = new List<SerializedUpgrade>();

	public List<SerializedGlobalUpgrade> GlobalUpgrades = new List<SerializedGlobalUpgrade>();

	public SerializedBehavior SerializedBehavior;

	public SerializedMagicCicleSettings MagicCircleSettings;
}

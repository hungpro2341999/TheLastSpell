using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TheLastStand.Serialization.Perk;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedPlayableUnit : ISerializedData
{
	[XmlAttribute]
	public string Name;

	public string ArchetypeId;

	public bool IsStartingUnit;

	public SerializedPortrait Portrait;

	public float LastTurnHealth;

	public double Level;

	public int PerksPoints;

	public List<SerializedLevelUpPoint> SerializedLevelUpPoints;

	public float Experience;

	public float ExperienceInCurrentLevel;

	public int EquippedWeaponSetIndex;

	public SerializedLevelUpBonuses LevelUp;

	public List<SerializedEquipmentSlot> EquipmentSlots = new List<SerializedEquipmentSlot>();

	public List<string> Traits = new List<string>();

	public List<SerializedPerkCollection> PerkCollections = new List<SerializedPerkCollection>();

	public List<SerializedPerk> NativePerks = new List<SerializedPerk>();

	public List<SerializedPerk> RacePerks = new List<SerializedPerk>();

	public List<SerializedPerk> DynamicPerks = new List<SerializedPerk>();

	public SerializedLifetimeStats LifetimeStats;

	public List<SerializedSkill> ContextualSkills = new List<SerializedSkill>();

	public bool MovedThisDay;

	public bool HelmetDisplayed = true;

	public SerializedUnitStats Stats;

	public int ActionPointsSpentThisTurn;

	public int MomentumTilesActive;

	public int TotalMomentumTilesCrossedThisTurn;

	public int TilesCrossedThisTurn;

	public int PerkRerollCount;

	public SerializedUnit Unit;

	public string RaceId;

	public SerializedPlayableUnit()
	{
	}

	public SerializedPlayableUnit(SerializedUnit baseUnit)
	{
		Unit = baseUnit;
	}
}

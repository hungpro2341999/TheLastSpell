using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TheLastStand.Definition.Item;
using TheLastStand.Serialization.Perk;

namespace TheLastStand.Serialization.Item;

[Serializable]
public class SerializedItem : ISerializedData
{
	[XmlAttribute]
	public string Id;

	[XmlAttribute]
	public int Level;

	[XmlAttribute]
	public int Resistance;

	[XmlAttribute]
	public ItemDefinition.E_Rarity Rarity;

	[XmlAttribute]
	public bool HasBeenSoldBefore;

	public List<SerializedSkill> Skills = new List<SerializedSkill>();

	public List<SerializedSkill> PerkReplacementSkills = new List<SerializedSkill>();

	public List<SerializedAffix> Affixes = new List<SerializedAffix>();

	public List<SerializedAffixMalus> AffixesMalus = new List<SerializedAffixMalus>();

	public List<SerializedPerk> Perks = new List<SerializedPerk>();
}

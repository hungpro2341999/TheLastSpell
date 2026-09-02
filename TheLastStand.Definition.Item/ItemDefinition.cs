using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class ItemDefinition : TheLastStand.Framework.Serialization.Definition
{
	[Flags]
	public enum E_Category
	{
		None = 0,
		MeleeWeapon = 1,
		RangeWeapon = 2,
		MagicWeapon = 4,
		Shield = 8,
		ClothBodyArmor = 0x10,
		LightBodyArmor = 0x20,
		MediumBodyArmor = 0x40,
		HeavyBodyArmor = 0x80,
		ClothHelm = 0x100,
		LightHelm = 0x200,
		MediumHelm = 0x400,
		HeavyHelm = 0x800,
		ClothBoots = 0x1000,
		LightBoots = 0x2000,
		MediumBoots = 0x4000,
		HeavyBoots = 0x8000,
		Trinket = 0x10000,
		Utility = 0x20000,
		Potion = 0x40000,
		Scroll = 0x80000,
		Usable = 0xC0000,
		Weapon = 7,
		Helm = 0xF00,
		Boots = 0xF000,
		BodyArmor = 0xF0,
		Armor = 0xFFF0,
		Equipment = 0x1FFF8,
		OffHand = 0x20008,
		All = 0xFFFFF
	}

	public enum E_Hands
	{
		None,
		OneHand,
		TwoHands,
		OffHand
	}

	public enum E_Rarity
	{
		None,
		Common,
		Magic,
		Rare,
		Epic
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CategoryComparer : IEqualityComparer<E_Category>
	{
		public bool Equals(E_Category x, E_Category y)
		{
			return x == y;
		}

		public int GetHashCode(E_Category obj)
		{
			return (int)obj;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct RarityComparer : IEqualityComparer<E_Rarity>
	{
		public bool Equals(E_Rarity x, E_Rarity y)
		{
			return x == y;
		}

		public int GetHashCode(E_Rarity obj)
		{
			return (int)obj;
		}
	}

	public static readonly CategoryComparer SharedCategoryComparer;

	public static readonly RarityComparer SharedRarityComparer;

	private string artId = string.Empty;

	private List<int> definedLevels;

	public HashSet<string> Tags = new HashSet<string>();

	public string ArtId
	{
		get
		{
			if (!(artId != string.Empty))
			{
				return Id;
			}
			return artId;
		}
	}

	public Dictionary<int, Vector2> BaseDamageByLevel { get; } = new Dictionary<int, Vector2>();

	public string BaseName => Localizer.Get("ItemName_" + Id);

	public Dictionary<int, float> BasePriceByLevel { get; } = new Dictionary<int, float>();

	public Dictionary<int, Dictionary<UnitStatDefinition.E_Stat, float>> BaseStatBonusesByLevel { get; } = new Dictionary<int, Dictionary<UnitStatDefinition.E_Stat, float>>();

	public Dictionary<string, BodyPartDefinition> BodyPartsDefinitions { get; private set; }

	public E_Category Category { get; private set; }

	public string CategoryName => Category.GetLocalizedName();

	public E_Hands Hands { get; private set; }

	public string HandsName => Localizer.Get(string.Format("{0}{1}", "HandsName_", Hands));

	public string Id { get; private set; }

	public bool IsWeapon
	{
		get
		{
			if (Category != E_Category.MagicWeapon && Category != E_Category.MeleeWeapon)
			{
				return Category == E_Category.RangeWeapon;
			}
			return true;
		}
	}

	public bool IsHandItem => Hands != E_Hands.None;

	public bool IsOffHand => Hands == E_Hands.OffHand;

	public bool IsMainHand
	{
		get
		{
			if (Hands != E_Hands.OneHand)
			{
				return Hands == E_Hands.TwoHands;
			}
			return true;
		}
	}

	public Dictionary<int, Tuple<UnitStatDefinition.E_Stat, float>> MainStatBonusByLevel { get; } = new Dictionary<int, Tuple<UnitStatDefinition.E_Stat, float>>();

	public Vector2Int Resistance { get; private set; }

	public Dictionary<int, HashSet<string>> PerksByLevel { get; } = new Dictionary<int, HashSet<string>>();

	public Dictionary<int, Dictionary<string, int>> SkillsByLevel { get; } = new Dictionary<int, Dictionary<string, int>>();

	public ItemDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("An item hasn't an Id !", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		artId = Id;
		XElement xElement2 = xElement.Element("Category");
		if (xElement2 != null)
		{
			if (Enum.TryParse<E_Category>(xElement2.Value, out var result))
			{
				Category = result;
				XElement xElement3 = xElement.Element("Tags");
				if (xElement3 != null)
				{
					foreach (XElement item in xElement3.Elements("Tag"))
					{
						string value = item.Value;
						if (ItemDatabase.ItemsByTag.ContainsKey(value))
						{
							ItemDatabase.ItemsByTag[value].Add(Id);
						}
						else
						{
							ItemDatabase.ItemsByTag.Add(value, new List<string> { Id });
						}
						if (!Tags.Contains(value))
						{
							Tags.Add(value);
						}
					}
				}
				XElement xElement4 = xElement.Element("Hands");
				if (xElement4 != null)
				{
					if (!Enum.TryParse<E_Hands>(xElement4.Value, out var result2))
					{
						CLoggerManager.Log("Item " + Id + "'s Hands " + HasAnInvalid("E_Hands", xElement4.Value), LogType.Error);
						return;
					}
					Hands = result2;
				}
				XElement xElement5 = xElement.Element(UnitStatDefinition.E_Stat.Resistance.ToString());
				if (xElement5 != null)
				{
					Resistance = xElement5.ParseMinMax();
				}
				XElement xElement6 = xElement.Element("LevelVariations");
				Vector2 value2 = Vector2.zero;
				float value3 = -1f;
				Dictionary<UnitStatDefinition.E_Stat, float> value4 = null;
				Dictionary<string, int> value5 = null;
				HashSet<string> value6 = null;
				definedLevels = new List<int>();
				{
					foreach (XElement item2 in xElement6.Elements("Level"))
					{
						XAttribute xAttribute2 = item2.Attribute("Id");
						if (!int.TryParse(xAttribute2.Value, out var result3))
						{
							CLoggerManager.Log("Item " + Id + "'s Level " + HasAnInvalidInt(xAttribute2.Value), LogType.Error);
							continue;
						}
						definedLevels.Add(result3);
						XElement xElement7 = item2.Element("BaseDamage");
						if (xElement7 != null)
						{
							XAttribute xAttribute3 = xElement7.Attribute("Min");
							if (xAttribute3.IsNullOrEmpty())
							{
								CLoggerManager.Log("The BaseDamage " + OfTheItem(Id, result3) + " hasn't a Min !", LogType.Error);
								break;
							}
							if (!int.TryParse(xAttribute3.Value, out var result4))
							{
								CLoggerManager.Log($"Item {Id}(Level : {result3})'s BaseDamage Min {HasAnInvalidInt(xAttribute3.Value)}", LogType.Error);
								break;
							}
							XAttribute xAttribute4 = xElement7.Attribute("Max");
							if (xAttribute4.IsNullOrEmpty())
							{
								CLoggerManager.Log("The BaseDamage " + OfTheItem(Id, result3) + " hasn't a Max !", LogType.Error);
								break;
							}
							if (!int.TryParse(xAttribute4.Value, out var result5))
							{
								CLoggerManager.Log($"Item {Id}(Level : {result3})'s BaseDamage Max {HasAnInvalidInt(xAttribute4.Value)}", LogType.Error);
								break;
							}
							Vector2 vector = new Vector2(result4, result5);
							value2 = vector;
							BaseDamageByLevel.Add(result3, vector);
						}
						else
						{
							BaseDamageByLevel.Add(result3, value2);
						}
						XElement xElement8 = item2.Element("BasePrice");
						if (xElement8 != null)
						{
							if (!float.TryParse(xElement8.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result6))
							{
								CLoggerManager.Log($"Item {Id}(Level : {result3})'s Price {HasAnInvalidFloat(xElement8.Value)}", LogType.Error);
								break;
							}
							value3 = result6;
							BasePriceByLevel.Add(result3, result6);
						}
						else
						{
							BasePriceByLevel.Add(result3, value3);
						}
						BaseStatBonusesByLevel.Add(result3, new Dictionary<UnitStatDefinition.E_Stat, float>());
						XElement xElement9 = item2.Element("BaseStatBonuses");
						if (xElement9 != null)
						{
							foreach (XElement item3 in xElement9.Elements("BaseStatBonus"))
							{
								XAttribute xAttribute5 = item3.Attribute("Stat");
								UnitStatDefinition.E_Stat result7;
								float result8;
								if (xAttribute5.IsNullOrEmpty())
								{
									CLoggerManager.Log("A BaseStatBonus " + OfTheItem(Id, result3) + " hasn't a Stat!", LogType.Error);
								}
								else if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute5.Value, out result7))
								{
									CLoggerManager.Log("A BaseStatBonus " + OfTheItem(Id, result3) + " " + HasAnInvalidStat(xAttribute5.Value), LogType.Error);
								}
								else if (item3.IsNullOrEmpty())
								{
									CLoggerManager.Log("A BaseStatBonus (" + result7.ToString() + ") " + OfTheItem(Id, result3) + " is empty !", LogType.Error);
								}
								else if (!float.TryParse(item3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result8))
								{
									CLoggerManager.Log("A BaseStatBonus (" + result7.ToString() + ") " + OfTheItem(Id, result3) + " " + HasAnInvalidFloat(item3.Value), LogType.Error);
								}
								else
								{
									BaseStatBonusesByLevel[result3].Add(result7, result8);
								}
							}
							value4 = BaseStatBonusesByLevel[result3];
						}
						else
						{
							BaseStatBonusesByLevel[result3] = value4;
						}
						XElement xElement10 = item2.Element("MainStatBonus");
						if (!xElement10.IsNullOrEmpty())
						{
							XAttribute xAttribute6 = xElement10.Attribute("Stat");
							if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute6.Value, out var result9))
							{
								CLoggerManager.Log("The MainStatBonus " + OfTheItem(Id, result3) + " " + HasAnInvalidStat(xAttribute6.Value), LogType.Error);
							}
							if (!float.TryParse(xElement10.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result10))
							{
								CLoggerManager.Log("The MainStatBonus " + OfTheItem(Id, result3) + " " + HasAnInvalidFloat(xElement10.Value), LogType.Error);
								break;
							}
							MainStatBonusByLevel.Add(result3, new Tuple<UnitStatDefinition.E_Stat, float>(result9, result10));
						}
						else
						{
							MainStatBonusByLevel.Add(result3, null);
						}
						SkillsByLevel.Add(result3, null);
						XElement xElement11 = item2.Element("Skills");
						if (xElement11 != null)
						{
							SkillsByLevel[result3] = new Dictionary<string, int>();
							foreach (XElement item4 in xElement11.Elements())
							{
								if (item4.IsNullOrEmpty())
								{
									CLoggerManager.Log("A skill " + OfTheItem(Id, result3) + " is Empty !", LogType.Error);
									continue;
								}
								int result11 = -1;
								XAttribute xAttribute7 = item4.Attribute("OverallUsesCount");
								if (xAttribute7 != null && !int.TryParse(xAttribute7.Value, out result11))
								{
									CLoggerManager.Log("The skill " + item4.Value + " " + OfTheItem(Id, result3) + " " + HasAnInvalidInt(xAttribute7.Value), LogType.Error);
								}
								else
								{
									SkillsByLevel[result3].Add(item4.Value, result11);
								}
							}
							value5 = SkillsByLevel[result3];
						}
						else
						{
							SkillsByLevel[result3] = value5;
						}
						PerksByLevel.Add(result3, null);
						XElement xElement12 = item2.Element("Perks");
						if (xElement12 != null)
						{
							PerksByLevel[result3] = new HashSet<string>();
							foreach (XElement item5 in xElement12.Elements())
							{
								if (item5.IsNullOrEmpty())
								{
									CLoggerManager.Log("A Perk " + OfTheItem(Id, result3) + " is Empty !", LogType.Error);
								}
								else if (!PerksByLevel[result3].Contains(item5.Value))
								{
									PerksByLevel[result3].Add(item5.Value);
								}
							}
							value6 = PerksByLevel[result3];
						}
						else
						{
							PerksByLevel[result3] = value6;
						}
					}
					return;
				}
			}
			CLoggerManager.Log("Item " + Id + "'s Category " + HasAnInvalid("E_Category", xElement2.Value), LogType.Error);
		}
		else
		{
			CLoggerManager.Log("Item " + Id + " must have a Category", LogType.Error);
		}
	}

	public void DeserializeArtRelatedDatas(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("ArtId");
		if (!xElement.IsNullOrEmpty())
		{
			artId = xElement.Value;
		}
		else
		{
			artId = Id;
		}
		XElement xElement2 = obj.Element("BodyParts");
		if (xElement2 == null)
		{
			return;
		}
		BodyPartsDefinitions = new Dictionary<string, BodyPartDefinition>();
		foreach (XElement item in xElement2.Elements("BodyPartDefinition"))
		{
			BodyPartDefinition bodyPartDefinition = new BodyPartDefinition(item);
			BodyPartsDefinitions.Add(bodyPartDefinition.Id, bodyPartDefinition);
		}
	}

	public int GetHigherExistingLevelFromInitValue(int level)
	{
		while (level > -1)
		{
			if (definedLevels.Contains(level))
			{
				return level;
			}
			level--;
		}
		return -1;
	}

	public int GetLowerExistingLevelFromInitValue(int level)
	{
		while (level < 999)
		{
			if (definedLevels.Contains(level))
			{
				return level;
			}
			level++;
		}
		return -1;
	}

	public bool HasTag(string tag)
	{
		if (ItemDatabase.ItemsByTag.TryGetValue(tag, out var value))
		{
			return value.Contains(Id);
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class AffixDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class LeveledAffixDefinition : TheLastStand.Framework.Serialization.Definition
	{
		public AffixDefinition AffixDefinition { get; private set; }

		public int Level { get; private set; }

		public Dictionary<UnitStatDefinition.E_Stat, float> StatModifiers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, float>(UnitStatDefinition.SharedStatComparer);

		public LeveledAffixDefinition(AffixDefinition affixDefinition, XContainer container)
			: base(container)
		{
			AffixDefinition = affixDefinition;
		}

		public override void Deserialize(XContainer container)
		{
			XElement xElement = container as XElement;
			XAttribute xAttribute = xElement.Attribute("Id");
			if (xAttribute.IsNullOrEmpty())
			{
				Debug.LogError("The Level has no Id!");
				return;
			}
			if (!int.TryParse(xAttribute.Value, out var result) || result < 1 || result > 10)
			{
				Debug.LogError("The Level (" + xAttribute.Value + ") is invalid!");
				return;
			}
			Level = result;
			foreach (XElement item in xElement.Elements("Modifier"))
			{
				if (item.IsNullOrEmpty())
				{
					Debug.LogError("The Modifier is empty!");
					continue;
				}
				XAttribute xAttribute2 = item.Attribute("Stat");
				if (xAttribute2.IsNullOrEmpty())
				{
					Debug.LogError("The Modifier has no Stat!");
				}
				else
				{
					StatModifiers.Add((UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), xAttribute2.Value), float.Parse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture));
				}
			}
		}
	}

	public bool Droppable { get; private set; } = true;

	public Dictionary<UnitStatDefinition.E_Stat, float> EpicStatModifiers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, float>(UnitStatDefinition.SharedStatComparer);

	public string Id { get; private set; }

	public Dictionary<ItemDefinition.E_Category, float> ItemCategoriesWithWeight { get; private set; } = new Dictionary<ItemDefinition.E_Category, float>(ItemDefinition.SharedCategoryComparer);

	public Dictionary<int, LeveledAffixDefinition> LevelDefinitions { get; private set; } = new Dictionary<int, LeveledAffixDefinition>();

	public int LevelMax { get; private set; }

	public int LevelMin { get; private set; }

	public int MaxOccurrences { get; private set; } = -1;

	public float TotalCategoryWeight { get; private set; }

	public AffixDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("The AffixDefinition has no Id!");
			return;
		}
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("MaxOccurrences");
		if (xAttribute2 != null)
		{
			if (int.TryParse(xAttribute2.Value, out var result))
			{
				MaxOccurrences = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse MaxOccurrences attribute into an int : " + xAttribute2.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "AffixDefinition");
			}
		}
		XAttribute xAttribute3 = xElement.Attribute("Droppable");
		if (xAttribute3 != null)
		{
			if (!bool.TryParse(xAttribute3.Value, out var result2))
			{
				Debug.LogError("AffixDefinition " + Id + " has an invalid Droppable!");
				return;
			}
			Droppable = result2;
		}
		XElement xElement2 = xElement.Element("ItemLevel");
		if (xElement2 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no ItemLevel!");
			return;
		}
		XAttribute xAttribute4 = xElement2.Attribute("Min");
		if (xAttribute4 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no ItemLevel Min!");
			return;
		}
		if (!int.TryParse(xAttribute4.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3) || result3 < 0)
		{
			Debug.LogError("The AffixDefinition " + Id + " has an invalid ItemLevel Min " + xAttribute4.Value + "!");
			return;
		}
		LevelMin = result3;
		XAttribute xAttribute5 = xElement2.Attribute("Max");
		if (xAttribute5 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no ItemLevel Max!");
			return;
		}
		if (!int.TryParse(xAttribute5.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result4) || result4 < 0)
		{
			Debug.LogError("The AffixDefinition " + Id + " has an invalid ItemLevel Max " + xAttribute5.Value + "!");
			return;
		}
		LevelMax = result4;
		XElement xElement3 = xElement.Element("ItemCategories");
		if (xElement3 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no ItemCategories!");
			return;
		}
		foreach (XElement item in xElement3.Elements("ItemCategory"))
		{
			if (item.IsNullOrEmpty())
			{
				Debug.LogError("AffixDefinition " + Id + "'s ItemCategory is empty!");
				continue;
			}
			if (!Enum.TryParse<ItemDefinition.E_Category>(item.Value, out var result5))
			{
				Debug.LogError("AffixDefinition " + Id + "'s ItemCategory " + HasAnInvalid("E_Category", item.Value));
				continue;
			}
			if (ItemCategoriesWithWeight.ContainsKey(result5))
			{
				Debug.LogError($"The affix {Id} already contains an  ItemCategory {result5}!");
				continue;
			}
			XAttribute xAttribute6 = item.Attribute("Weight");
			if (xAttribute6.IsNullOrEmpty())
			{
				TPDebug.Log($"The affix {Id} must have a Weight to its ItemCategory {result5}");
				return;
			}
			if (!float.TryParse(xAttribute6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result6))
			{
				TPDebug.Log($"The affix {Id} must have a valid Weight (float) to its ItemCategory {result5}");
				return;
			}
			ItemCategoriesWithWeight.Add(result5, result6);
			TotalCategoryWeight += result6;
		}
		XElement xElement4 = xElement.Element("Levels");
		if (xElement4 == null)
		{
			Debug.LogError("The AffixDefinition " + Id + " has no Levels!");
			return;
		}
		foreach (XElement item2 in xElement4.Elements("Level"))
		{
			LeveledAffixDefinition leveledAffixDefinition = new LeveledAffixDefinition(this, item2);
			LevelDefinitions.Add(leveledAffixDefinition.Level, leveledAffixDefinition);
		}
		XElement xElement5 = xElement.Element("EpicBonus");
		if (xElement5.IsNullOrEmpty())
		{
			Debug.LogError("The AffixDefinition " + Id + " has no EpicBonus!");
			return;
		}
		foreach (XElement item3 in xElement5.Elements("Modifier"))
		{
			if (item3.IsNullOrEmpty())
			{
				Debug.LogError("The Modifier is empty!");
				continue;
			}
			XAttribute xAttribute7 = item3.Attribute("Stat");
			if (xAttribute7.IsNullOrEmpty())
			{
				Debug.LogError("The Modifier has no Stat!");
			}
			else
			{
				EpicStatModifiers.Add((UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), xAttribute7.Value), float.Parse(item3.Value, NumberStyles.Float, CultureInfo.InvariantCulture));
			}
		}
	}
}

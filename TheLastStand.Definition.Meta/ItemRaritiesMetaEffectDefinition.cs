using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Item;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class ItemRaritiesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "ItemRaritiesModifier";

	public string RarityTreeId { get; private set; }

	public Dictionary<int, int> WeightBonusByRarityLevel { get; set; } = new Dictionary<int, int>();

	public ItemRaritiesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		if (xAttribute == null || string.IsNullOrEmpty(xAttribute.Value))
		{
			Debug.LogError("ItemRaritiesModifier has an invalid Id or Id doesn't exist !");
		}
		RarityTreeId = xAttribute.Value;
		foreach (XElement item in obj.Elements("Probability"))
		{
			XAttribute xAttribute2 = item.Attribute("Weight");
			int result2;
			if (xAttribute2 == null || !int.TryParse(xAttribute2.Value, out var result))
			{
				Debug.LogError("Probability has an invalid Weight or Weight doesn't exist !");
			}
			else if (!int.TryParse(item.Value, out result2) || result2 >= Enum.GetValues(typeof(ItemDefinition.E_Rarity)).Length || result2 < 0)
			{
				Debug.LogError("Probability has an invalid Value or Value can't be parse as E_Rarity !");
			}
			else if (WeightBonusByRarityLevel.ContainsKey(result2))
			{
				WeightBonusByRarityLevel[result2] += result;
			}
			else
			{
				WeightBonusByRarityLevel.Add(result2, result);
			}
		}
	}

	public override string ToString()
	{
		string text = "Rarity Probability Tree : <b>" + RarityTreeId + "</b> Modifications :\r\n";
		foreach (KeyValuePair<int, int> item in WeightBonusByRarityLevel)
		{
			text += $"\tRarityLevel : <b>{item.Key}</b> ; Bonus Weight : <b>{item.Value}</b>;\r\n";
		}
		return text;
	}
}

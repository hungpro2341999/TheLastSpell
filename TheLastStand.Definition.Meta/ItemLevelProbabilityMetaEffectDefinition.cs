using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class ItemLevelProbabilityMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "ItemLevelProbabilityModifier";

	public string LevelTreeId { get; private set; }

	public Dictionary<int, int> WeightBonusByLevelProbability { get; set; } = new Dictionary<int, int>();

	public ItemLevelProbabilityMetaEffectDefinition(XContainer container)
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
			Debug.LogError("ItemLevelProbabilityModifier has an invalid Id or Id doesn't exist !");
		}
		LevelTreeId = xAttribute.Value;
		foreach (XElement item in obj.Elements("Probability"))
		{
			XAttribute xAttribute2 = item.Attribute("Weight");
			int result2;
			if (xAttribute2 == null || !int.TryParse(xAttribute2.Value, out var result))
			{
				Debug.LogError("Probability has an invalid Weight or Weight doesn't exist !");
			}
			else if (!int.TryParse(item.Value, out result2))
			{
				Debug.LogError("Probability has an invalid Value !");
			}
			else if (WeightBonusByLevelProbability.ContainsKey(result2))
			{
				WeightBonusByLevelProbability[result2] += result;
			}
			else
			{
				WeightBonusByLevelProbability.Add(result2, result);
			}
		}
	}

	public override string ToString()
	{
		string text = "Level Probability Tree : <b>" + LevelTreeId + "</b> Modifications :\r\n";
		foreach (KeyValuePair<int, int> item in WeightBonusByLevelProbability)
		{
			text += $"\tLevel : <b>{item.Key}</b> ; Bonus Weight : <b>{item.Value}</b>;\r\n";
		}
		return text;
	}
}

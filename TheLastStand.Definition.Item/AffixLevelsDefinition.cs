using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class AffixLevelsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<int, Dictionary<int, float>> AffixLevelsProbas { get; private set; } = new Dictionary<int, Dictionary<int, float>>();

	public Dictionary<int, Dictionary<AffixMalusDefinition.E_MalusLevel, float>> AffixMalusLevelsProbas { get; private set; } = new Dictionary<int, Dictionary<AffixMalusDefinition.E_MalusLevel, float>>();

	public AffixLevelsDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("ItemLevel"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			if (xAttribute.IsNullOrEmpty())
			{
				Debug.LogError("AffixLevelsDefinition ItemLevel must have an Id");
				continue;
			}
			if (!int.TryParse(xAttribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("AffixLevelsDefinition ItemLevel must have a valid Id (int)", LogType.Error);
				continue;
			}
			if (AffixLevelsProbas.ContainsKey(result))
			{
				CLoggerManager.Log($"AffixLevelsDefinition already have this Id : {result}", LogType.Error);
				continue;
			}
			AffixLevelsProbas.Add(result, new Dictionary<int, float>());
			foreach (XElement item2 in item.Elements("AffixLevelProba"))
			{
				XAttribute xAttribute2 = item2.Attribute("Id");
				if (xAttribute2.IsNullOrEmpty())
				{
					CLoggerManager.Log($"AffixLevelsDefinition {result}'s AffixLevelProba must have an Id", LogType.Error);
					continue;
				}
				if (!int.TryParse(xAttribute2.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2) && result2 > 0 && result2 < 4)
				{
					CLoggerManager.Log($"AffixLevelsDefinition {result}'s AffixLevelProba {HasAnInvalidInt(xAttribute2.Value)}", LogType.Error);
					continue;
				}
				if (AffixLevelsProbas[result].ContainsKey(result2))
				{
					CLoggerManager.Log($"AffixLevelsDefinition ItemLevel with id {result} already have an AffixLevelProba this Id : {result2}", LogType.Error);
					continue;
				}
				if (item2.IsEmpty || !float.TryParse(item2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
				{
					CLoggerManager.Log("AffixLevelsDefinition AffixLevelProba must be a valid float", LogType.Error);
					return;
				}
				AffixLevelsProbas[result].Add(result2, result3);
			}
		}
		foreach (KeyValuePair<int, Dictionary<int, float>> affixLevelsProba in AffixLevelsProbas)
		{
			AffixMalusLevelsProbas.Add(affixLevelsProba.Key, new Dictionary<AffixMalusDefinition.E_MalusLevel, float>(AffixMalusDefinition.SharedMalusLevelComparer));
			foreach (KeyValuePair<int, float> item3 in affixLevelsProba.Value)
			{
				AffixMalusLevelsProbas[affixLevelsProba.Key].Add((AffixMalusDefinition.E_MalusLevel)item3.Key, item3.Value);
			}
		}
	}
}

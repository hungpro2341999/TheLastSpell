using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SpawnWaveEnemiesDefinition : TheLastStand.Framework.Serialization.Definition
{
	public BossWaveSettings BossWaveSettings { get; private set; }

	public Dictionary<string, int> EliteEnemyUnitTemplateDefinitions { get; private set; } = new Dictionary<string, int>();

	public bool EliteOverrideSpawnDefinition { get; private set; }

	public Dictionary<string, int> EnemyUnitTemplateDefinitions { get; private set; } = new Dictionary<string, int>();

	public List<Tuple<int, float>> EnemyTierDefinitions { get; private set; } = new List<Tuple<int, float>>();

	public List<int> AllTiers { get; private set; } = new List<int>();

	public List<SeerAdditionalPortraitSettings> SeerAdditionalPortraitsSettings { get; private set; } = new List<SeerAdditionalPortraitSettings>();

	public SpawnWaveEnemiesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("EnemyUnitTemplateDefinitions");
		if (xElement2 == null)
		{
			CLoggerManager.Log("SpawnWaveDefinition has no EnemyUnitTemplateDefinitions!", LogType.Error);
			return;
		}
		XElement xElement3 = xElement.Element("BossWaveSettings");
		if (xElement3 != null)
		{
			BossWaveSettings = new BossWaveSettings(xElement3);
		}
		foreach (XElement item2 in xElement2.Elements("EnemyUnitTemplateDefinition"))
		{
			XAttribute xAttribute = item2.Attribute("Id");
			if (xAttribute.IsNullOrEmpty())
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition has no Id!", LogType.Error);
				continue;
			}
			XAttribute xAttribute2 = item2.Attribute("Weight");
			if (xAttribute2.IsNullOrEmpty())
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition has no Weight!", LogType.Error);
				continue;
			}
			if (!int.TryParse(xAttribute2.Value, out var result))
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition has an invalid weight!", LogType.Error);
				continue;
			}
			EnemyUnitTemplateDefinitions.Add(xAttribute.Value, result);
			int tier = EnemyUnitDatabase.EnemyUnitTemplateDefinitions[xAttribute.Value].Tier;
			if (!AllTiers.Contains(tier))
			{
				AllTiers.Add(tier);
			}
		}
		foreach (XElement item3 in xElement2.Elements("EnemyTier"))
		{
			XAttribute xAttribute3 = item3.Attribute("Value");
			if (xAttribute3.IsNullOrEmpty())
			{
				CLoggerManager.Log("EnemyTier has no Value!", LogType.Error);
				continue;
			}
			float item = 1f;
			XAttribute xAttribute4 = item3.Attribute("WeightMultiplier");
			if (!xAttribute4.IsNullOrEmpty())
			{
				if (!float.TryParse(xAttribute4.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					CLoggerManager.Log("EnemyTier has an invalid weight multiplier (setting it to 1)!", LogType.Error);
					return;
				}
				item = result2;
			}
			if (!int.TryParse(xAttribute3.Value, out var result3))
			{
				CLoggerManager.Log("EnemyUnitTemplateDefinition has an invalid tier (setting it to 1)!", LogType.Error);
				return;
			}
			EnemyTierDefinitions.Add(new Tuple<int, float>(result3, item));
			if (!AllTiers.Contains(result3))
			{
				AllTiers.Add(result3);
			}
		}
		XElement xElement4 = xElement2.Element("EliteEnemies");
		if (xElement4 != null)
		{
			XAttribute xAttribute5 = xElement4.Attribute("OverrideSpawnDefinition");
			if (!bool.TryParse(xAttribute5.Value, out var result4))
			{
				CLoggerManager.Log("Could not parse attribute OverrideSpawnDefinition into a bool : '" + xAttribute5.Value + "'.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
			}
			EliteOverrideSpawnDefinition = result4;
			EliteEnemyUnitTemplateDefinitions.Clear();
			foreach (XElement item4 in xElement4.Elements("EliteEnemy"))
			{
				XAttribute xAttribute6 = item4.Attribute("Id");
				XAttribute xAttribute7 = item4.Attribute("Nb");
				if (!int.TryParse(xAttribute7.Value, out var result5))
				{
					CLoggerManager.Log("Could not parse attribute Nb into an int : '" + xAttribute7.Value + "'.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
				}
				if (EliteEnemyUnitTemplateDefinitions.ContainsKey(xAttribute6.Value))
				{
					CLoggerManager.Log("Duplicate of : '" + xAttribute6.Value + "' in EliteEnemies definition. Adding them.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Warning, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
					EliteEnemyUnitTemplateDefinitions[xAttribute6.Value] += result5;
				}
				else
				{
					EliteEnemyUnitTemplateDefinitions.Add(xAttribute6.Value, result5);
				}
			}
		}
		SeerAdditionalPortraitsSettings.Clear();
		XElement xElement5 = xElement.Element("SeerAdditionalPortraitsSettings");
		if (xElement5 == null)
		{
			return;
		}
		foreach (XElement item5 in xElement5.Elements("SeerAdditionalPortraitSettings"))
		{
			SeerAdditionalPortraitsSettings.Add(new SeerAdditionalPortraitSettings(item5));
		}
	}

	public int GetMaximumEnemiesVariations()
	{
		int num = 0;
		for (int num2 = AllTiers.Count - 1; num2 >= 0; num2--)
		{
			num += EnemyUnitDatabase.EnemyUnitTemplatesByTierDefinitions[AllTiers[num2]].Count;
		}
		return num;
	}
}

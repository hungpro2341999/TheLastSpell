using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Apocalypse;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse;

public class ApocalypseConfigurationDefinition : TheLastStand.Framework.Serialization.Definition
{
	private static class Constants
	{
		public const string EnemyStatModifierConfigurationElement = "EnemyStatModifierConfiguration";

		public const string UnlockConditionElement = "UnlockCondition";

		public const string DamnedSoulsPercentagePerLevelElement = "DamnedSoulsPercentagePerLevel";

		public const string GaugeDisplayMaxApocalypseLevelElement = "GaugeDisplayMaxApocalypseLevel";
	}

	public Dictionary<UnitStatDefinition.E_Stat, string> StatWithModifierTypes { get; } = new Dictionary<UnitStatDefinition.E_Stat, string>(UnitStatDefinition.SharedStatComparer);

	public List<ApocalypseUnlockCondition> ApocalypseUnlockConditions { get; } = new List<ApocalypseUnlockCondition>();

	public uint DamnedSoulsPercentagePerLevel { get; private set; }

	public uint GaugeDisplayMaxApocalypseLevel { get; private set; }

	public ApocalypseConfigurationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "EnemyStatModifierConfiguration":
				foreach (XElement item2 in item.Element("StatModifiableValueType").Elements("StatWithModifierType"))
				{
					XAttribute xAttribute = item2.Attribute("Id");
					XAttribute xAttribute2 = item2.Attribute("Type");
					if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result2))
					{
						Debug.LogError("StatWithModifierType " + xAttribute.Value + " " + HasAnInvalidStat(xAttribute.Value));
					}
					StatWithModifierTypes.Add(result2, xAttribute2.Value);
				}
				break;
			case "UnlockCondition":
				foreach (XElement item3 in item.Elements())
				{
					string text = item3.Name.ToString();
					if (text != null && text == "RunCompletedInCity")
					{
						ApocalypseUnlockConditions.Add(new RunCompletedInCityCondition(new RunCompletedInCityConditionDefinition(item3)));
					}
				}
				break;
			case "DamnedSoulsPercentagePerLevel":
			{
				if (!uint.TryParse(item.Value, out var result3))
				{
					CLoggerManager.Log("Could not parse DamnedSoulsPercentagePerLevel value " + item.Value + " to a valid uint!");
				}
				else
				{
					DamnedSoulsPercentagePerLevel = result3;
				}
				break;
			}
			case "GaugeDisplayMaxApocalypseLevel":
			{
				if (!uint.TryParse(item.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
				{
					CLoggerManager.Log("Could not parse GaugeDisplayMaxApocalypseLevel value " + item.Value + " to a valid uint!");
					break;
				}
				GaugeDisplayMaxApocalypseLevel = result;
				int num = 0;
				foreach (ApocalypseTierDefinition value in ApocalypseDatabase.TierDefinitions.Values)
				{
					if (value.ApocalypseLevelCompletedToUnlock > num)
					{
						num = value.ApocalypseLevelCompletedToUnlock;
					}
				}
				if (GaugeDisplayMaxApocalypseLevel < num)
				{
					CLoggerManager.Log(string.Format("{0}'s value ({1}) is less than the highest tier unlock value ({2}), it must be higher !", "GaugeDisplayMaxApocalypseLevel", GaugeDisplayMaxApocalypseLevel, num), LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApocalypseConfigurationDefinition");
				}
				break;
			}
			}
		}
	}
}

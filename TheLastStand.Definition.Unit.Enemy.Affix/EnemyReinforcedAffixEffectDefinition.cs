using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyReinforcedAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Reinforced;

	public Dictionary<UnitStatDefinition.E_Stat, Node> ModifiedStatsEveryXDay { get; private set; }

	public EnemyReinforcedAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		ModifiedStatsEveryXDay = new Dictionary<UnitStatDefinition.E_Stat, Node>();
		foreach (XElement item in obj.Elements("StatModifier"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result))
			{
				CLoggerManager.Log("Id attribute could not be parsed into a stat : \"" + xAttribute.Value + "\".", TPSingleton<EnemyUnitDatabase>.Instance, LogType.Error, CLogLevel.MAJOR);
				continue;
			}
			XAttribute xAttribute2 = item.Attribute("Value");
			ModifiedStatsEveryXDay.Add(result, Parser.Parse(xAttribute2.Value, base.TokenVariables));
		}
	}
}

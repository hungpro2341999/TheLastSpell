using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyAuraAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Aura;

	public bool IncludeSelf { get; private set; }

	public Node Range { get; private set; }

	public Dictionary<UnitStatDefinition.E_Stat, Node> StatModifiers { get; private set; }

	public int TurnsCount { get; private set; }

	public EnemyAuraAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("Range");
		Range = Parser.Parse(xElement2.Value, base.TokenVariables);
		IncludeSelf = xElement.Element("IncludeSelf") != null;
		XElement xElement3 = xElement.Element("StatModifiers");
		XAttribute xAttribute = xElement3.Attribute("TurnsCount");
		TurnsCount = 1;
		if (xAttribute != null)
		{
			if (!int.TryParse(xAttribute.Value, out var result))
			{
				CLoggerManager.Log("Could not parse TurnsCount attribute into an int in elite aura affix : \"" + xAttribute.Value + "\".", TPSingleton<EnemyUnitDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "EnemyUnitDatabase");
			}
			else
			{
				TurnsCount = result;
			}
		}
		StatModifiers = new Dictionary<UnitStatDefinition.E_Stat, Node>();
		foreach (XElement item in xElement3.Elements("StatModifier"))
		{
			XAttribute xAttribute2 = item.Attribute("Id");
			if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute2.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse Id attribute into a stat in elite aura affix : \"" + xAttribute2.Value + "\".", TPSingleton<EnemyUnitDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "EnemyUnitDatabase");
				continue;
			}
			XAttribute xAttribute3 = item.Attribute("Value");
			StatModifiers.Add(result2, Parser.Parse(xAttribute3.Value, base.TokenVariables));
		}
	}
}

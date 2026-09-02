using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class DynamicStatsModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "DynamicStatsModifier";
	}

	public List<UnitStatDefinition.E_Stat> Stats { get; private set; } = new List<UnitStatDefinition.E_Stat>();

	public Node ValueExpression { get; private set; }

	public DynamicStatsModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		foreach (XElement item in xElement.Elements("Stat"))
		{
			if (Enum.TryParse<UnitStatDefinition.E_Stat>(item.Value, out var result))
			{
				Stats.Add(result);
			}
			else
			{
				CLoggerManager.Log("Could not parse Stat element into an E_Stat : " + item.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "StatModifierEffectDefinition");
			}
		}
		XAttribute xAttribute = xElement.Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
	}
}

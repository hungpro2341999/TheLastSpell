using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class StatModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "StatModifier";
	}

	public bool ChildStatFollows { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public Node ValueExpression { get; private set; }

	public StatModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Stat");
		if (Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result))
		{
			Stat = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse Stat attribute into an E_Stat : " + xAttribute.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "StatModifierEffectDefinition");
		}
		XAttribute xAttribute2 = obj.Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute2.Value, base.TokenVariables);
		XAttribute xAttribute3 = obj.Attribute("ChildStatFollows");
		bool result2;
		if (xAttribute3 == null)
		{
			ChildStatFollows = true;
		}
		else if (bool.TryParse(xAttribute3.Value, out result2))
		{
			ChildStatFollows = result2;
		}
		else
		{
			CLoggerManager.Log("Could not parse ChildStatFollows attribute into a bool : " + xAttribute3.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "StatModifierEffectDefinition");
		}
	}
}

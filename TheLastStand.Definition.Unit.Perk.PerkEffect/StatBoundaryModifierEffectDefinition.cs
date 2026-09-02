using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class StatBoundaryModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "StatBoundaryModifier";
	}

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public Node MinModifierValueExpression { get; private set; }

	public Node MaxModifierValueExpression { get; private set; }

	public StatBoundaryModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
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
		XAttribute xAttribute2 = obj.Attribute("MinModifier");
		MinModifierValueExpression = Parser.Parse((xAttribute2 != null) ? xAttribute2.Value : "0", base.TokenVariables);
		XAttribute xAttribute3 = obj.Attribute("MaxModifier");
		MaxModifierValueExpression = Parser.Parse((xAttribute3 != null) ? xAttribute3.Value : "0", base.TokenVariables);
	}
}

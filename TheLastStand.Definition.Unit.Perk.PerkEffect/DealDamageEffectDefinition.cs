using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class DealDamageEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "DealDamage";
	}

	public bool IgnoreArmor { get; private set; }

	public bool IgnoreDefense { get; private set; }

	public PerkTargetingDefinition PerkTargetingDefinition { get; private set; }

	public Node Value { get; private set; }

	public DealDamageEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("IgnoreDefense");
		if (xAttribute != null)
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				IgnoreDefense = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse IgnoreDefense attribute into a bool : " + xAttribute.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "DealDamageEffectDefinition");
			}
		}
		XAttribute xAttribute2 = xElement.Attribute("IgnoreArmor");
		if (xAttribute2 != null)
		{
			if (bool.TryParse(xAttribute2.Value, out var result2))
			{
				IgnoreArmor = result2;
			}
			else
			{
				CLoggerManager.Log("Could not parse IgnoreArmor attribute into a bool : " + xAttribute2.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "DealDamageEffectDefinition");
			}
		}
		PerkTargetingDefinition = new PerkTargetingDefinition(xElement.Element("PerkTargeting"), base.TokenVariables);
		XAttribute xAttribute3 = xElement.Attribute("Value");
		string text = xAttribute3.Value.Replace(base.TokenVariables);
		if (!string.IsNullOrEmpty(text))
		{
			Value = Parser.Parse(text);
		}
		else
		{
			CLoggerManager.Log("Could not parse Value attribute : " + xAttribute3.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "DealDamageEffectDefinition");
		}
	}
}

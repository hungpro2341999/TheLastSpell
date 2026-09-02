using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ApplyStatusEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ApplyStatus";
	}

	public PerkTargetingDefinition PerkTargetingDefinition { get; private set; }

	public Status.E_StatusType StatusType { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; } = UnitStatDefinition.E_Stat.Undefined;

	public Node ChanceExpression { get; private set; }

	public Node ValueExpression { get; private set; }

	public Node TurnsCountExpression { get; private set; }

	public bool HideDisplayEffect { get; private set; }

	public bool RefreshHUD { get; private set; } = true;

	public ApplyStatusEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		PerkTargetingDefinition = new PerkTargetingDefinition(xElement.Element("PerkTargeting"), base.TokenVariables);
		XAttribute xAttribute = xElement.Attribute("Status");
		if (Enum.TryParse<Status.E_StatusType>(xAttribute.Value, out var result))
		{
			StatusType = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse Status attribute into an E_StatusType : " + xAttribute.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApplyStatusEffectDefinition");
		}
		XAttribute xAttribute2 = xElement.Attribute("Stat");
		if ((StatusType & Status.E_StatusType.Buff) != Status.E_StatusType.None || (StatusType & Status.E_StatusType.Debuff) != Status.E_StatusType.None)
		{
			if (Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute2.Value, out var result2))
			{
				Stat = result2;
			}
			else
			{
				CLoggerManager.Log("Could not parse Stat attribute into an E_Stat : " + xAttribute2.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApplyStatusEffectDefinition");
			}
		}
		else if (xAttribute2 != null)
		{
			CLoggerManager.Log($"Specified an E_Stat {xAttribute2.Value} while {StatusType} does not need one.", LogType.Warning, CLogLevel.MAJOR, forcePrintInUnity: true, "ApplyStatusEffectDefinition");
		}
		XAttribute xAttribute3 = xElement.Attribute("Value");
		if (xAttribute3 != null)
		{
			ValueExpression = Parser.Parse(xAttribute3.Value, base.TokenVariables);
		}
		XAttribute xAttribute4 = xElement.Attribute("Chance");
		if (xAttribute4 != null)
		{
			ChanceExpression = Parser.Parse(xAttribute4.Value, base.TokenVariables);
		}
		XAttribute xAttribute5 = xElement.Attribute("TurnsCount");
		TurnsCountExpression = Parser.Parse(xAttribute5.Value, base.TokenVariables);
		XAttribute xAttribute6 = xElement.Attribute("HideDisplayEffect");
		if (xAttribute6 != null)
		{
			if (bool.TryParse(xAttribute6.Value, out var result3))
			{
				HideDisplayEffect = result3;
			}
			else
			{
				CLoggerManager.Log("Could not parse HideDisplayEffect attribute into a bool : " + xAttribute6.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApplyStatusEffectDefinition");
			}
		}
		XAttribute xAttribute7 = xElement.Attribute("RefreshHUD");
		if (xAttribute7 != null)
		{
			if (bool.TryParse(xAttribute7.Value, out var result4))
			{
				RefreshHUD = result4;
			}
			else
			{
				CLoggerManager.Log("Could not parse RefreshHUD attribute into a bool : " + xAttribute7.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApplyStatusEffectDefinition");
			}
		}
	}
}

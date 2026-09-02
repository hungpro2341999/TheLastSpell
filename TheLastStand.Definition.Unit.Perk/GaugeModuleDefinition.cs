using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class GaugeModuleDefinition : BufferModuleDefinition
{
	public new static class Constants
	{
		public const string Id = "GaugeModule";
	}

	public UnitStatDefinition.E_Stat GaugeStat { get; private set; }

	[NotNull]
	public Node GaugeValue { get; private set; }

	public GaugeModuleDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("GaugeValue");
		GaugeValue = Parser.Parse(xAttribute.Value, base.TokenVariables);
		XAttribute xAttribute2 = xElement.Attribute("GaugeStat");
		if (Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute2.Value, out var result))
		{
			GaugeStat = result;
		}
		else
		{
			CLoggerManager.Log($"Unable to parse {xAttribute2.Value} into a valid E_Stat, line {((IXmlLineInfo)xElement).LineNumber}", LogType.Error, CLogLevel.MAJOR);
		}
	}
}

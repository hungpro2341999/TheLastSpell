using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public abstract class ABufferPerkActionDefinition : APerkActionDefinition
{
	public BufferModuleDefinition.BufferIndex BufferIndex { get; private set; }

	public abstract string Id { get; }

	public Node ValueExpression { get; private set; }

	public ABufferPerkActionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
		XAttribute xAttribute2 = obj.Attribute("BufferIndex");
		if (!string.IsNullOrEmpty(xAttribute2?.Value))
		{
			if (Enum.TryParse<BufferModuleDefinition.BufferIndex>(xAttribute2.Value, out var result))
			{
				BufferIndex = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute2.Value + " into enum BufferIndex.", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}
}

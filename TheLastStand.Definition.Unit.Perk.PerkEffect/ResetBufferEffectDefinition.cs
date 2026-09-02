using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ResetBufferEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ResetBuffer";
	}

	public int? ResetValue;

	public ResetBufferEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("ResetValue");
		if (xAttribute != null)
		{
			if (int.TryParse(xAttribute.Value, out var result))
			{
				ResetValue = result;
			}
			else
			{
				CLoggerManager.Log("Found a ResetValue for ResetBuffer but the int parsing failed. Safely assigning null.", LogType.Warning, CLogLevel.MAJOR, forcePrintInUnity: true, "ResetBufferEffectDefinition");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class StatLockerEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "StatLocker";
	}

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public StatLockerEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Stat");
		if (Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result))
		{
			Stat = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse Stat attribute into an E_Stat : " + xAttribute.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "StatLockerEffectDefinition");
		}
	}
}

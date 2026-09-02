using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public class InstantiateBuffEffectDisplayDefinition : InstantiateEffectDisplayDefinition
{
	public static class Constants
	{
		public const string Id = "InstantiateBuffEffectDisplay";
	}

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public InstantiateBuffEffectDisplayDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (Enum.TryParse<UnitStatDefinition.E_Stat>((container as XElement).Attribute("Stat").Value, out var result))
		{
			Stat = result;
		}
		else
		{
			CLoggerManager.Log("");
		}
	}
}

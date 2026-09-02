using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public class RefillGaugeDefinition : APerkActionDefinition
{
	public static class Constants
	{
		public const string Id = "RefillGauge";
	}

	public RefillGaugeDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}

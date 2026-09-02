using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public class IncreaseBufferDefinition : ABufferPerkActionDefinition
{
	public static class Constants
	{
		public const string Id = "IncreaseBuffer";
	}

	public override string Id => "IncreaseBuffer";

	public IncreaseBufferDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}
}

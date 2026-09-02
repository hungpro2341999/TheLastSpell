using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public class DecreaseBufferDefinition : ABufferPerkActionDefinition
{
	public static class Constants
	{
		public const string Id = "DecreaseBuffer";
	}

	public override string Id => "DecreaseBuffer";

	public DecreaseBufferDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}
}

using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public class SetBufferDefinition : ABufferPerkActionDefinition
{
	public static class Constants
	{
		public const string Id = "SetBufferTo";
	}

	public override string Id => "SetBufferTo";

	public SetBufferDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}
}

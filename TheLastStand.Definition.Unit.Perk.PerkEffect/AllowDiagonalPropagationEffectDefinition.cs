using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class AllowDiagonalPropagationEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "AllowDiagonalPropagation";
	}

	public AllowDiagonalPropagationEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
	}
}

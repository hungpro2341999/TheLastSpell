using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class PermanentBaseStatModifierEffectDefinition : StatModifierEffectDefinition
{
	public new static class Constants
	{
		public const string Id = "PermanentBaseStatModifier";
	}

	public PermanentBaseStatModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}
}

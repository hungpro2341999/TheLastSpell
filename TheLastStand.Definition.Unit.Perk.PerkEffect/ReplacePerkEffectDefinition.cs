using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ReplacePerkEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ReplacePerk";
	}

	public string PerkToReplaceId { get; private set; }

	public string PerkReplacementId { get; private set; }

	public ReplacePerkEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("PerkToReplaceId");
		PerkToReplaceId = xAttribute.Value;
		XAttribute xAttribute2 = obj.Attribute("PerkReplacementId");
		PerkReplacementId = xAttribute2.Value;
	}
}

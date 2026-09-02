using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class GetAdditionalExperienceEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "GetAdditionalExperience";
	}

	public Node ValueExpression { get; private set; }

	public GetAdditionalExperienceEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
	}
}

using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class IncreaseStartingFogDensityApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public int Value { get; private set; }

	public IncreaseStartingFogDensityApocalypseEffectDefinition(XContainer xContainer, Dictionary<string, string> tokenVariables = null)
		: base(xContainer, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container != null)
		{
			base.Deserialize(container);
			XAttribute xAttribute = (container as XElement).Attribute("Value");
			Value = Parser.Parse(xAttribute.Value, base.TokenVariables).EvalToInt();
		}
	}
}

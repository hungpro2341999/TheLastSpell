using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class MultiplyPanicGainApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public float Multiplier { get; private set; }

	public MultiplyPanicGainApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container != null)
		{
			base.Deserialize(container);
			XAttribute xAttribute = (container as XElement).Attribute("Multiplier");
			Multiplier = Parser.Parse(xAttribute.Value, base.TokenVariables).EvalToFloat();
		}
	}
}

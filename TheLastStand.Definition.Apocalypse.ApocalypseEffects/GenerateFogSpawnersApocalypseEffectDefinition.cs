using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class GenerateFogSpawnersApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public float Multiplier { get; private set; }

	public GenerateFogSpawnersApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Multiplier");
		if (xAttribute != null)
		{
			Multiplier = Parser.Parse(xAttribute.Value, base.TokenVariables).EvalToFloat();
			if (Multiplier < 0f)
			{
				Multiplier = 1f;
			}
		}
		else
		{
			Multiplier = 1f;
		}
	}
}

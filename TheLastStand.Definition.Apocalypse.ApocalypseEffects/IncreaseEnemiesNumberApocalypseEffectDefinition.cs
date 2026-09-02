using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class IncreaseEnemiesNumberApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public int Value { get; private set; }

	public IncreaseEnemiesNumberApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
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

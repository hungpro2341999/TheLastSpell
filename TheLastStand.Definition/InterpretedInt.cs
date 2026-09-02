using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition;

public class InterpretedInt : InterpretedValue<int>
{
	public InterpretedInt(XElement xElement, int defaultValue, Dictionary<string, string> tokenVariables = null)
		: base((XContainer)xElement, defaultValue, tokenVariables)
	{
	}

	protected override bool TryDeserializeTValue(XElement xElement, out int newTValue)
	{
		if (int.TryParse(xElement.Value, out newTValue))
		{
			return true;
		}
		newTValue = 0;
		return false;
	}

	protected override bool TryEvalExpression(out int expressionValue)
	{
		if (tExpression == null)
		{
			expressionValue = 0;
			return false;
		}
		expressionValue = tExpression.EvalToInt();
		return true;
	}
}

using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace TheLastStand.Definition;

public class InterpretedFloat : InterpretedValue<float>
{
	public InterpretedFloat(XContainer container, float defaultValue = 0f, Dictionary<string, string> tokenVariables = null)
		: base(container, defaultValue, tokenVariables)
	{
	}

	protected override bool TryDeserializeTValue(XElement xElement, out float newTValue)
	{
		if (float.TryParse(xElement.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out newTValue))
		{
			return true;
		}
		newTValue = 0f;
		return false;
	}

	protected override bool TryEvalExpression(out float expressionValue)
	{
		if (tExpression == null)
		{
			expressionValue = 0f;
			return false;
		}
		expressionValue = tExpression.EvalToInt();
		return true;
	}
}

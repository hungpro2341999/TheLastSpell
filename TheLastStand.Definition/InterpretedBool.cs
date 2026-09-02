using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition;

public class InterpretedBool : InterpretedValue<bool>
{
	public InterpretedBool(XContainer container, bool defaultValue = false, Dictionary<string, string> tokenVariables = null)
		: base(container, defaultValue, tokenVariables)
	{
	}

	protected override bool TryDeserializeTValue(XElement xElement, out bool newTValue)
	{
		throw new NotImplementedException();
	}

	protected override bool TryEvalExpression(out bool expressionValue)
	{
		throw new NotImplementedException();
	}
}

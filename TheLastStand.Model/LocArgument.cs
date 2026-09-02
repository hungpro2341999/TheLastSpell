using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Model;

public class LocArgument
{
	public static class Constants
	{
		public const string Id = "LocArgument";
	}

	public string Value { get; }

	public Node ValueExpression { get; }

	public string Prefix { get; }

	public string Suffix { get; }

	public string Style { get; }

	public LocArgument(string value, Node valueExpression, string style, string prefix, string suffix)
	{
		Value = value;
		ValueExpression = valueExpression;
		Prefix = prefix;
		Suffix = suffix;
		Style = style;
	}

	protected object GetObjectValue(InterpreterContext interpreterContext)
	{
		if (ValueExpression == null)
		{
			return Value;
		}
		return ValueExpression.Eval(interpreterContext);
	}

	public virtual string GetFinalValue(InterpreterContext interpreterContext)
	{
		if (!string.IsNullOrEmpty(Style))
		{
			return $"<style={Style}>{Prefix}{GetObjectValue(interpreterContext)}{Suffix}</style>";
		}
		return $"{Prefix}{GetObjectValue(interpreterContext)}{Suffix}";
	}
}

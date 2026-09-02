using TPLib.Localization;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Model;

public class LocalizedArgument : LocArgument
{
	public new static class Constants
	{
		public const string Id = "LocalizedArgument";
	}

	public LocalizedArgument(string value, Node valueExpression, string style, string prefix, string suffix)
		: base(value, valueExpression, style, prefix, suffix)
	{
	}

	public override string GetFinalValue(InterpreterContext interpreterContext)
	{
		string key = $"{base.Prefix}{GetObjectValue(interpreterContext)}{base.Suffix}";
		if (!string.IsNullOrEmpty(base.Style))
		{
			return "<style=" + base.Style + ">" + Localizer.Get(key) + "</style>";
		}
		return Localizer.Get(key);
	}
}

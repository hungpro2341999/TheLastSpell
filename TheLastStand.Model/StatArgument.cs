using System;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Model;

public class StatArgument : LocArgument
{
	public new static class Constants
	{
		public const string Id = "StatArgument";
	}

	public bool DisplaySign { get; }

	public UnitStatDefinition.E_Stat Stat { get; }

	public Node StatExpression { get; }

	public StatArgument(string value, Node valueExpression, string style, string prefix, string suffix, UnitStatDefinition.E_Stat stat, Node statExpression, bool displaySign)
		: base(value, valueExpression, style, prefix, suffix)
	{
		Stat = stat;
		DisplaySign = displaySign;
		StatExpression = statExpression;
	}

	public override string GetFinalValue(InterpreterContext interpreterContext)
	{
		UnitStatDefinition.E_Stat stat = UnitStatDefinition.E_Stat.Undefined;
		if (StatExpression != null && Enum.TryParse<UnitStatDefinition.E_Stat>(StatExpression.Eval(interpreterContext).ToString(), out var result))
		{
			stat = result;
		}
		else if (Stat != UnitStatDefinition.E_Stat.Undefined)
		{
			stat = Stat;
		}
		string text = string.Empty;
		if (base.Value != null || base.ValueExpression != null)
		{
			float floatValue = GetFloatValue(interpreterContext);
			text += (string.IsNullOrEmpty(base.Style) ? stat.GetValueStylized(floatValue, outlined: true, DisplaySign) : $"<style={base.Style}>{base.Prefix}{floatValue}{base.Suffix}</style>");
			text += " ";
		}
		string stylizedName = stat.GetStylizedName();
		return text + stylizedName;
	}

	protected virtual float GetFloatValue(InterpreterContext interpreterContext)
	{
		float.TryParse(GetObjectValue(interpreterContext).ToString(), out var result);
		return result;
	}
}

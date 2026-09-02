using TPLib.Localization;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Unit.Perk;
using UnityEngine;

namespace TheLastStand.Model;

public class RestoreStatArgument : StatArgument
{
	public new static class Constants
	{
		public const string Id = "RestoreStatArgument";

		public const string RestoreKey = "Perks_RestoreKeyword";
	}

	public bool ModifiedValue { get; }

	public bool DisplayRestoreText { get; }

	public RestoreStatArgument(string value, Node valueExpression, string style, string prefix, string suffix, UnitStatDefinition.E_Stat stat, bool displaySign, bool modifiedValue, bool displayRestoreText)
		: base(value, valueExpression, style, prefix, suffix, stat, null, displaySign)
	{
		ModifiedValue = modifiedValue;
		DisplayRestoreText = displayRestoreText;
	}

	public override string GetFinalValue(InterpreterContext interpreterContext)
	{
		return "<style=RegenStat>" + (DisplayRestoreText ? (Localizer.Get("Perks_RestoreKeyword") + " ") : string.Empty) + base.GetFinalValue(interpreterContext) + "</style>";
	}

	protected override float GetFloatValue(InterpreterContext interpreterContext)
	{
		float num = base.GetFloatValue(interpreterContext);
		if (ModifiedValue && interpreterContext is Perk perk && (base.Stat == UnitStatDefinition.E_Stat.Health || base.Stat == UnitStatDefinition.E_Stat.HealthTotal))
		{
			num *= perk.Owner.GetClampedStatValue(UnitStatDefinition.E_Stat.HealingReceived) * 0.01f;
			num = Mathf.Floor(num);
		}
		return num;
	}
}

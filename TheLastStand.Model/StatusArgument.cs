using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit.Perk;
using UnityEngine;

namespace TheLastStand.Model;

public class StatusArgument : LocArgument
{
	public new static class Constants
	{
		public const string Id = "StatusArgument";
	}

	public TheLastStand.Model.Status.Status.E_StatusType Status { get; }

	public Node TurnsCountExpression { get; }

	public Node ChanceExpression { get; }

	public bool ModifiedValue { get; }

	public UnitStatDefinition.E_Stat Stat { get; }

	public StatusArgument(string value, Node valueExpression, string style, string prefix, string suffix, TheLastStand.Model.Status.Status.E_StatusType status, Node turnsCountExpression, Node chanceExpression, bool modifiedValue, UnitStatDefinition.E_Stat stat)
		: base(value, valueExpression, style, prefix, suffix)
	{
		Status = status;
		TurnsCountExpression = turnsCountExpression;
		ChanceExpression = chanceExpression;
		ModifiedValue = modifiedValue;
		Stat = stat;
	}

	public override string GetFinalValue(InterpreterContext interpreterContext)
	{
		float? num = null;
		int? turnsCount = null;
		int? chance = null;
		if ((base.Value != null || base.ValueExpression != null) && float.TryParse(GetObjectValue(interpreterContext).ToString(), out var result))
		{
			num = result;
		}
		if (TurnsCountExpression != null && int.TryParse(TurnsCountExpression.Eval(interpreterContext).ToString(), out var result2))
		{
			turnsCount = result2;
		}
		if (ChanceExpression != null && int.TryParse(ChanceExpression.Eval(interpreterContext).ToString(), out var result3))
		{
			chance = result3;
		}
		if (ModifiedValue && interpreterContext is Perk perk)
		{
			if (turnsCount.HasValue)
			{
				turnsCount = perk.Owner.ComputeStatusDuration(Status, turnsCount.Value);
			}
			switch (Status)
			{
			case TheLastStand.Model.Status.Status.E_StatusType.Stun:
				if (chance.HasValue)
				{
					chance = Mathf.RoundToInt(perk.Owner.UnitController.GetModifiedStunChance((float)chance.Value / 100f) * 100f);
				}
				break;
			case TheLastStand.Model.Status.Status.E_StatusType.Poison:
				if (num.HasValue)
				{
					num = perk.Owner.UnitController.GetModifiedPoisonDamage(num.Value);
				}
				break;
			}
		}
		string valueStylizedOverride = ((!string.IsNullOrEmpty(base.Style)) ? $"<style={base.Style}>{base.Prefix}{num}{base.Suffix}</style>" : null);
		return Status.GetFormattedStyle(num, turnsCount, chance, Stat, valueStylizedOverride);
	}
}

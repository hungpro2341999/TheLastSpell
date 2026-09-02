using TPLib.Log;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class LogPerkActionController : APerkActionController
{
	public LogPerkAction LogPerkAction => PerkAction as LogPerkAction;

	public LogPerkActionController(LogPerkActionDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new LogPerkAction(definition as LogPerkActionDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		object obj = LogPerkAction.LogPerkActionDefinition.ValueExpression.Eval(PerkAction.PerkEvent.PerkModule.Perk);
		string formattingString = LogPerkAction.LogPerkActionDefinition.FormattingString;
		string message;
		if (!string.IsNullOrEmpty(formattingString))
		{
			message = string.Format(formattingString, obj);
		}
		else
		{
			string expressionString = LogPerkAction.LogPerkActionDefinition.ExpressionString;
			message = $"{expressionString} = {obj}";
		}
		CLoggerManager.Log(message, LogType.Log, CLogLevel.MODDING, forcePrintInUnity: true, "Perk");
	}
}

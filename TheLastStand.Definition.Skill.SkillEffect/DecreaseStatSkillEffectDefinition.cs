using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class DecreaseStatSkillEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "DecreaseStat";
	}

	private Node lossValueExpression;

	public override string Id => "DecreaseStat_" + Stat;

	public float LossValue => GetLossValue(null);

	public string StatDescription => UnitDatabase.UnitStatDefinitions[Stat].Description;

	public string StatName => UnitDatabase.UnitStatDefinitions[Stat].Name;

	public UnitStatDefinition.E_Stat Stat { get; private set; } = UnitStatDefinition.E_Stat.Undefined;

	public DecreaseStatSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public float GetLossValue(InterpreterContext context)
	{
		return lossValueExpression.EvalToFloat(context);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		lossValueExpression = Parser.Parse(xElement?.Element("LossValue")?.Value ?? "0", base.TokenVariables);
		if (Enum.TryParse<UnitStatDefinition.E_Stat>(xElement.Element("Stat").Attribute("Id").Value, out var result))
		{
			Stat = result;
		}
		else
		{
			CLoggerManager.Log($"Unknown stat {result}", LogType.Error);
		}
	}
}

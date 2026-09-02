using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class RegenStatSkillEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "RegenStat";
	}

	private Node bonusValueExpression;

	public float Bonus => GetBonus(null);

	public override string Id => "RegenStat";

	public string StatDescription => UnitDatabase.UnitStatDefinitions[Stat].Description;

	public string StatName => UnitDatabase.UnitStatDefinitions[Stat].Name;

	public UnitStatDefinition.E_Stat Stat { get; private set; } = UnitStatDefinition.E_Stat.Undefined;

	public RegenStatSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public float GetBonus(InterpreterContext context)
	{
		return bonusValueExpression.EvalToFloat(context);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		bonusValueExpression = Parser.Parse(xElement?.Element("Bonus")?.Value ?? "0", base.TokenVariables);
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

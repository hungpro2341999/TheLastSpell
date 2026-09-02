using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition;

public abstract class LocalizableDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<LocArgument> LocArguments { get; private set; }

	protected LocalizableDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public object[] GetArguments(InterpreterContext interpreterContext)
	{
		object[] array = new object[LocArguments.Count];
		for (int i = 0; i < LocArguments.Count; i++)
		{
			array[i] = LocArguments[i].GetFinalValue(interpreterContext);
		}
		return array;
	}

	private void DeserializeLocArguments(XElement xLocArguments)
	{
		LocArguments = new List<LocArgument>();
		foreach (XElement item2 in xLocArguments.Elements())
		{
			XAttribute xAttribute = item2.Attribute("Value");
			XAttribute xAttribute2 = item2.Attribute("Interpreted");
			XAttribute xAttribute3 = item2.Attribute("Prefix");
			XAttribute xAttribute4 = item2.Attribute("Suffix");
			XAttribute xAttribute5 = item2.Attribute("Style");
			if (item2.Name.LocalName != "StatArgument" && item2.Name.LocalName != "StatusArgument")
			{
				_ = item2.Name.LocalName != "AttackTypeArgument";
			}
			string text = xAttribute?.Value.Replace(base.TokenVariables);
			Node valueExpression = null;
			if (!string.IsNullOrEmpty(xAttribute2?.Value) && bool.Parse(xAttribute2.Value) && !string.IsNullOrEmpty(text))
			{
				valueExpression = Parser.Parse(text);
			}
			LocArgument item = null;
			switch (item2.Name.LocalName)
			{
			case "LocArgument":
				item = new LocArgument(text, valueExpression, xAttribute5?.Value, xAttribute3?.Value, xAttribute4?.Value);
				break;
			case "AttackTypeArgument":
			{
				XAttribute xAttribute14 = item2.Attribute("AttackType");
				XAttribute xAttribute15 = item2.Attribute("InterpretedAttackType");
				Node attackTypeExpression = ((!string.IsNullOrEmpty(xAttribute15?.Value)) ? Parser.Parse(xAttribute15.Value) : null);
				item = new AttackTypeArgument(text, valueExpression, xAttribute5?.Value, xAttribute3?.Value, xAttribute4?.Value, attackTypeExpression, xAttribute14?.Value);
				break;
			}
			case "LocalizedArgument":
				item = new LocalizedArgument(text, valueExpression, xAttribute5?.Value, xAttribute3?.Value, xAttribute4?.Value);
				break;
			case "StatArgument":
			{
				XAttribute xAttribute11 = item2.Attribute("Stat");
				XAttribute xAttribute12 = item2.Attribute("DisplaySign");
				XAttribute xAttribute13 = item2.Attribute("InterpretedStat");
				Node statExpression = ((!string.IsNullOrEmpty(xAttribute13?.Value)) ? Parser.Parse(xAttribute13.Value) : null);
				bool displaySign = string.IsNullOrEmpty(xAttribute12?.Value) || bool.Parse(xAttribute12.Value);
				UnitStatDefinition.E_Stat result3 = UnitStatDefinition.E_Stat.Undefined;
				if (!string.IsNullOrEmpty(xAttribute11?.Value))
				{
					Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute11.Value, out result3);
				}
				item = new StatArgument(text, valueExpression, xAttribute5?.Value, xAttribute3?.Value, xAttribute4?.Value, result3, statExpression, displaySign);
				break;
			}
			case "RestoreStatArgument":
			{
				XAttribute xAttribute16 = item2.Attribute("Stat");
				XAttribute xAttribute17 = item2.Attribute("DisplaySign");
				bool displaySign2 = string.IsNullOrEmpty(xAttribute17?.Value) || bool.Parse(xAttribute17.Value);
				Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute16.Value, out var result4);
				XAttribute xAttribute18 = item2.Attribute("DisplayRestoreText");
				bool displayRestoreText = !string.IsNullOrEmpty(xAttribute18?.Value) && bool.Parse(xAttribute18.Value);
				XAttribute xAttribute19 = item2.Attribute("ModifiedValue");
				bool modifiedValue2 = !string.IsNullOrEmpty(xAttribute19?.Value) && bool.Parse(xAttribute19.Value);
				item = new RestoreStatArgument(text, valueExpression, xAttribute5?.Value, xAttribute3?.Value, xAttribute4?.Value, result4, displaySign2, modifiedValue2, displayRestoreText);
				break;
			}
			case "StatusArgument":
			{
				XAttribute xAttribute6 = item2.Attribute("Status");
				XAttribute xAttribute7 = item2.Attribute("TurnsCount");
				XAttribute xAttribute8 = item2.Attribute("Chance");
				XAttribute xAttribute9 = item2.Attribute("ModifiedValue");
				XAttribute xAttribute10 = item2.Attribute("Stat");
				Enum.TryParse<Status.E_StatusType>(xAttribute6.Value.Replace(base.TokenVariables), out var result);
				UnitStatDefinition.E_Stat result2 = UnitStatDefinition.E_Stat.Undefined;
				if (!string.IsNullOrEmpty(xAttribute10?.Value))
				{
					Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute10.Value.Replace(base.TokenVariables), out result2);
				}
				Node turnsCountExpression = null;
				string text2 = xAttribute7?.Value.Replace(base.TokenVariables);
				if (!string.IsNullOrEmpty(text2))
				{
					turnsCountExpression = Parser.Parse(text2);
				}
				Node chanceExpression = null;
				string text3 = xAttribute8?.Value.Replace(base.TokenVariables);
				if (!string.IsNullOrEmpty(text3))
				{
					chanceExpression = Parser.Parse(text3);
				}
				bool modifiedValue = !string.IsNullOrEmpty(xAttribute9?.Value) && bool.Parse(xAttribute9.Value);
				item = new StatusArgument(text, valueExpression, xAttribute5?.Value, xAttribute3?.Value, xAttribute4?.Value, result, turnsCountExpression, chanceExpression, modifiedValue, result2);
				break;
			}
			}
			LocArguments.Add(item);
		}
	}

	public override void Deserialize(XContainer container)
	{
		LocArguments = new List<LocArgument>();
		if (container is XElement xLocArguments)
		{
			DeserializeLocArguments(xLocArguments);
		}
	}
}

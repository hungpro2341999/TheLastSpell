using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkDataCondition;

public class PerkDataConditionsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<APerkDataConditionDefinition> ConditionDefinitions { get; private set; }

	public PerkDataConditionsDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		ConditionDefinitions = new List<APerkDataConditionDefinition>();
		if (!(container is XElement xElement))
		{
			return;
		}
		foreach (XElement item in xElement.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "IsTrue":
				ConditionDefinitions.Add(new IsTrueDataConditionDefinition(item, base.TokenVariables));
				break;
			case "IsFalse":
				ConditionDefinitions.Add(new IsFalseDataConditionDefinition(item, base.TokenVariables));
				break;
			default:
				CLoggerManager.Log("Tried to Deserialize an unimplemented PerkDataCondition: " + item.Name.LocalName, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkDataConditionsDefinition");
				break;
			}
		}
	}
}

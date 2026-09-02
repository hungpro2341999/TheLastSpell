using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class TargetingMethodsContainerDefinition : TheLastStand.Framework.Serialization.Definition
{
	public bool AvoidOverkill { get; private set; }

	public List<TargetingMethodDefinition> TargetingMethods { get; private set; } = new List<TargetingMethodDefinition>();

	public TargetingMethodsContainerDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "Closest":
				TargetingMethods.Add(new ClosestTargetingMethodDefinition(item));
				break;
			case "Farthest":
				TargetingMethods.Add(new FarthestTargetingMethodDefinition(item));
				break;
			case "FirstTarget":
				TargetingMethods.Add(new FirstTargetTargetingMethodDefinition(item));
				break;
			case "Optimal":
				TargetingMethods.Add(new OptimalTargetingMethodDefinition(item));
				break;
			case "Score":
				TargetingMethods.Add(new ScoreTargetingMethodDefinition(item));
				break;
			case "Random":
				TargetingMethods.Add(new RandomTargetingMethodDefinition(item));
				break;
			case "AvoidOverkill":
				AvoidOverkill = true;
				break;
			default:
				CLoggerManager.Log("Error, " + item.Name.LocalName + " is not a correct Targeting Method!", LogType.Error);
				break;
			}
		}
		if (TargetingMethods.Count == 0)
		{
			CLoggerManager.Log("No Targeting Method found!", LogType.Error);
		}
	}
}

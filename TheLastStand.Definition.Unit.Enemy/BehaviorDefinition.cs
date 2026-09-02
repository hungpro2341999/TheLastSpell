using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class BehaviorDefinition : TheLastStand.Framework.Serialization.Definition
{
	public GoalDefinition[] GoalDefinitions { get; private set; }

	public int GoalsComputingOrder { get; private set; }

	public PathfindingDefinition.E_PathfindingStyle PathfindingStyle { get; private set; }

	public int TurnsToSkipOnSpawn { get; private set; }

	public int ThinkingScope { get; private set; }

	public int NumberOfGoalsToExecute { get; private set; }

	public BehaviorDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("PathfindingStyle");
		PathfindingDefinition.E_PathfindingStyle result = PathfindingDefinition.E_PathfindingStyle.Hypotenuse;
		if (xElement2 != null && !Enum.TryParse<PathfindingDefinition.E_PathfindingStyle>(xElement2.Value, out result))
		{
			Debug.LogError("Invalid PathfindingStyle");
		}
		PathfindingStyle = result;
		ThinkingScope = int.Parse(xElement.Element("ThinkingScope").Value);
		XElement xElement3 = xElement.Element("GoalsComputingOrder");
		if (xElement3 != null)
		{
			XAttribute xAttribute = xElement3.Attribute("Value");
			GoalsComputingOrder = int.Parse(xAttribute.Value);
		}
		XElement xElement4 = xElement.Element("Goals");
		int num = 0;
		foreach (XElement item in xElement4.Elements("Goal"))
		{
			_ = item;
			num++;
		}
		GoalDefinitions = new GoalDefinition[num];
		int num2 = 0;
		foreach (XElement item2 in xElement4.Elements("Goal"))
		{
			GoalDefinition goalDefinition = new GoalDefinition(item2);
			GoalDefinitions[num2++] = goalDefinition;
		}
		XElement xElement5 = xElement.Element("NumberOfGoalsToExecute");
		if (xElement5 != null)
		{
			if (!int.TryParse(xElement5.Value, out var result2))
			{
				Debug.LogError("NumberOfGoalsToExecute should have a value of type int !");
			}
			NumberOfGoalsToExecute = result2;
		}
		XElement xElement6 = xElement.Element("TurnsToSkipOnSpawn");
		if (xElement6 != null)
		{
			if (!int.TryParse(xElement6.Attribute("Value").Value, out var result3))
			{
				CLoggerManager.Log("Could not parse Value attribute in TurnsToSkipOnSpawn element in BehaviorDefinition into an int.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "BehaviorDefinition");
			}
			TurnsToSkipOnSpawn = result3;
		}
	}
}

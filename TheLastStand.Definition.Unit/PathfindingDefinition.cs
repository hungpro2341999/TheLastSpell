using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class PathfindingDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_PathfindingStyle
	{
		Undefined,
		Manhattan,
		Hypotenuse,
		Bresenham
	}

	public float EnemyAISpreadFactor { get; private set; }

	public float NodeWeightFogMultiplier { get; private set; }

	public PathfindingDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		if (!float.TryParse(obj.Element("EnemyAISpreadFactor").Attribute("Value").Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Debug.LogError("Invalid EnemyAISpreadFactor Value");
		}
		EnemyAISpreadFactor = result;
		if (!float.TryParse(obj.Element("NodeWeightFogMultiplier").Attribute("Value").Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			Debug.LogError("Invalid NodeWeightFogMultiplier Value");
		}
		NodeWeightFogMultiplier = result2;
	}
}

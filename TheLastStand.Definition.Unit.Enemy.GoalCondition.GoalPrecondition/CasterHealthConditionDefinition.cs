using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class CasterHealthConditionDefinition : GoalConditionDefinition
{
	public const string Name = "CasterHealth";

	public float Max { get; private set; }

	public float Min { get; private set; }

	public CasterHealthConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Min");
		if (xAttribute != null)
		{
			if (!float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				Debug.LogError("Invalid Min (" + xAttribute.Value + ")");
			}
			Min = result;
		}
		XAttribute xAttribute2 = obj.Attribute("Max");
		if (xAttribute2 != null)
		{
			if (!float.TryParse(xAttribute2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				Debug.LogError("Invalid Max (" + xAttribute2.Value + ")");
			}
			Max = result2;
		}
	}
}

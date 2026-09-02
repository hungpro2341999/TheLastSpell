using System;
using System.Globalization;
using System.Xml.Linq;
using DG.Tweening;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class BossStagingDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Node TotalDuration { get; private set; }

	public Ease MovementEasing { get; private set; }

	public float MovementDuration { get; private set; }

	public float PauseDuration { get; private set; }

	public BossStagingDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("Duration");
		TotalDuration = Parser.Parse(xElement.Value);
		if (float.TryParse(obj.Element("PauseDuration").Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			PauseDuration = result;
		}
		if (Enum.TryParse<Ease>(obj.Element("MovementEasing").Value, out var result2))
		{
			MovementEasing = result2;
		}
		if (float.TryParse(obj.Element("MovementDuration").Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
		{
			MovementDuration = result3;
		}
	}
}

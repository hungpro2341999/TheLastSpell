using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class RecruitmentDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Node MageCost { get; set; }

	public float MageGenerationProbabilityIncreasedPerReRoll { get; set; }

	public float MageGenerationStartProbability { get; set; }

	public int RosterRerollCost { get; set; }

	public Node UnitCost { get; set; }

	public List<UnitGenerationDefinition> UnitsToGenerate { get; private set; } = new List<UnitGenerationDefinition>();

	public RecruitmentDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container.Element("UnitGenerationSettings");
		XElement xElement2 = xElement.Element("Cost");
		if (xElement2.IsNullOrEmpty())
		{
			Debug.LogError("The UnitGenerationSettings must have a Cost");
			return;
		}
		UnitCost = Parser.Parse(xElement2.Value);
		xElement.Element("UnitLimits");
		foreach (XElement item in xElement.Elements("Slot"))
		{
			if (item.Attribute("Id").IsNullOrEmpty())
			{
				Debug.LogError("The Slot must have Id");
			}
			else
			{
				UnitsToGenerate.Add(new UnitGenerationDefinition(item));
			}
		}
		XElement xElement3 = container.Element("MageGenerationSettings");
		if (xElement3.IsNullOrEmpty())
		{
			Debug.LogError("The document must have MageGenerationSettings");
			return;
		}
		XElement xElement4 = xElement3.Element("Cost");
		if (!xElement4.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement4.Value, out var _))
			{
				Debug.LogError("MageGenerationSettings must have a valid cost!");
				return;
			}
			MageCost = Parser.Parse(xElement4.Value);
		}
		XElement xElement5 = xElement3.Element("StartProbability");
		if (!xElement5.IsNullOrEmpty())
		{
			if (!float.TryParse(xElement5.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				Debug.LogError("MageGenerationSettings must have a valid start probability!");
				return;
			}
			MageGenerationStartProbability = result2;
		}
		XElement xElement6 = xElement3.Element("ProbabilityIncreasedPerReRoll");
		if (!xElement6.IsNullOrEmpty())
		{
			if (!float.TryParse(xElement6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
			{
				Debug.LogError("MageGenerationSettings must have a valid ProbabilityIncreasedPerReRoll!");
				return;
			}
			MageGenerationProbabilityIncreasedPerReRoll = result3;
		}
		XElement xElement7 = container.Element("RosterRerollCost");
		if (!xElement7.IsNullOrEmpty())
		{
			if (!int.TryParse(xElement7.Value, out var result4))
			{
				Debug.LogError("RecruitmentDefinition must have a valid RosterRerollCost!");
			}
			else
			{
				RosterRerollCost = result4;
			}
		}
	}
}

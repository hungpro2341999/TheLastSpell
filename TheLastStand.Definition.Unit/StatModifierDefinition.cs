using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit;

public class StatModifierDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string Id = "StatModifier";
	}

	public float FlatModifier { get; set; }

	public float PercentageModifier { get; set; }

	public StatModifierDefinition(float flatModifier, float percentageModifier)
		: base(null)
	{
		FlatModifier = flatModifier;
		PercentageModifier = percentageModifier;
	}

	public StatModifierDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (container != null)
		{
			string text = xElement.Element("PercentageModifier")?.Value;
			string text2 = xElement.Element("FlatModifier")?.Value;
			PercentageModifier = ((text != null) ? ((float)int.Parse(text)) : 100f);
			FlatModifier = ((text2 != null) ? ((float)int.Parse(text2)) : 0f);
		}
	}
}

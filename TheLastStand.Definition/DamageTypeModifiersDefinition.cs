using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition;

public class DamageTypeModifiersDefinition : TheLastStand.Framework.Serialization.Definition
{
	public float MeleeArmorShreddingBonus { get; private set; }

	public Dictionary<int, float> DodgeMultiplierByDistance { get; private set; }

	public DamageTypeModifiersDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container.Element("DamageTypeModifiersDefinition");
		XElement xElement2 = xElement.Element("Melee");
		if (xElement2 != null)
		{
			XElement xElement3 = xElement2.Element("ArmorShredding");
			if (!float.TryParse(xElement3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("Could not parse " + xElement3.Value + " to a valid float value.");
				return;
			}
			MeleeArmorShreddingBonus = result;
		}
		XElement xElement4 = xElement.Element("Range");
		if (xElement4 == null)
		{
			return;
		}
		XElement xElement5 = xElement4.Element("DodgeMultipliersByDistance");
		DodgeMultiplierByDistance = new Dictionary<int, float>();
		int num = -1;
		foreach (XElement item in xElement5.Elements("Multiplier"))
		{
			XAttribute xAttribute = item.Attribute("StartingDistance");
			XAttribute xAttribute2 = item.Attribute("Value");
			if (!int.TryParse(xAttribute.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse StartingDistance attribute value " + xAttribute.Value + " to a valid integer value.");
				break;
			}
			if (!float.TryParse(xAttribute2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
			{
				CLoggerManager.Log("Could not parse Value attribute value " + xAttribute2.Value + " to a valid integer value.");
				break;
			}
			_ = -1;
			num = result2;
			DodgeMultiplierByDistance.Add(result2, result3);
		}
	}
}

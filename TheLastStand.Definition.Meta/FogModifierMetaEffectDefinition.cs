using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class FogModifierMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "FogModifier";

	public int IncreaseEveryXDays { get; private set; } = -1;

	public int InitialDensityIndex { get; private set; } = -1;

	public FogModifierMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("IncreaseEveryXDays");
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				Debug.LogError("Could not parse " + xElement2.Value + " to a correct value!");
				return;
			}
			IncreaseEveryXDays = result;
		}
		XElement xElement3 = xElement.Element("InitialDensityIndex");
		if (xElement3 != null)
		{
			if (int.TryParse(xElement3.Value, out var result2))
			{
				InitialDensityIndex = result2;
			}
			else
			{
				Debug.LogError("Could not parse " + xElement3.Value + " to a correct value!");
			}
		}
	}
}

using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class WavesParametersMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "WavesParameters";

	public int DistanceMaxFromCenterModifier;

	public WavesParametersMetaEffectDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container != null)
		{
			XElement xElement = (container as XElement).Element("DistanceMaxFromCenterModifier");
			if (xElement != null && int.TryParse(xElement.Value, out var result))
			{
				DistanceMaxFromCenterModifier = result;
			}
		}
	}
}

using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class GenerateLightFogDefinition : BuildingPassiveEffectDefinition
{
	public bool CanLightFogExistOnSelf { get; private set; }

	public GenerateLightFogDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		CanLightFogExistOnSelf = xElement.Element("CanLightFogExistOnSelf") != null;
	}
}

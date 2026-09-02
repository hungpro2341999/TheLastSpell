using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class ImproveSpawnWaveInfoDefinition : BuildingPassiveEffectDefinition
{
	public int Level { get; set; } = 1;

	public ImproveSpawnWaveInfoDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Level = int.Parse(xElement.Element("Level").Value);
	}
}

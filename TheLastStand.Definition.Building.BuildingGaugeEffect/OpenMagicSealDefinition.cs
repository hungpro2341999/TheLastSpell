using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingGaugeEffect;

public class OpenMagicSealDefinition : BuildingGaugeEffectDefinition
{
	public const string Name = "OpenMagicSeal";

	public OpenMagicSealDefinition(XContainer container)
		: base("OpenMagicSeal", container)
	{
	}
}

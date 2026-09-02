using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class GenerateGuardianDefinition : BuildingPassiveEffectDefinition
{
	public GenerateGuardianDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}

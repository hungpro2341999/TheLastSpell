using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class UpdateBonePileLevelDefinition : BuildingPassiveEffectDefinition
{
	public UpdateBonePileLevelDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}

using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class UpdateShopLevelDefinition : BuildingPassiveEffectDefinition
{
	public UpdateShopLevelDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}

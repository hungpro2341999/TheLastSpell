using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImproveUnitLimitDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImproveUnitLimit";

	public ImproveUnitLimitDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
	}
}

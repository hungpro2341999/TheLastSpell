using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImproveGuessWhoDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImproveGuessWho";

	public ImproveGuessWhoDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}
}

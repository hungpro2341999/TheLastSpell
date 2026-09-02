using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class ImproveGuessWho : BuildingUpgradeEffect
{
	public ImproveGuessWhoController ImproveGuessWhoController => base.BuildingUpgradeEffectController as ImproveGuessWhoController;

	public ImproveGuessWhoDefinition ImproveGuessWhoDefinition => base.BuildingUpgradeEffectDefinition as ImproveGuessWhoDefinition;

	public ImproveGuessWho(ImproveGuessWhoDefinition definition, ImproveGuessWhoController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}

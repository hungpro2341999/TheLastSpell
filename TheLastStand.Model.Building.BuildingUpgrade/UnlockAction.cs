using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class UnlockAction : BuildingUpgradeEffect
{
	public UnlockActionController UnlockActionController => base.BuildingUpgradeEffectController as UnlockActionController;

	public UnlockActionDefinition UnlockActionDefinition => base.BuildingUpgradeEffectDefinition as UnlockActionDefinition;

	public UnlockAction(UnlockActionDefinition definition, UnlockActionController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}

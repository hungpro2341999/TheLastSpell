using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class SwapAction : BuildingUpgradeEffect
{
	public SwapActionController SwapActionController => base.BuildingUpgradeEffectController as SwapActionController;

	public SwapActionDefinition SwapActionDefinition => base.BuildingUpgradeEffectDefinition as SwapActionDefinition;

	public SwapAction(SwapActionDefinition definition, SwapActionController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}

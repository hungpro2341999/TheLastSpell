using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class ImproveLevel : BuildingUpgradeEffect
{
	public ImproveLevelController ImproveLevelController => base.BuildingUpgradeEffectController as ImproveLevelController;

	public ImproveLevelDefinition ImproveLevelDefinition => base.BuildingUpgradeEffectDefinition as ImproveLevelDefinition;

	public ImproveLevel(ImproveLevelDefinition definition, ImproveLevelController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}

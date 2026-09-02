using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.BuildingUpgrade;

namespace TheLastStand.Model.Building.BuildingUpgrade;

public class SwapSkill : BuildingUpgradeEffect
{
	public SwapSkillController SwapSkillController => base.BuildingUpgradeEffectController as SwapSkillController;

	public SwapSkillDefinition SwapSkillDefinition => base.BuildingUpgradeEffectDefinition as SwapSkillDefinition;

	public SwapSkill(SwapSkillDefinition definition, SwapSkillController controller, BuildingUpgrade buildingUpgrade)
		: base(definition, controller, buildingUpgrade)
	{
	}
}

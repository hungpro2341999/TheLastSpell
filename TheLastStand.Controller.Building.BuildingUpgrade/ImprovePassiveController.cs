using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class ImprovePassiveController : BuildingUpgradeEffectController
{
	public ImprovePassive ImprovePassive => base.BuildingUpgradeEffect as ImprovePassive;

	public ImprovePassiveController(ImprovePassiveDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImprovePassive(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		for (int num = base.BuildingUpgradeEffect.BuildingUpgrade.Building.PassivesModule.BuildingPassives.Count - 1; num >= 0; num--)
		{
			if (base.BuildingUpgradeEffect.BuildingUpgrade.Building.PassivesModule.BuildingPassives[num].BuildingPassiveDefinition.Id == ImprovePassive.ImprovePassiveDefinition.PassiveId)
			{
				base.BuildingUpgradeEffect.BuildingUpgrade.Building.PassivesModule.BuildingPassives[num].BuildingPassiveController.ImproveEffects(ImprovePassive.ImprovePassiveDefinition.Value.EvalToInt());
			}
		}
	}
}

using TPLib;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class ImproveLevelController : BuildingUpgradeEffectController
{
	public ImproveLevel ImproveLevel => base.BuildingUpgradeEffect as ImproveLevel;

	public ImproveLevelController(ImproveLevelDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveLevel(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		if (!onLoad)
		{
			if (base.BuildingUpgradeEffect.BuildingUpgrade.BuildingUpgradeDefinition.IsGlobal)
			{
				TPSingleton<BuildingManager>.Instance.GlobalItemProductionUpgradeLevel.Level += ImproveLevel.ImproveLevelDefinition.LevelsCount;
			}
			else
			{
				base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.Level += ImproveLevel.ImproveLevelDefinition.LevelsCount;
			}
		}
	}
}

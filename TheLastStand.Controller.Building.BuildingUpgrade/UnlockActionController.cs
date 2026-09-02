using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Building.BuildingAction;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class UnlockActionController : BuildingUpgradeEffectController
{
	public UnlockAction UnlockAction => base.BuildingUpgradeEffect as UnlockAction;

	public UnlockActionController(UnlockActionDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new UnlockAction(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		BuildingActionDefinition buildingActionDefinition = null;
		if (BuildingDatabase.BuildingActionDefinitions.TryGetValue(UnlockAction.UnlockActionDefinition.NewActionId, out var value))
		{
			buildingActionDefinition = value.Clone();
			if (base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule == null)
			{
				base.BuildingUpgradeEffect.BuildingUpgrade.Building.TryCreateEmptyProductionModule();
			}
			if (base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions == null)
			{
				base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions = new List<TheLastStand.Model.Building.BuildingAction.BuildingAction>();
			}
			base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.BuildingActions.Add(new BuildingActionController(buildingActionDefinition, base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule).BuildingAction);
		}
		else
		{
			TPSingleton<BuildingManager>.Instance.LogError("BuildingActionDefinition " + UnlockAction.UnlockActionDefinition.NewActionId + " not found", CLogLevel.MAJOR);
		}
	}
}

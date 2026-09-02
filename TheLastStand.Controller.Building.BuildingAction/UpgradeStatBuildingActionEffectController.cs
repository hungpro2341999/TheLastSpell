using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingAction;

public class UpgradeStatBuildingActionEffectController : BuildingActionEffectController
{
	public UpgradeStatBuildingActionEffect UpgradeStatBuildingActionEffect => base.BuildingActionEffect as UpgradeStatBuildingActionEffect;

	public UpgradeStatBuildingActionEffectController(UpgradeStatBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new UpgradeStatBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.BuildingActionTargeting switch
		{
			BuildingActionEffectDefinition.E_BuildingActionTargeting.All => true, 
			BuildingActionEffectDefinition.E_BuildingActionTargeting.Single => tile.Unit is PlayableUnit, 
			_ => false, 
		};
	}

	public override void ExecuteActionEffect()
	{
		switch (UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.BuildingActionTargeting)
		{
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.All:
		{
			foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				UpgradeStat(playableUnit2, UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.Stat, UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.Bonus);
			}
			break;
		}
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.Single:
			if (base.BuildingActionEffect.Target.Unit is PlayableUnit playableUnit)
			{
				UpgradeStat(playableUnit, UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.Stat, UpgradeStatBuildingActionEffect.UpgradeStatBuildingActionDefinition.Bonus);
			}
			else
			{
				TPSingleton<BuildingManager>.Instance.LogError("Selected Unit is not a playable.");
			}
			break;
		}
	}

	private void UpgradeStat(PlayableUnit playableUnit, UnitStatDefinition.E_Stat stat, int bonus)
	{
		UpgradeStatDisplay pooledComponent = ObjectPooler.GetPooledComponent("UpgradeStatDisplay", ResourcePooler.LoadOnce<UpgradeStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/UpgradeStatDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(stat, bonus);
		playableUnit.PlayableUnitController.AddEffectDisplay(pooledComponent);
		if (bonus >= 0)
		{
			playableUnit.PlayableUnitStatsController.IncreaseBaseStat(stat, bonus, includeChildStat: true);
		}
		else
		{
			playableUnit.PlayableUnitStatsController.DecreaseBaseStat(stat, -bonus, includeChildStat: false);
		}
		GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraitsStats();
		TPSingleton<BuildingManager>.Instance.Log($"UpgradeStat {stat} of {playableUnit.Id} by {bonus}.");
	}
}

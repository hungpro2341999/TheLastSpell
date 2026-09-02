using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization;
using TheLastStand.View.Building.BuildingGaugeEffect;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

public class UpgradeStatGaugeEffectController : BuildingGaugeEffectController
{
	public UpgradeStatGaugeEffectController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new UpgradeStatGaugeEffect(productionBuilding, definition, this, new UpgradeStatView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	public UpgradeStatGaugeEffectController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new UpgradeStatGaugeEffect(productionBuilding, definition, this, new UpgradeStatView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	public override List<IEffectTargetSkillActionController> TriggerEffect()
	{
		List<IEffectTargetSkillActionController> list = base.TriggerEffect();
		UpgradeStatGaugeEffect upgradeStatGaugeEffect = base.BuildingGaugeEffect as UpgradeStatGaugeEffect;
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			UpgradeStatDisplay pooledComponent = ObjectPooler.GetPooledComponent("UpgradeStatDisplay", ResourcePooler.LoadOnce<UpgradeStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/UpgradeStatDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init(upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Stat, upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus);
			playableUnit.PlayableUnitController.AddEffectDisplay(pooledComponent);
			list.Add(playableUnit.PlayableUnitController);
			if (upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus >= 0)
			{
				playableUnit.PlayableUnitStatsController.IncreaseBaseStat(upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Stat, upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus, includeChildStat: true);
			}
			else
			{
				playableUnit.PlayableUnitStatsController.DecreaseBaseStat(upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Stat, -upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus, includeChildStat: false);
			}
		}
		TPSingleton<BuildingManager>.Instance.Log($"({base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id}) UpgradeStat ({upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Bonus} {upgradeStatGaugeEffect.UpgradeStatGaugeEffectDefinition.UpgradeStatDefinition.Stat})", CLogLevel.MAJOR);
		return list;
	}
}

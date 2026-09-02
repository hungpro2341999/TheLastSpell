using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization;
using TheLastStand.View.Building.BuildingGaugeEffect;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

public class GainGoldController : BuildingGaugeEffectController
{
	public GainGold GainGold => base.BuildingGaugeEffect as GainGold;

	public GainGoldController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new GainGold(productionBuilding, definition, this, new GainGoldView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	public GainGoldController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new GainGold(productionBuilding, definition, this, new GainGoldView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	public override List<IEffectTargetSkillActionController> TriggerEffect()
	{
		List<IEffectTargetSkillActionController> list = base.TriggerEffect();
		int num = GainGold.ComputeGoldValue();
		if (Analytics.AllowedToSendData)
		{
			Analytics.SendGoldBuildingEvent(num);
		}
		TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + num);
		GainGoldDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainGoldDisplay", ResourcePooler.LoadOnce<GainGoldDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(num);
		base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		list.Add(base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController);
		TPSingleton<BuildingManager>.Instance.Log($"({base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id}) Gold gain (+{num})", CLogLevel.MAJOR);
		return list;
	}
}

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

public class GainMaterialsController : BuildingGaugeEffectController
{
	private GainMaterials GainMaterials => base.BuildingGaugeEffect as GainMaterials;

	public GainMaterialsController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new GainMaterials(productionBuilding, definition, this, new GainMaterialsView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	public GainMaterialsController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new GainMaterials(productionBuilding, definition, this, new GainMaterialsView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	public override List<IEffectTargetSkillActionController> TriggerEffect()
	{
		List<IEffectTargetSkillActionController> list = base.TriggerEffect();
		int num = GainMaterials.ComputeMaterialsValue();
		if (Analytics.AllowedToSendData)
		{
			Analytics.SendMaterialBuildingEvent(num);
		}
		TPSingleton<ResourceManager>.Instance.Materials += num;
		GainMaterialDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainMaterialDisplay", ResourcePooler.LoadOnce<GainMaterialDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(num);
		base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		list.Add(base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController);
		TPSingleton<BuildingManager>.Instance.Log($"({base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id}) Material gain (+{num})", CLogLevel.MAJOR);
		return list;
	}
}

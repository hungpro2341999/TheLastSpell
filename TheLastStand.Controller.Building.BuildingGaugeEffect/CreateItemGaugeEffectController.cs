using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.Serialization;
using TheLastStand.View.Building.BuildingGaugeEffect;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

public class CreateItemGaugeEffectController : BuildingGaugeEffectController
{
	public static class Constants
	{
		public const string CreateItemDisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/CreateItemDisplay";
	}

	public CreateItemGaugeEffect CreateItem => base.BuildingGaugeEffect as CreateItemGaugeEffect;

	public CreateItemGaugeEffectController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new CreateItemGaugeEffect(productionBuilding, definition, this, new CreateItemView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	public CreateItemGaugeEffectController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new CreateItemGaugeEffect(productionBuilding, definition, this, new CreateItemView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	public override List<IEffectTargetSkillActionController> TriggerEffect()
	{
		List<IEffectTargetSkillActionController> list = base.TriggerEffect();
		CreateItemGaugeEffectDefinition createItemGaugeEffectDefinition = CreateItem.CreateItemGaugeEffectDefinition;
		ProductionItems productionItem = new ProductionItemController(base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition, TPSingleton<BuildingManager>.Instance.GlobalItemProductionUpgradeLevel.Level).ProductionItem;
		productionItem.IsNightProduction = false;
		int prodRewardsCount = TPSingleton<ItemManager>.Instance.ProdRewardsCount;
		for (int i = 0; i < prodRewardsCount; i++)
		{
			TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(ItemSlotDefinition.E_ItemSlotId.None, createItemGaugeEffectDefinition.CreateItemDefinition, CreateItem.GenerationProbabilitiesTree.GenerateLevel());
			productionItem.Items.Add(item);
			TPSingleton<ItemManager>.Instance.Log("(" + base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id + ") Item created: " + item.ItemDefinition.Id + ".", CLogLevel.MAJOR);
		}
		CreateItemDisplay pooledComponent = ObjectPooler.GetPooledComponent("CreateItemDisplay", ResourcePooler.LoadOnce<CreateItemDisplay>("Prefab/Displayable Effect/UI Effect Displays/CreateItemDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(productionItem);
		base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		list.Add(base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController);
		TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.AddProductionObject(productionItem);
		return list;
	}
}

using TPLib;
using TheLastStand.DRM.Achievements;
using TheLastStand.Definition.Building;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.ProductionReport;

namespace TheLastStand.Controller.ProductionReport;

public class ProductionItemController : ProductionObjectController
{
	public ProductionItems ProductionItem => base.ProductionObject as ProductionItems;

	public ProductionItemController(ISerializedData container)
	{
		base.ProductionObject = new ProductionItems(container, this);
	}

	public ProductionItemController(BuildingDefinition productionBuilding = null, int productionLevel = -1, string buildingActionId = "", int createItemIndex = -1)
	{
		base.ProductionObject = new ProductionItems(this, productionBuilding, productionLevel, buildingActionId, createItemIndex);
	}

	public override void ObtainContent()
	{
		if (ProductionItem.Items != null)
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.AddItem(ProductionItem.ChosenItem, null, isNewItem: true);
			if (!ProductionItem.IsNightProduction)
			{
				TPSingleton<MetaConditionManager>.Instance.IncreaseProducedItems(ProductionItem.ChosenItem.ItemDefinition, base.ProductionObject.ProductionBuildingDefinition.Id);
				if ((int)TPSingleton<MetaConditionManager>.Instance.RunContext.ItemsProducedCount >= 40)
				{
					TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_PRODUCE_40_BUILDING_ITEMS_RUN);
				}
			}
		}
		ProductionItem.ChosenItem = null;
	}
}

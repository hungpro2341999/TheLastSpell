using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.Serialization;

namespace TheLastStand.Controller.ProductionReport;

public class ProductionNightRewardObjectController : ProductionObjectController
{
	public ProductionNightRewardObject ProductionNightRewardObject => base.ProductionObject as ProductionNightRewardObject;

	public ProductionNightRewardObjectController(SerializedProductionItems container)
	{
		base.ProductionObject = new ProductionNightRewardObject(container, this);
	}

	public ProductionNightRewardObjectController(TheLastStand.Model.Item.Item[] items, BuildingDefinition productionBuilding = null)
	{
		base.ProductionObject = new ProductionNightRewardObject(this, items, productionBuilding);
	}

	public override void ObtainContent()
	{
		ProductionItems productionItem = new ProductionItemController().ProductionItem;
		productionItem.IsNightProduction = true;
		foreach (TheLastStand.Model.Item.Item item in ProductionNightRewardObject.Items)
		{
			productionItem.Items.Add(item);
		}
		TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.AddProductionObject(productionItem);
	}
}

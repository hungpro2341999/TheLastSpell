using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Serialization;
using TheLastStand.View.ProductionReport;

namespace TheLastStand.Model.ProductionReport;

public class ProductionReport : ISerializable, IDeserializable
{
	public ProductionReportController ProductionReportController { get; private set; }

	public List<ProductionObject> ProducedObjects { get; set; } = new List<ProductionObject>();

	public ProductionReportPanel ProductionReportPanel { get; }

	public ProductionReport(SerializedProductionReport container, ProductionReportController controller, ProductionReportPanel view)
	{
		ProductionReportController = controller;
		ProductionReportPanel = view;
		Deserialize(container);
	}

	public ProductionReport(ProductionReportController controller, ProductionReportPanel view)
	{
		ProductionReportController = controller;
		ProductionReportPanel = view;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedProductionReport serializedProductionReport = container as SerializedProductionReport;
		if (serializedProductionReport.ProductionItems != null)
		{
			foreach (SerializedProductionItems productionItem2 in serializedProductionReport.ProductionItems)
			{
				ProductionItems productionItem = new ProductionItemController(productionItem2).ProductionItem;
				ProducedObjects.Add(productionItem);
			}
			PanicManager.Panic.PanicReward.BaseNbRerollReward = serializedProductionReport.BaseRewardReroll;
			PanicManager.Panic.PanicReward.RemainingNbRerollReward = serializedProductionReport.RemainingRewardReroll;
		}
		if (serializedProductionReport.ProductionGolds != null)
		{
			foreach (SerializedProducedResource productionGold in serializedProductionReport.ProductionGolds)
			{
				TPSingleton<ResourceManager>.Instance.BackwardCompatibilityGold += productionGold.Value;
			}
		}
		if (serializedProductionReport.ProductionMaterials == null)
		{
			return;
		}
		foreach (SerializedProducedResource productionMaterial in serializedProductionReport.ProductionMaterials)
		{
			TPSingleton<ResourceManager>.Instance.BackwardCompatibilityMaterials += productionMaterial.Value;
		}
	}

	public ISerializedData Serialize()
	{
		SerializedProductionReport serializedProductionReport = new SerializedProductionReport
		{
			ProductionGolds = new List<SerializedProducedResource>(),
			ProductionItems = new List<SerializedProductionItems>(),
			ProductionMaterials = new List<SerializedProducedResource>(),
			BaseRewardReroll = PanicManager.Panic.PanicReward.BaseNbRerollReward,
			RemainingRewardReroll = PanicManager.Panic.PanicReward.RemainingNbRerollReward
		};
		foreach (ProductionObject producedObject in ProducedObjects)
		{
			if (producedObject is ProductionItems productionItems)
			{
				serializedProductionReport.ProductionItems.Add(productionItems.Serialize() as SerializedProductionItems);
			}
		}
		return serializedProductionReport;
	}
}

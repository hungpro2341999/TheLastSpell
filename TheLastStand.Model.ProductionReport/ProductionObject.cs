using TPLib.Localization;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Definition.Building;
using TheLastStand.View.ProductionReport;

namespace TheLastStand.Model.ProductionReport;

public abstract class ProductionObject
{
	public BuildingDefinition ProductionBuildingDefinition { get; set; }

	public ProductionObjectController ProductionObjectController { get; protected set; }

	public ProductionObjectDisplay ProductionObjectView { get; set; }

	public string Name
	{
		get
		{
			if (ProductionBuildingDefinition == null)
			{
				return Localizer.Get("NightReportPanel_NightRewardObject");
			}
			return ProductionBuildingDefinition.Name;
		}
	}

	public ProductionObject(ProductionObjectController productionObjectController)
	{
		ProductionObjectController = productionObjectController;
	}

	public ProductionObject(ProductionObjectController productionObjectController, BuildingDefinition productionBuildingDefinition)
	{
		ProductionObjectController = productionObjectController;
		ProductionBuildingDefinition = productionBuildingDefinition;
	}
}

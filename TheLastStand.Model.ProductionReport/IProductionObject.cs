using TheLastStand.Definition.Building;

namespace TheLastStand.Model.ProductionReport;

public interface IProductionObject
{
	BuildingDefinition ProductionBuilding { get; set; }

	void ObtainContent();
}

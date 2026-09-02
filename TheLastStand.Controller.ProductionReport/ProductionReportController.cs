using TPLib;
using TheLastStand.Model.ProductionReport;
using TheLastStand.Serialization;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.ToDoList;

namespace TheLastStand.Controller.ProductionReport;

public class ProductionReportController
{
	public TheLastStand.Model.ProductionReport.ProductionReport ProductionReport { get; private set; }

	public ProductionReportController(SerializedProductionReport container, ProductionReportPanel view)
	{
		ProductionReport = new TheLastStand.Model.ProductionReport.ProductionReport(container, this, view);
	}

	public ProductionReportController(ProductionReportPanel view)
	{
		ProductionReport = new TheLastStand.Model.ProductionReport.ProductionReport(this, view);
	}

	public void AddProductionObject(ProductionObject productionObject)
	{
		ProductionReport.ProducedObjects.Add(productionObject);
		TPSingleton<ToDoListView>.Instance.RefreshProductionNotification();
	}

	public void RemoveProductionObject(ProductionObject productionObject)
	{
		ProductionReport.ProducedObjects.Remove(productionObject);
		TPSingleton<ToDoListView>.Instance.RefreshProductionNotification();
		TPSingleton<ToDoListView>.Instance.RefreshInventoryNotification();
	}
}

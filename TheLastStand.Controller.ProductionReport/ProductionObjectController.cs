using TheLastStand.Model.ProductionReport;

namespace TheLastStand.Controller.ProductionReport;

public abstract class ProductionObjectController
{
	protected ProductionObject ProductionObject { get; set; }

	public abstract void ObtainContent();
}

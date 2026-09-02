using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.View.ProductionReport;

namespace TheLastStand.Controller.ProductionReport;

public class RewardItemSlotController : ItemSlotController
{
	public RewardItemSlot RewardItemSlot { get; private set; }

	public RewardItemSlotController(ItemSlotDefinition definition, RewardItemSlotView view)
		: base(definition, view)
	{
		RewardItemSlot = new RewardItemSlot(definition, this, view);
	}
}

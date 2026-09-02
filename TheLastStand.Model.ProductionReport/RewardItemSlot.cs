using TheLastStand.Controller.ProductionReport;
using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.View.ProductionReport;

namespace TheLastStand.Model.ProductionReport;

public class RewardItemSlot : ItemSlot
{
	public RewardItemSlotController RewardItemSlotController { get; private set; }

	public ItemSlotDefinition RewardItemSlotDefinition { get; private set; }

	public RewardItemSlotView RewardItemSlotView { get; private set; }

	public RewardItemSlot(ItemSlotDefinition definition, RewardItemSlotController controller, RewardItemSlotView view)
		: base(definition, controller, view)
	{
	}
}

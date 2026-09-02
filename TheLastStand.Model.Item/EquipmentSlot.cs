using TheLastStand.Controller.Item;
using TheLastStand.Definition.Item;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Item;
using TheLastStand.View.CharacterSheet;

namespace TheLastStand.Model.Item;

public class EquipmentSlot : ItemSlot
{
	private EquipmentSlot blockOtherSlot;

	public EquipmentSlot BlockedByOtherSlot { get; set; }

	public EquipmentSlot BlockOtherSlot
	{
		get
		{
			return blockOtherSlot;
		}
		set
		{
			if (blockOtherSlot != value)
			{
				if (blockOtherSlot != null)
				{
					blockOtherSlot.BlockedByOtherSlot = null;
				}
				blockOtherSlot = value;
				if (value != null)
				{
					blockOtherSlot.BlockedByOtherSlot = this;
				}
			}
		}
	}

	public EquipmentSlotController EquipmentSlotController => base.ItemSlotController as EquipmentSlotController;

	public EquipmentSlotView EquipmentSlotView
	{
		get
		{
			return base.ItemSlotView as EquipmentSlotView;
		}
		set
		{
			base.ItemSlotView = value;
		}
	}

	public PlayableUnit PlayableUnit { get; }

	public EquipmentSlot(SerializedItemSlot container, ItemSlotDefinition itemSlotDefinition, EquipmentSlotController equipmentSlotController, EquipmentSlotView equipmentSlotView, PlayableUnit unit)
		: base(itemSlotDefinition, equipmentSlotController, equipmentSlotView)
	{
		PlayableUnit = unit;
		Deserialize(container);
	}

	public EquipmentSlot(ItemSlotDefinition itemSlotDefinition, EquipmentSlotController equipmentSlotController, EquipmentSlotView equipmentSlotView, PlayableUnit unit)
		: base(itemSlotDefinition, equipmentSlotController, equipmentSlotView)
	{
		PlayableUnit = unit;
	}
}

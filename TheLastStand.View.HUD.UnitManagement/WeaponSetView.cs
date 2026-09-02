using TheLastStand.Definition.Item;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.View.HUD.UnitManagement;

public class WeaponSetView : MonoBehaviour
{
	[SerializeField]
	private EquipmentBoxSlotView rightHandSlotView;

	[SerializeField]
	private EquipmentBoxSlotView leftHandSlotView;

	private PlayableUnit playableUnit;

	private bool useEquippedSlots;

	public void RefreshEquipmentSlots(PlayableUnit playableUnit, bool useEquippedSlots)
	{
		this.playableUnit = playableUnit;
		this.useEquippedSlots = useEquippedSlots;
		RefreshEquipmentSlots();
	}

	public void RefreshEquipmentSlots()
	{
		if (playableUnit == null)
		{
			rightHandSlotView.Refresh(null);
			leftHandSlotView.Refresh(null);
			return;
		}
		int num = (useEquippedSlots ? playableUnit.EquippedWeaponSetIndex : ((playableUnit.EquippedWeaponSetIndex + 1) % 2));
		if (playableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.RightHand, out var value))
		{
			TheLastStand.Model.Item.Item item = ((num < value.Count) ? value[num].Item : null);
			rightHandSlotView.Refresh(item);
		}
		else
		{
			rightHandSlotView.Refresh(null);
		}
		int num2;
		if (value != null && num < value.Count)
		{
			EquipmentSlot equipmentSlot = value[num];
			num2 = ((equipmentSlot != null && equipmentSlot.Item?.IsTwoHandedWeapon == true) ? 1 : 0);
		}
		else
		{
			num2 = 0;
		}
		bool flag = (byte)num2 != 0;
		ItemSlotDefinition.E_ItemSlotId key = (flag ? ItemSlotDefinition.E_ItemSlotId.RightHand : ItemSlotDefinition.E_ItemSlotId.LeftHand);
		if (playableUnit.EquipmentSlots.TryGetValue(key, out var value2))
		{
			TheLastStand.Model.Item.Item item2 = ((num < value2.Count) ? value2[num].Item : null);
			leftHandSlotView.Refresh(item2, flag);
		}
		else
		{
			leftHandSlotView.Refresh(null);
		}
	}
}

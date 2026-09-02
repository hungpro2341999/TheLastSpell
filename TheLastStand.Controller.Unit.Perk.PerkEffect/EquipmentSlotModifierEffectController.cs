using System;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Item;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Manager;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.View.CharacterSheet;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class EquipmentSlotModifierEffectController : APerkEffectController
{
	public EquipmentSlotModifierEffect EquipmentSlotModifierEffect => base.PerkEffect as EquipmentSlotModifierEffect;

	public TheLastStand.Model.Unit.Perk.Perk Perk => base.PerkEffect.APerkModule.Perk;

	public EquipmentSlotModifierEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	public override void OnUnlock(bool onLoad)
	{
		if (!onLoad)
		{
			List<EquipmentSlotView> availableViews = CharacterSheetPanel.EquipmentSlots[EquipmentSlotModifierEffect.EquipmentSlotModifierEffectDefinition.SlotId];
			int num = EquipmentSlotModifierEffect.EquipmentSlotModifierEffectDefinition.ValueExpression.EvalToInt(Perk);
			if (!Perk.Owner.EquipmentSlots.ContainsKey(EquipmentSlotModifierEffect.EquipmentSlotModifierEffectDefinition.SlotId) && num > 0)
			{
				Perk.Owner.EquipmentSlots.Add(EquipmentSlotModifierEffect.EquipmentSlotModifierEffectDefinition.SlotId, new List<EquipmentSlot>());
			}
			UnitTraitDefinition.SlotModifier slotModifier = new UnitTraitDefinition.SlotModifier(EquipmentSlotModifierEffect.EquipmentSlotModifierEffectDefinition.SlotId, num, num > 0, string.Empty);
			if (slotModifier.AddSlot)
			{
				AddSlot(slotModifier, availableViews);
			}
			else
			{
				RemoveSlot(slotModifier, availableViews);
			}
		}
	}

	public override void Lock(bool onLoad)
	{
		List<EquipmentSlotView> availableViews = CharacterSheetPanel.EquipmentSlots[EquipmentSlotModifierEffect.EquipmentSlotModifierEffectDefinition.SlotId];
		int num = EquipmentSlotModifierEffect.EquipmentSlotModifierEffectDefinition.ValueExpression.EvalToInt(Perk) * -1;
		if (Perk.Owner.EquipmentSlots.ContainsKey(EquipmentSlotModifierEffect.EquipmentSlotModifierEffectDefinition.SlotId))
		{
			UnitTraitDefinition.SlotModifier slotModifier = new UnitTraitDefinition.SlotModifier(EquipmentSlotModifierEffect.EquipmentSlotModifierEffectDefinition.SlotId, num, num > 0, string.Empty);
			if (slotModifier.AddSlot)
			{
				AddSlot(slotModifier, availableViews);
			}
			else
			{
				RemoveSlot(slotModifier, availableViews);
			}
		}
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new EquipmentSlotModifierEffect(aPerkEffectDefinition as EquipmentSlotModifierEffectDefinition, this, aPerkModule);
	}

	private void AddSlot(UnitTraitDefinition.SlotModifier slotModifier, List<EquipmentSlotView> availableViews)
	{
		for (int i = 0; i < Mathf.Abs(slotModifier.Amount); i++)
		{
			if (Perk.Owner.EquipmentSlots.ContainsKey(slotModifier.Name) && Perk.Owner.EquipmentSlots[slotModifier.Name].Count < availableViews.Count)
			{
				EquipmentSlot equipmentSlot = new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[slotModifier.Name], availableViews[Perk.Owner.EquipmentSlots[slotModifier.Name].Count], Perk.Owner).EquipmentSlot;
				Perk.Owner.EquipmentSlots[slotModifier.Name].Add(equipmentSlot);
				equipmentSlot.EquipmentSlotView.ItemSlot = equipmentSlot;
				equipmentSlot.EquipmentSlotView.Refresh();
			}
		}
	}

	private void RemoveSlot(UnitTraitDefinition.SlotModifier slotModifier, List<EquipmentSlotView> availableViews)
	{
		bool flag = ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(slotModifier.Name);
		int equippedWeaponSetIndex = Perk.Owner.EquippedWeaponSetIndex;
		if (flag && Perk.Owner.EquippedWeaponSetIndex == 1)
		{
			Perk.Owner.PlayableUnitController.SwitchWeaponSet();
		}
		if (!Perk.Owner.EquipmentSlots.TryGetValue(slotModifier.Name, out var value) || (flag && value.Count == 1))
		{
			return;
		}
		for (int i = 0; i < Math.Abs(slotModifier.Amount); i++)
		{
			if (value.Count == 0)
			{
				break;
			}
			EquipmentSlot equipmentSlot = value[value.Count - 1];
			if (flag && equippedWeaponSetIndex == 1 && value[0].Item != null)
			{
				if (value[0].BlockOtherSlot != null)
				{
					value[0].BlockOtherSlot.BlockedByOtherSlot = null;
					value[0].BlockOtherSlot = null;
				}
				value[0].ItemSlotController.SwapItems();
			}
			if (equipmentSlot.Item != null)
			{
				if (equippedWeaponSetIndex == 1 && flag)
				{
					Perk.Owner.PlayableUnitController.EquipItem(equipmentSlot.Item, value[0]);
				}
				else
				{
					equipmentSlot.EquipmentSlotController.SwapItems();
				}
			}
			equipmentSlot.EquipmentSlotView.ItemSlot = null;
			value.ForEach(delegate(EquipmentSlot x)
			{
				x.EquipmentSlotView.Refresh();
			});
			value.Remove(equipmentSlot);
		}
		if (flag)
		{
			Perk.Owner.PlayableUnitView.RefreshSnapshot();
			return;
		}
		Perk.Owner.PlayableUnitView.RefreshBodyParts();
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		if (selectedPlayableUnit != null && selectedPlayableUnit.Id == Perk.Owner.Id && TPSingleton<CharacterSheetPanel>.Instance.IsOpened)
		{
			TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar(selectedPlayableUnit);
		}
	}
}
